using AutoMapper;
using BusinessObject.DTOs.Request.FinalQuizzes;
using BusinessObject.DTOs.Response.FinalQuizzes;
using BusinessObject.Model;
using BusinessObject.Payload.Request.OpenAI;
using BusinessObject.Payload.Request.OpenApi;
using Common.Constants;
using Repository.IRepositories;
using Service.OpenAI;
using BusinessObject.DTOs.Response;
using Common.Utils;
using Service.Trello;
using Microsoft.AspNetCore.Hosting;

namespace Service
{
    public class FinalQuizService
    {
        private readonly IMapper _mapper;
        private readonly IFinalQuizRepository _finalQuizRepository;
        private readonly OpenAIApiService _openAIApiService;
        private readonly IQuestionRepository _questionRepository;
        private readonly IFinalQuizResultRepository _finalQuizResultRepository;
        private readonly TrelloCardService _trelloCardService;
        private readonly IWebHostEnvironment _env;
        private readonly IFinalQuizUserAnswerRepository _finalQuizUserAnswerRepository;

        public FinalQuizService(
            IMapper mapper,
            IFinalQuizRepository finalQuizRepository,
            OpenAIApiService openAIApiService,
            IQuestionRepository questionRepository,
            IFinalQuizResultRepository finalQuizResultRepository,
            TrelloCardService trelloCardService,
            IWebHostEnvironment env,
            IFinalQuizUserAnswerRepository finalQuizUserAnswerRepository)
        {
            _mapper = mapper;
            _finalQuizRepository = finalQuizRepository;
            _openAIApiService = openAIApiService;
            _questionRepository = questionRepository;
            _finalQuizResultRepository = finalQuizResultRepository;
            _trelloCardService = trelloCardService;
            _env = env;
            _finalQuizUserAnswerRepository = finalQuizUserAnswerRepository;
        }

        public async Task<BaseResponse<FinalQuizResponse>> GetFinalQuizByIdAsync(int finalQuizId, string userId)
        {
            if(string.IsNullOrEmpty(finalQuizId.ToString()))
            {
                throw new Exception("Id không được để trống");
            }

            var checkAllowance = await _finalQuizRepository.CheckAllowance(finalQuizId, userId);
            if (!checkAllowance)
            {
                throw new Exception("Bạn chưa đủ điền kiện thực hiện bài final quiz. Vui lòng hoàn thành khoá học trước.");
            }

            var finalQuiz = await _finalQuizRepository.GetFinalQuizAsync(finalQuizId);
            if (finalQuiz == null)
            {
                throw new Exception("Bài final quiz không tìm thấy.");
            }

            return new BaseResponse<FinalQuizResponse>(
                "Lấy bài kiểm tra thành công",
                StatusCodeEnum.OK_200,
                finalQuiz
            );
        }

