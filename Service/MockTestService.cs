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
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Common.Utils;
using DocumentFormat.OpenXml.Vml.Spreadsheet;

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


        public async Task<BaseResponse<PaginatedResponse<MockTestQueryResponse>>> GetAllMockTestAsync(MockTestQueryRequest request, string userRole)
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
                    Status = request.userID == null ? MockTestStatusEnum.Locked : _mockTestRepository.CheckMockTestStatus(token.MockTestId, request.userID, userRole)
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

        public async Task<BaseResponse<object>> GetMockTestByIdAsync(int mockTestId, string userId, string userRole)
        {
            if(userRole == "Student")
            {
                var package = await _packageRepository.GetByUserId(userId);

                if (package == null || package.IsMockTest == false)
                {
                    throw new Exception("Bạn chưa đăng kí gói thi thử.");
                }

                var mockTest = await _mockTestRepository.GetByMockTestIdAsync(mockTestId);
                if (mockTest == null)
                {
                    throw new Exception("Bài thi thử không tìm thấy");
                }

                return new BaseResponse<object>(
                    "Lấy mock test thành công",
                    StatusCodeEnum.OK_200,
                    mockTest
                    );
            }
            var mocktestWithResult = await _mockTestRepository.GetMockTestResultWithReviewAsync(mockTestId);
            if(mocktestWithResult == null)
            {
                throw new Exception("Không tìm thấy bài thi thử cần tìm");
            }

            return new BaseResponse<object>(
                    "Lấy mock test thành công",
                    StatusCodeEnum.OK_200,
                    mocktestWithResult
                    );
        }


        public async Task<BaseResponse<MockTestResultReviewResponse>> GetMockTestResultAsync(int mockTestId, string userId)
        {
            var status = _mockTestRepository.CheckMockTestDone(mockTestId, userId);
            if (status != MockTestStatusEnum.Completed)
            {
                throw new Exception("Bạn chưa hoàn thành bài thi này.");
            }

            var latestResultId = await _mockTestResultRepository.GetLatestMockTestResultIdAsync(mockTestId, userId);
            if (!latestResultId.HasValue)
            {
                throw new Exception("Không tìm thấy kết quả bài thi.");
            }

            // Trả về review
            return await GetMockTestResultReviewByIdAsync(latestResultId.Value);
        }


        public async Task<BaseResponse<List<MockTestMarkResponse>>> MarkMockTestAsync(MockTestMarkRequest request, string userId)
        {
            List<MockTestMarkResponse> result = new List<MockTestMarkResponse>();

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

            // Rule-Based Pre-Filtering: Phân loại câu hỏi thành 2 nhóm
            var ruleBasedResults = new List<(int QuestionId, string QuestionText, string Answer, string RightAnswer, decimal Mark, string? PartialMark, bool IsCorrect, decimal Score, string Comment)>();
            var questionsNeedingAI = new List<(int QuestionId, MockTestQuestionMarkRequest Question)>();
            var userAnswerEntities = new Dictionary<int, Mocktestuseranswer>(); // Mapping QuestionId -> UserAnswer entity

            foreach (var userAnswer in request.UserAnswers)
            {
                if (userAnswer == null)
                    continue;

                var questionText = await _questionRepository.GetByIdAsync(userAnswer.QuestionId);
                if (questionText == null)
                    continue;

                var userAnswerText = userAnswer.Answer ?? string.Empty;
                var correctAnswer = questionText.CorrectAnswer;
                var mark = (decimal)questionText.Mark;
                var partialMark = questionText.PartialMark;

                // Rule-Based Pre-Filtering: Kiểm tra xem có thể chấm điểm bằng rule không
                // Với MockTest, nếu có PartialMark thì luôn cần AI để chấm chi tiết
                // Chỉ dùng rule-based khi không có PartialMark
                bool needsAI = !string.IsNullOrWhiteSpace(partialMark);
                
                if (!needsAI)
                {
                    var (canUseRuleBased, isCorrect, similarity) = CommonUtils.CanUseRuleBasedScoring(
                        userAnswerText, 
                        correctAnswer, 
                        similarityThreshold: 0.9m);

                    if (canUseRuleBased)
                    {
                        // Sử dụng Rule-Based Scoring (không cần gọi OpenAI)
                        decimal score = isCorrect ? mark : 0m;
                        var comment = isCorrect 
                            ? "Câu trả lời chính xác." 
                            : $"Câu trả lời chưa chính xác. Đáp án đúng: {correctAnswer}";
                        
                        ruleBasedResults.Add((
                            questionText.MockTestQuestionId, 
                            questionText.QuestionContent, 
                            userAnswerText, 
                            correctAnswer, 
                            mark,
                            partialMark,
                            isCorrect, 
                            score, 
                            comment));
                    }
                    else
                    {
                        // Cần AI để chấm điểm (câu trả lời phức tạp, không rõ ràng)
                        var questionRequest = new MockTestQuestionMarkRequest
                        {
                            QuestionText = questionText.QuestionContent,
                            Answer = userAnswerText,
                            RightAnswer = correctAnswer,
                            Mark = mark,
                            PartialMark = partialMark,
                            QuestionId = questionText.MockTestQuestionId
                        };
                        questionsNeedingAI.Add((questionText.MockTestQuestionId, questionRequest));
                    }
                }
                else
                {
                    // Có PartialMark, luôn cần AI để chấm chi tiết
                    var questionRequest = new MockTestQuestionMarkRequest
                    {
                        QuestionText = questionText.QuestionContent,
                        Answer = userAnswerText,
                        RightAnswer = correctAnswer,
                        Mark = mark,
                        PartialMark = partialMark,
                        QuestionId = questionText.MockTestQuestionId
                    };
                    questionsNeedingAI.Add((questionText.MockTestQuestionId, questionRequest));
                }

                // Lưu user answer vào database
                var userAnswerEntity = new Mocktestuseranswer
                {
                    MockTestResultId = mocktestResult.MockTestResultId,
                    MockTestQuestionId = questionText.MockTestQuestionId,
                    Answer = userAnswerText,
                    Score = 0, // Sẽ cập nhật sau khi chấm xong
                };
                var savedUserAnswer = await _userAnswerRepository.AddAsync(userAnswerEntity);
                userAnswerEntities[questionText.MockTestQuestionId] = savedUserAnswer;
            }

            // Chỉ gọi OpenAI nếu có câu hỏi cần AI scoring
            List<(int QuestionId, string QuestionText, string Answer, string RightAnswer, decimal Score, bool IsCorrect, string Comment)> aiResults = new List<(int, string, string, string, decimal, bool, string)>();
            if (questionsNeedingAI.Any())
            {
                var contentText = string.Join("\n", questionsNeedingAI.Select(q =>
                    $"Question: {q.Question.QuestionText}\nAnswer: {q.Question.Answer}\nRight answer: {q.Question.RightAnswer}\nMark: {q.Question.Mark}\nPartial mark: {q.Question.PartialMark}"));

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

                if (response?.Output != null && response.Output.Count > 0)
                {
                    // 🧩 Ghép output text
                    var outputText = string.Join("\n",
                        response.Output
                            .SelectMany(o => o.Content)
                            .Where(c => c.Type == "output_text")
                            .Select(c => c.Text));

                    if (!string.IsNullOrWhiteSpace(outputText))
                    {
                        var cleaned = outputText.Replace("```", string.Empty).Trim();
                        var parts = cleaned.Split("|||", StringSplitOptions.RemoveEmptyEntries)
                                           .Select(p => p.Trim())
                                           .ToList();

                        // Chuẩn hoá số phần tử bằng số câu hỏi
                        if (parts.Count < questionsNeedingAI.Count)
                        {
                            while (parts.Count < questionsNeedingAI.Count) parts.Add(string.Empty);
                        }
                        else if (parts.Count > questionsNeedingAI.Count)
                        {
                            parts = parts.Take(questionsNeedingAI.Count).ToList();
                        }

                        static string Normalize(string? s)
                        {
                            return (s ?? string.Empty).Trim().ToLowerInvariant().Replace(" ", string.Empty);
                        }

                        // Nếu AI có mục TOTAL, tách ra (nhưng không dùng vì tính từng câu)
                        for (int idx = parts.Count - 1; idx >= 0; idx--)
                        {
                            var p = parts[idx];
                            var mTotal = Regex.Match(p, @"^TOTAL\s*=\s*([0-9]+(\.[0-9]+)?)$", RegexOptions.IgnoreCase);
                            if (mTotal.Success)
                            {
                                parts.RemoveAt(idx);
                            }
                        }

                        // Parse từng phần và tạo response
                        for (int i = 0; i < questionsNeedingAI.Count; i++)
                        {
                            var piece = parts[i];
                            var q = questionsNeedingAI[i];

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

                            bool isCorrect = score >= 0.999m || Normalize(q.Question.Answer) == Normalize(q.Question.RightAnswer);
                            
                            aiResults.Add((
                                q.QuestionId, 
                                q.Question.QuestionText, 
                                q.Question.Answer, 
                                q.Question.RightAnswer, 
                                score, 
                                isCorrect, 
                                comment));
                        }
                    }
                }
            }

            // Tính điểm tổng
            decimal finalScore = 0;

            // Đếm điểm từ rule-based results
            foreach (var (_, _, _, _, _, _, _, score, _) in ruleBasedResults)
            {
                finalScore += score;
            }

            // Đếm điểm từ AI results
            foreach (var (_, _, _, _, score, _, _) in aiResults)
            {
                finalScore += score;
            }

            // Tạo response từ rule-based results
            foreach (var (questionId, questionText, answer, rightAnswer, mark, partialMark, isCorrect, score, comment) in ruleBasedResults)
            {
                result.Add(new MockTestMarkResponse
                {
                    Question = questionText,
                    Answer = answer,
                    RightAnswer = rightAnswer,
                    Score = score,
                    IsCorrect = isCorrect,
                    Comment = comment
                });

                // Cập nhật điểm vào user answer đã lưu
                if (userAnswerEntities.TryGetValue(questionId, out var userAnswer))
                {
                    userAnswer.Score = score;
                    await _userAnswerRepository.UpdateAsync(userAnswer);
                }
            }

            // Tạo response từ AI results
            foreach (var (questionId, questionText, answer, rightAnswer, score, isCorrect, comment) in aiResults)
            {
                result.Add(new MockTestMarkResponse
                {
                    Question = questionText,
                    Answer = answer,
                    RightAnswer = rightAnswer,
                    Score = score,
                    IsCorrect = isCorrect,
                    Comment = comment
                });

                // Cập nhật điểm vào user answer đã lưu
                if (userAnswerEntities.TryGetValue(questionId, out var userAnswer))
                {
                    userAnswer.Score = score;
                    await _userAnswerRepository.UpdateAsync(userAnswer);
                }
            }

            // Cập nhật điểm tổng vào kết quả
            mocktestResult.Score = finalScore;
            await _mockTestResultRepository.UpdateAsync(mocktestResult);

            return new BaseResponse<List<MockTestMarkResponse>>(
                $"Chấm bài mock test thành công",
                StatusCodeEnum.OK_200,
                result);
        }

        public async Task<BaseResponse<MockTestCreateResponse>> CreateMockTestAsync(MockTestCreateRequest request, string userId)
        {
            var mocktest = new Mocktest
            {
                MockTestTitle = request.MockTestTitle,
                MockTestDescription = request.MockTestDescription,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            var mockTestCreate = await _mockTestRepository.AddAsync(mocktest);

            var questions = new List<Mocktestquestion>();

            using (var stream = request?.ExcelFile?.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet?.RangeUsed().RowsUsed().Skip(1); // bỏ header

                var pictures = worksheet.Pictures
                    .Select(p => new
                    {
                        Picture = p,
                        Row = p.TopLeftCell.Address.RowNumber,
                        Column = p.TopLeftCell.Address.ColumnNumber
                    })
                    .ToList();

                foreach (var row in rows)
                {
                    string imageUrl = "";
                    
                    // Tìm hình ảnh ở cột 4 (mặc định) của row hiện tại
                    var picture = pictures.FirstOrDefault(p => 
                        p.Row == row.RowNumber() && 
                        p.Column == 5);
                    
                    if(picture != null)
                    {
                        var imageBytes = picture.Picture.ImageStream.ToArray();
                        
                        // Tạo MemoryStream từ imageBytes (không dispose để FormFile có thể đọc)
                        var imageStream = new MemoryStream(imageBytes);
                        
                        IFormFile imageFile = new FormFile(
                            imageStream,
                            0,
                            imageBytes.Length,
                            "file",
                            $"{Guid.NewGuid()}.png"
                        )
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = "image/png"
                        };

                        imageUrl = await FileUploadHelper.UploadQuestionImageAsync(imageFile, _env.WebRootPath);
                        
                        // Dispose stream sau khi upload xong
                        imageStream.Dispose();
                    }

                    var question = new Mocktestquestion
                    {
                        MockTestId = mockTestCreate.MockTestId,
                        QuestionContent = row.Cell(1).GetString(),
                        CorrectAnswer = row.Cell(2).GetString(),
                        PartialMark = string.IsNullOrWhiteSpace(row.Cell(3).GetString()) ? null : row.Cell(3).GetString(),
                        Mark = decimal.TryParse(row.Cell(4).GetString(), out var markValue) ? markValue : 0m,
                        ImageUrl = imageUrl,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };
                    questions.Add(question);
                    await _questionRepository.AddAsync(question);
                }
            }

            var result = _mapper.Map<MockTestCreateResponse>(mockTestCreate);
            result.MockTestQuestions = _mapper.Map<List<MockTestQuestionCreateResponse>>(questions);

            // Convert ImageUrl từ relative path sang full URL
            foreach (var question in result.MockTestQuestions)
            {
                question.ImageUrl = string.IsNullOrEmpty(question.ImageUrl) ? "" : await _mediaService.GetMediaUrlAsync(question.ImageUrl);
            }

            return new BaseResponse<MockTestCreateResponse>(
                "Tạo bài mock test thành công",
                StatusCodeEnum.OK_200,
                result);
        }

        public async Task<BaseResponse<string>> DeleteMockTestAsync(int mockTestId)
        {
            var mockTest = await _mockTestRepository.FindOneAsync(x => x.MockTestId == mockTestId);
            if (mockTest == null)
            {
                throw new Exception("Mock test không tồn tại");
            }

            await _mockTestRepository.DeleteAsync(mockTest);

            return new BaseResponse<string>(
                "Xóa mock test thành công",
                StatusCodeEnum.OK_200,
                "Mock test đã được xóa thành công.");
        }
    }
}
