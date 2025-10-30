using AutoMapper;
using BusinessObject.Model;
using BusinessObject.Payload.Request.OpenAI;
using BusinessObject.Payload.Request.OpenApi;
using Common.Constants;
using Repository.IRepositories;
using Service.OpenAI;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using BusinessObject.DTOs.Request.MockTest;
using BusinessObject.DTOs.Response.MockTest;
using BusinessObject.DTOs.Response.Quizzes;
using BusinessObject.DTOs.Request.Quizzes;
using BusinessObject.DTOs.Response;
using BusinessObject.Payload.Request.MockTest;
using BusinessObject.Payload.Response.MockTest;
using Microsoft.AspNetCore.Hosting;

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

        public MockTestService(
            IMapper mapper, 
            IMockTestRepository mockTestRepository,
            OpenAIApiService openAIApiService,
            IMockTestQuestionRepository questionRepository,
            IMockTestResultRepository mockTestResultRepository,
            MediaService mediaService,
            IWebHostEnvironment env)
        {
            _mapper = mapper;
            _mockTestRepository = mockTestRepository;
            _openAIApiService = openAIApiService;
            _questionRepository = questionRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _mediaService = mediaService;
            _env = env;
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

            var totalPages = (int)Math.Ceiling((double)totalCount / request.GetPageSize());

            // Map to response
            return new BaseResponse<PaginatedResponse<MockTestQueryResponse>>
            {
                Data = new PaginatedResponse<MockTestQueryResponse>
                {
                    Items = sortedItems.Select(token => _mapper.Map<MockTestQueryResponse>(token)).ToList(),
                    TotalCount = totalCount,
                    Page = request.Page,
                    Size = request.GetPageSize(),
                    TotalPages = totalPages
                },
                Message = "Mock tests retrieved successfully",
                StatusCode = StatusCodeEnum.OK_200
            };
        }

        public async Task<BaseResponse<MockTestResponse>> GetMockTestByIdAsync(int mockTestId)
        {
            var mockTest = await _mockTestRepository.GetByMockTestIdAsync(mockTestId);
            if (mockTest == null)
            {
                throw new Exception("Mock test not found");
            }

            var mockTestResponse = _mapper.Map<MockTestResponse>(mockTest);

            foreach (var c in mockTestResponse.MockTestQuestions)
            {
                if (c.ImageUrl == null)
                {
                    continue;
                }

                var imageResponse = await _mediaService.GetImageAsync(_env.WebRootPath, c.ImageUrl);
                c.Image = imageResponse.Data;
            }

            return new BaseResponse<MockTestResponse>(
                "Mock test retrieved successfully",
                StatusCodeEnum.OK_200,
                mockTestResponse
            );
        }

        public async Task<BaseResponse<List<QuizMarkResponse>>> MarkMockTestAsync(QuizMarkRequest request)
        {
            List<QuizMarkResponse> result = new List<QuizMarkResponse>();
            List<QuestionMarkRequest> questions = new List<QuestionMarkRequest>();
            int? mockTestIdFromQuestions = null;

            // Validate input
            if (request?.UserAnswers == null || request.UserAnswers.Count == 0)
            {
                return new BaseResponse<List<QuizMarkResponse>>(
                    "No answers to mark",
                    StatusCodeEnum.OK_200,
                    new List<QuizMarkResponse>());
            }

            foreach (var userAnswer in request.UserAnswers)
            {
                if (userAnswer == null)
                    continue;

                var questionText = await _questionRepository.GetByIdAsync(userAnswer.QuestionId);
                if (questionText == null)
                    continue;

                if (mockTestIdFromQuestions == null)
                    mockTestIdFromQuestions = questionText.MockTestId;

                var questionRequest = new QuestionMarkRequest
                {
                    QuestionText = questionText.QuestionContent,
                    Answer = userAnswer.Answer ?? string.Empty,
                    RightAnswer = questionText.CorrectAnswer
                };

                questions.Add(questionRequest);
            }

            var contentText = string.Join("\n", questions.Select(q =>
                $"Question: {q.QuestionText}\nAnswer: {q.Answer}\nRight answer: {q.RightAnswer}\n"));

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
                            text = $"Với danh sách câu hỏi sau, hãy chấm đúng/sai bằng cách so sánh Answer với Right answer. Viết NHẬN XÉT ngắn (<100 chữ) cho mỗi câu THEO ĐÚNG THỨ TỰ. Chỉ trả về các đoạn comment, ngăn cách bằng chuỗi |||, KHÔNG thêm bất cứ văn bản nào khác.\n{contentText}\nChỉ in: comment1 ||| comment2 ||| ... ||| commentN"
                        }
                    }
                }
            };

            var body = new OpenAIBody
            {
                Model = "gpt-4.1-mini",
                Input = input,
                Temperature = 0.5,
                MaxTokens = 200
            };
            var apiRequest = OpenApiRequest.Builder()
                .CallUrl("/responses")
                .Body(body)
                .Build();

            var response = await _openAIApiService.PostAsync<OpenAIBody, OpenAIResponse>(apiRequest, body);

            if (response?.Output == null || response.Output.Count == 0)
                return new BaseResponse<List<QuizMarkResponse>>(
                    "Mock test marked successfully",
                    StatusCodeEnum.OK_200,
                    new List<QuizMarkResponse>());

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

            for (int i = 0; i < questions.Count; i++)
            {
                var q = questions[i];
                var comment = parts[i];
                bool isCorrect = Normalize(q.Answer) == Normalize(q.RightAnswer);
                result.Add(new QuizMarkResponse
                {
                    Question = q.QuestionText,
                    Answer = q.Answer,
                    RightAnswer = q.RightAnswer,
                    IsCorrect = isCorrect,
                    Comment = comment
                });
                if (isCorrect)
                    finalScore += 1;
            }

            var mockTestResult = new Mocktestresult
            {
                MockTestId = request.UserAnswers[0].QuestionId,
                CreatedBy = request.UserAnswers[0].UserId,
                Score = finalScore,
                CreatedAt = DateTime.UtcNow,
                IsPassed = finalScore > 60
            };

            await _mockTestResultRepository.AddAsync(mockTestResult);

            return new BaseResponse<List<QuizMarkResponse>>(
                "Mock test marked successfully",
                StatusCodeEnum.OK_200,
                result);
        }

        public async Task<BaseResponse<MockTestCreateResponse>> ImportMockTestFromExcelAsync(MockTestCreateRequest request, string userId)
        {
            var response = new MockTestCreateResponse();

            if (request.ExcelFile == null || request.ExcelFile.Length == 0)
                throw new Exception("No file uploaded");

            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var fileExtension = Path.GetExtension(request.ExcelFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                throw new Exception("Only .xlsx and .xls files are allowed");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var mockTest = new Mocktest
            {
                MockTestTitle = request.MockTestTitle,
                MockTestDescription = request.MockTestDescription ?? "",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _mockTestRepository.AddAsync(mockTest);

            using var stream = request.ExcelFile.OpenReadStream();
            using var package = new ExcelPackage(stream);

            var worksheet = package.Workbook.Worksheets[0];
            if (worksheet == null)
                throw new Exception("Excel file must have at least one worksheet");

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount < 2)
                throw new Exception("Excel file must have at least two rows");

            var imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "mocktest", mockTest.MockTestId.ToString());
            if (!Directory.Exists(imageFolderPath))
                Directory.CreateDirectory(imageFolderPath);

            var questions = new List<Mocktestquestion>();

            for (int row = 2; row <= rowCount; row++)
            {
                var questionContent = worksheet.Cells[row, 1]?.Value?.ToString()?.Trim();
                var correctAnswer = worksheet.Cells[row, 2]?.Value?.ToString()?.Trim();

                if (string.IsNullOrEmpty(questionContent) || string.IsNullOrEmpty(correctAnswer))
                    continue;

                string? imageUrl = null;
                var drawings = worksheet.Drawings.Where(d => d.From.Row + 1 == row).ToList();

                if (drawings.Count > 0 && drawings[0] is ExcelPicture pic)
                {
                    var fileName = $"{row - 1}.png";
                    var filePath = Path.Combine(imageFolderPath, fileName);
                    var img = pic.Image;
                    if (img != null && img.ImageBytes != null)
                    {
                        await File.WriteAllBytesAsync(filePath, img.ImageBytes);
                        imageUrl = $"/images/mocktest/{mockTest.MockTestId}/{fileName}";
                    }
                }

                var question = new Mocktestquestion
                {
                    MockTestId = mockTest.MockTestId,
                    QuestionContent = questionContent,
                    CorrectAnswer = correctAnswer,
                    ImageUrl = imageUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _questionRepository.AddAsync(question);
                questions.Add(question);
            }

            if (questions.Count == 0)
                throw new Exception("No questions found in Excel file");

            response.Questions = questions;

            return new BaseResponse<MockTestCreateResponse>(
                "Mock test imported successfully",
                StatusCodeEnum.OK_200,
                response);
        }
    }
}