        public async Task<BaseResponse<List<FinalQuizMarkResponse>>> MarkFinalQuizAsync(FinalQuizMarkRequest request, string userId)
        {
            List<FinalQuizMarkResponse> result = new List<FinalQuizMarkResponse>();

            List<QuestionMarkRequest> questions = new List<QuestionMarkRequest>();

            // Validate input list
            if (request?.UserAnswers == null || request.UserAnswers.Count == 0)
            {
                return new BaseResponse<List<FinalQuizMarkResponse>>(
                    "No answers to mark",
                    StatusCodeEnum.OK_200,
                    new List<FinalQuizMarkResponse>());
            }

            var finalQuizResult = new Finalquizresult
            {
                FinalQuizId = request.FinalQuizID,
                Score = 0,
                IsPassed = false,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var fquizResult = await _finalQuizResultRepository.AddAsync(finalQuizResult);

            foreach (var userAnswer in request.UserAnswers)
            {
                if (userAnswer == null)
                {
                    continue;
                }
                var questionText = await _questionRepository.GetByIdAsync(userAnswer.QuestionId);
                if (questionText == null)
                {
                    // Bỏ qua nếu không tìm thấy câu hỏi tương ứng
                    continue;
                }

                var questionRequest = new QuestionMarkRequest
                {
                    QuestionText = questionText.QuestionContent,
                    Answer = userAnswer.Answer ?? string.Empty,
                    RightAnswer = questionText.CorrectAnswer,
                };

                if (questionText.PictureUrl != null)
                {
                    questionRequest.ImageBase64 = CommonUtils.GetBase64FromWwwRoot(_env.WebRootPath, questionText.PictureUrl);
                }

                var finalQuizUserAnswer = new Finalquizuseranswer
                {
                    QuestionId = userAnswer.QuestionId,
                    FinalQuizResultId = fquizResult.FinalQuizResultId,
                    Answer = userAnswer.Answer,
                };

                await _finalQuizUserAnswerRepository.AddAsync(finalQuizUserAnswer);

                questions.Add(questionRequest);
            }

            var contentText = string.Join("\n", questions.Select(q =>
            $"Question: {q.QuestionText}\nAnswer: {q.Answer}\nRight answer: {q.RightAnswer}\nImage: {q.ImageBase64}"));

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
                                text = $"Với danh sách câu hỏi sau, hãy chấm đúng/sai bằng cách so sánh Answer với Right answer. Nếu image không null thì hãy tạo hình ảnh từ base64 và dùng image như 1 phần của question. Viết NHẬN XÉT ngắn (<100 chữ) cho mỗi câu THEO ĐÚNG THỨ TỰ. Chỉ trả về các đoạn comment, ngăn cách bằng chuỗi |||, KHÔNG thêm bất cứ văn bản nào khác.\n{contentText}\nChỉ in: comment1 ||| comment2 ||| ... ||| commentN"
                            }
                        }
                    }
                };

            var body = new OpenAIBody
            {
                Model = "gpt-4o-mini",
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
                return new BaseResponse<List<FinalQuizMarkResponse>>(
                    "Quiz mark successfully",
                    StatusCodeEnum.OK_200,
                    new List<FinalQuizMarkResponse>());

            // 🧩 Ghép output text
            var outputText = string.Join("\n",
                response.Output
                    .SelectMany(o => o.Content)
                    .Where(c => c.Type == "output_text")
                    .Select(c => c.Text));

            if (string.IsNullOrWhiteSpace(outputText))
            {
                throw new Exception("OpenAI trả về rỗng (outputText empty)");
            }

            // 🧠 Cắt comment theo delimiter đơn giản '|||'
            var cleaned = outputText.Replace("```", string.Empty).Trim();
            var parts = cleaned.Split("|||", StringSplitOptions.RemoveEmptyEntries)
                               .Select(p => p.Trim())
                               .ToList();

            // Chuẩn hoá số phần tử bằng số câu hỏi
            if (parts.Count < questions.Count)
            {
                while (parts.Count < questions.Count) parts.Add(string.Empty);
            }
            else if (parts.Count > questions.Count)
            {
                parts = parts.Take(questions.Count).ToList();
            }

            // Hàm nội bộ để so sánh đáp án linh hoạt
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
                result.Add(new FinalQuizMarkResponse
                {
                    Question = q.QuestionText,
                    Answer = q.Answer,
                    RightAnswer = q.RightAnswer,
                    IsCorrect = isCorrect,
                    Comment = comment
                });
                if (isCorrect == true)
                {
                    finalScore += 1;
                }
            }

            fquizResult.Score = finalScore;

            if(finalScore <= questions.Count / 2)
            {
                fquizResult.IsPassed = false;
            }

            await _finalQuizResultRepository.UpdateAsync(fquizResult);

            return new BaseResponse<List<FinalQuizMarkResponse>>(
                    "Chấm bài quiz thành công",
                    StatusCodeEnum.OK_200,
                    result);
        }
    }
}
