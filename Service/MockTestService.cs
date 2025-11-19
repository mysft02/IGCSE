using AutoMapper;
using BusinessObject.Model;
using BusinessObject.Payload.Request.OpenAI;
using BusinessObject.Payload.Request.OpenApi;
using Common.Constants;
using Repository.IRepositories;
using Service.OpenAI;
using BusinessObject.DTOs.Response.MockTest;
using BusinessObject.DTOs.Response;
using Microsoft.AspNetCore.Hosting;
using BusinessObject.DTOs.Request.MockTest;
using Common.Utils;
using System.Text.RegularExpressions;
using BusinessObject.Payload.Response.Trello;
using BusinessObject.DTOs.Request.Packages;
using BusinessObject.DTOs.Response.Packages;
using Repository.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace Service
{
    public class MockTestService
    {
        private readonly IMapper _mapper;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly OpenAIApiService _openAIApiService;
        private readonly IMockTestQuestionRepository _questionRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly MediaService _mediaService;
        private readonly IWebHostEnvironment _env;
        private readonly IMockTestUserAnswerRepository _userAnswerRepository;
        private readonly IPackageRepository _packageRepository;

        public MockTestService(
            IMapper mapper, 
            IMockTestRepository mockTestRepository,
            OpenAIApiService openAIApiService,
            IMockTestQuestionRepository questionRepository,
            IMockTestResultRepository mockTestResultRepository,
            MediaService mediaService,
            IWebHostEnvironment env,
            IMockTestUserAnswerRepository userAnswerRepository,
            IPackageRepository packageRepository)
        {
            _mapper = mapper;
            _mockTestRepository = mockTestRepository;
            _openAIApiService = openAIApiService;
            _questionRepository = questionRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _mediaService = mediaService;
            _env = env;
            _userAnswerRepository = userAnswerRepository;
            _packageRepository = packageRepository;
        }

        public async Task<Mocktest> CreateMockTestForTrelloAsync(string mockTestName, List<TrelloCardResponse> trelloCardResponses, string userId)
        {
            mockTestName = mockTestName.Trim();
            string description = "This is a mock test imported from Trello.";

            foreach (var trelloCardResponse in trelloCardResponses)
            {
                if (trelloCardResponse.Name.Contains("Description"))
                {
                    var parts = trelloCardResponse.Name.Split(':', 2);
                    description = parts.Length > 1 ? parts[1].Trim() : trelloCardResponse.Name.Trim();
                }
            }

            var mockTest = new Mocktest
            {
                MockTestTitle = mockTestName,
                MockTestDescription = description,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdMockTest = await _mockTestRepository.AddAsync(mockTest);
            return createdMockTest;
        }

        public async Task<BaseResponse<PaginatedResponse<MockTestQueryResponse>>> GetAllMockTestAsync(MockTestQueryRequest request)
        {
            // Build filter expression
            var filter = request.BuildFilter<Mocktest>();

            // Get total count first (for pagination info)
            var totalCount = await _mockTestRepository.CountAsync(filter);

            // Get filtered data with pagination
            var items = await _mockTestRepository.FindWithPagingAsync(
            filter,
                request.Page,
                request.GetPageSize()
            );

            // Apply sorting to the paged results
            var sortedItems = request.ApplySorting(items);

            var itemList = sortedItems
                .Select(token => new MockTestQueryResponse
                {
                    MockTestId = token.MockTestId,
                    MockTestTitle = token.MockTestTitle,
                    MockTestDescription = token.MockTestDescription,
                    CreatedAt = token.CreatedAt,
                    UpdatedAt = token.UpdatedAt,
                    CreatedBy = token.CreatedBy,
                    Status = request.userID == null ? MockTestStatusEnum.Locked : _mockTestRepository.CheckMockTestDone(token.MockTestId, request.userID)
                })
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.GetPageSize());

            // Map to response
            return new BaseResponse<PaginatedResponse<MockTestQueryResponse>>
            {
                Data = new PaginatedResponse<MockTestQueryResponse>
                {
                    Items = itemList,
                    TotalCount = totalCount,
                    Page = request.Page,
                    Size = request.GetPageSize(),
                    TotalPages = totalPages
                },
                Message = "Lấy mock test thành công",
                StatusCode = StatusCodeEnum.OK_200
            };
        }

        public async Task<BaseResponse<PaginatedResponse<MockTestResultQueryResponse>>> GetAllMockTestResultAsync(MockTestResultQueryRequest request)
        {
            var filter = request.BuildFilter<Mocktestresult>();

            // Get total count first (for pagination info)
            var totalCount = await _mockTestResultRepository.CountAsync(filter);

            // Get filtered items
            var items = await _mockTestResultRepository.GetMockTestResultList(request, filter);

            // Apply sorting to the paged results
            var sortedItems = request.ApplySorting(items);

            // Apply pagination after sorting
            var pagedItems = sortedItems
                .Skip(request.Page * request.GetPageSize())
                .Take(request.GetPageSize())
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.GetPageSize());

            // Map to response
            return new BaseResponse<PaginatedResponse<MockTestResultQueryResponse>>
            {
                Data = new PaginatedResponse<MockTestResultQueryResponse>
                {
                    Items = pagedItems,
                    TotalCount = totalCount,
                    Page = request.Page,
                    Size = request.GetPageSize(),
                    TotalPages = totalPages
                },
                Message = "Lấy toàn bộ kết quả mock test thành công",
                StatusCode = StatusCodeEnum.OK_200
            };
        }

        public async Task<BaseResponse<MockTestResultReviewResponse>> GetMockTestResultReviewByIdAsync(int id)
        {
            var item = await _mockTestResultRepository.GetMockTestResultWithReview(id);

            // Map to response
            return new BaseResponse<MockTestResultReviewResponse>
            {
                Data = item,
                Message = "Lấy kết quả mock test thành công",
                StatusCode = StatusCodeEnum.OK_200
            };
        }

        public async Task<BaseResponse<MockTestForStudentResponse>> GetMockTestByIdAsync(int mockTestId, string userId)
        {
            var package = await _packageRepository.GetByUserId(userId);

            if(package == null || package.IsMockTest == false)
            {
                throw new Exception("Bạn chưa đăng kí gói thi thử.");
            }

            var mockTest = await _mockTestRepository.GetByMockTestIdAsync(mockTestId);
            if (mockTest == null)
            {
                throw new Exception("Bài thi thử không tìm thấy");
            }

            return new BaseResponse<MockTestForStudentResponse>(
                "Lấy mock test thành công",
                StatusCodeEnum.OK_200,
                mockTest
            );
        }

        public async Task<BaseResponse<List<MockTestMarkResponse>>> MarkMockTestAsync(MockTestMarkRequest request, string userId)
        {
            List<MockTestMarkResponse> result = new List<MockTestMarkResponse>();
            List<MockTestQuestionMarkRequest> questions = new List<MockTestQuestionMarkRequest>();

            // Validate input
            if (request?.UserAnswers == null || request.UserAnswers.Count == 0)
            {
                return new BaseResponse<List<MockTestMarkResponse>>(
                    "Không có câu hỏi nào để chấm",
                    StatusCodeEnum.OK_200,
                    new List<MockTestMarkResponse>());
            }

            var mockTestResultCreate = new Mocktestresult
            {
                MockTestId = request.MockTestId,
                UserId = userId,
                Score = 0,
                CreatedAt = DateTime.UtcNow
            };

            var mocktestResult = await _mockTestResultRepository.AddAsync(mockTestResultCreate);

            foreach (var userAnswer in request.UserAnswers)
            {
                if (userAnswer == null)
                    continue;

                var questionText = await _questionRepository.GetByIdAsync(userAnswer.QuestionId);
                if (questionText == null)
                    continue;

                var questionRequest = new MockTestQuestionMarkRequest
                {
                    QuestionText = questionText.QuestionContent,
                    Answer = userAnswer.Answer ?? string.Empty,
                    RightAnswer = questionText.CorrectAnswer,
                    Mark = (decimal)questionText.Mark,
                    PartialMark = questionText.PartialMark,
                    QuestionId = questionText.MockTestQuestionId
                };

                questions.Add(questionRequest);
            }

            var contentText = string.Join("\n", questions.Select(q =>
                $"Question: {q.QuestionText}\nAnswer: {q.Answer}\nRight answer: {q.RightAnswer}\nMark: {q.Mark}\nPartial mark: {q.PartialMark}"));

            var input = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "input_text",
                            text = $"Bạn là hệ thống chấm điểm. Với danh sách câu hỏi dưới đây, hãy CHẤM ĐIỂM THEO Partial mark nếu có, còn nếu không thì chỉ cần chấm đúng sai rồi cho điểm theo Mark, xét cả câu trả lời và ảnh (nếu có). Với mỗi câu, hãy trả về đúng một mục theo định dạng máy đọc dễ dàng: score=<điểm>||comment=<nhận xét ngắn <100 chữ>. Không xuống dòng trong comment>.\nCác mục của các câu phải được nối bằng chuỗi phân tách: |||\nSau khi liệt kê xong, in thêm mục cuối: TOTAL=<tổng điểm>.\nKHÔNG trả về thêm bất kỳ chữ nào khác ngoài đúng định dạng đã nêu.\n\nDỮ LIỆU:\n{contentText}"
                        }
                    }
                }
            };

            var body = new OpenAIBody
            {
                Model = "gpt-4.1-mini",
                Input = input,
                Temperature = 0.5,
                MaxTokens = 800
            };
            var apiRequest = OpenApiRequest.Builder()
                .CallUrl("/responses")
                .Body(body)
                .Build();

            var response = await _openAIApiService.PostAsync<OpenAIBody, OpenAIResponse>(apiRequest, body);

            if (response?.Output == null || response.Output.Count == 0)
                return new BaseResponse<List<MockTestMarkResponse>>(
                    "Mock test marked successfully",
                    StatusCodeEnum.OK_200,
                    new List<MockTestMarkResponse>());

            // 🧩 Ghép output text
            var outputText = string.Join("\n",
                response.Output
                    .SelectMany(o => o.Content)
                    .Where(c => c.Type == "output_text")
                    .Select(c => c.Text));

            if (string.IsNullOrWhiteSpace(outputText))
                throw new Exception("OpenAI trả về rỗng (outputText empty)");

            var cleaned = outputText.Replace("```", string.Empty).Trim();
            var parts = cleaned.Split("|||", StringSplitOptions.RemoveEmptyEntries)
                               .Select(p => p.Trim())
                               .ToList();

            if (parts.Count < questions.Count)
                while (parts.Count < questions.Count) parts.Add(string.Empty);
            else if (parts.Count > questions.Count)
                parts = parts.Take(questions.Count).ToList();

            static string Normalize(string? s)
            {
                return (s ?? string.Empty).Trim().ToLowerInvariant().Replace(" ", string.Empty);
            }

            decimal finalScore = 0;
            decimal? totalFromAi = null;

            // Nếu AI có mục TOTAL, tách ra
            for (int idx = parts.Count - 1; idx >= 0; idx--)
            {
                var p = parts[idx];
                var mTotal = Regex.Match(p, @"^TOTAL\s*=\s*([0-9]+(\.[0-9]+)?)$", RegexOptions.IgnoreCase);
                if (mTotal.Success)
                {
                    if (decimal.TryParse(mTotal.Groups[1].Value, out var tot))
                    {
                        totalFromAi = tot;
                    }
                    parts.RemoveAt(idx);
                }
            }

            for (int i = 0; i < questions.Count; i++)
            {
                var q = questions[i];
                var piece = parts[i];

                decimal score = 0;
                string comment = piece;

                // Parse score theo dạng score=...||comment=...
                var m = Regex.Match(piece, @"score\s*=\s*([0-9]+(\.[0-9]+)?)\s*\|\|\s*comment\s*=\s*(.*)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (m.Success)
                {
                    if (decimal.TryParse(m.Groups[1].Value, out var s))
                    {
                        score = s;
                    }
                    comment = m.Groups[3].Value.Trim();
                }

                bool isCorrect = score >= 0.999m || Normalize(q.Answer) == Normalize(q.RightAnswer);
                result.Add(new MockTestMarkResponse
                {
                    Question = q.QuestionText,
                    Answer = q.Answer,
                    RightAnswer = q.RightAnswer,
                    Score = score,
                    IsCorrect = isCorrect,
                    Comment = comment
                });
                finalScore += score;

                var userAnswer = new Mocktestuseranswer
                {
                    MockTestResultId = mocktestResult.MockTestResultId,
                    MockTestQuestionId = q.QuestionId,
                    Answer = q.Answer,
                    Score = score,
                };

                await _userAnswerRepository.AddAsync(userAnswer);
            }

            if (totalFromAi.HasValue)
            {
                finalScore = totalFromAi.Value;
            }

            // Cập nhật điểm tổng vào kết quả
            mocktestResult.Score = finalScore;
            await _mockTestResultRepository.UpdateAsync(mocktestResult);

            return new BaseResponse<List<MockTestMarkResponse>>(
                $"Chấm bài mock test thành công",
                StatusCodeEnum.OK_200,
                result);
        }
    }
}
