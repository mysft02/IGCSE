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
using Repository.Repositories;
using BusinessObject.DTOs.Response.Quizzes;
using System.Text.RegularExpressions;

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

        public async Task<BaseResponse<object>> GetFinalQuizByIdAsync(int finalQuizId, string userId)
        {
            if(string.IsNullOrEmpty(finalQuizId.ToString()))
            {
                throw new Exception("Id không được để trống");
            }

            var checkPassed = await _finalQuizResultRepository.GetFinalQuizResultWithReview(finalQuizId, userId);
            if(checkPassed != null)
            {
                return new BaseResponse<object>(
                    "Lấy bài kiểm tra thành công",
                    StatusCodeEnum.OK_200,
                    checkPassed
            );
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

            return new BaseResponse<object>(
                "Lấy bài kiểm tra thành công",
                StatusCodeEnum.OK_200,
                finalQuiz
            );
        }

        private string prompt =
            @"Bạn là hệ thống chấm bài tự động. 
            Nhiệm vụ: Với mỗi câu hỏi trong danh sách, hãy xác định Đúng hoặc Sai bằng cách so sánh Answer với RightAnswer
            RightAnswer là đáp án đúng của câu hỏi.
            Answer là đáp án của người dùng.
            Đáp án không nhất thiết trùng khớp:
                - Answer có thể là một phần của RightAnswer.
                - RightAnswer có thể là một phần của Answer.
                - Answer và RightAnswer có thể có các từ ngữ khác nhau nhưng vẫn đúng.
                - Answer và RightAnswer chỉ cần mang ý nghĩa tương đương nhau.

            Sau đó tạo một nhận xét NGẮN (≤100 chữ) cho từng câu, viết theo ĐÚNG THỨ TỰ xuất hiện trong dữ liệu.

            YÊU CẦU ĐẦU RA:
            - Chỉ trả về danh sách các nhận xét.
            - Mỗi nhận xét cách nhau bằng chuỗi phân tách: |||
            - Ý đúng và sai sẽ được biểu hiện bằng kí tự T và F, cách văn bản nhận xét bằng kí tự $
            - KHÔNG thêm đánh số, tiêu đề, nhãn, hay văn bản thừa.

            Chỉ xuất kết quả theo đúng định dạng yêu cầu.";

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

            var fqResult = new Finalquizresult
            {
                FinalQuizId = request.FinalQuizID,
                Score = 0,
                IsPassed = false,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            var finalQuizResult = await _finalQuizResultRepository.AddAsync(fqResult);

            int count = 1;

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
                    QuestionText = Regex.Replace(questionText.QuestionContent, @"Câu\s*\d+\s*:", $"Câu {count}:"),
                    Answer = userAnswer.Answer ?? string.Empty,
                    RightAnswer = questionText.CorrectAnswer,
                };

                var quizUserAnswer = new Finalquizuseranswer
                {
                    FinalQuizResultId = finalQuizResult.FinalQuizResultId,
                    QuestionId = userAnswer.QuestionId,
                    Answer = userAnswer.Answer
                };

                await _finalQuizUserAnswerRepository.AddAsync(quizUserAnswer);

                questions.Add(questionRequest);
                count++;
            }

            var contentText = string.Join("\n", questions.Select(q =>
                $"Question: {q.QuestionText}\nAnswer: {q.Answer}\nRightAnswer: {q.RightAnswer}\n"));

            var fullPrompt = prompt + $"\n\nDỮ LIỆU:\n{contentText}";

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
                                text = fullPrompt
                            }
                        }
                    }
                };

            var body = new OpenAIBody
            {
                Model = "gpt-4o-mini",
                Input = input,
                Temperature = 0.5,
                MaxTokens = 5000
            };
            var apiRequest = OpenApiRequest.Builder()
                .CallUrl("/responses")
                .Body(body)
                .Build();

            var response = await _openAIApiService.PostAsync<OpenAIBody, OpenAIResponse>(apiRequest, body);

            if (response?.Output == null || response.Output.Count == 0)
                return new BaseResponse<List<FinalQuizMarkResponse>>(
                    "Final Quiz mark successfully",
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

            // 🧠 Parse output theo format mới: T$comment hoặc F$comment, phân cách bằng |||
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

            decimal finalScore = 0;
            List<(bool isCorrect, string comment)> parsedResults = new List<(bool, string)>();

            // Parse từng phần để extract T/F và comment
            foreach (var part in parts)
            {
                bool isCorrect = false;
                string comment = string.Empty;

                // Format: T$comment hoặc F$comment
                if (part.StartsWith("T$", StringComparison.OrdinalIgnoreCase))
                {
                    isCorrect = true;
                    comment = part.Substring(2).Trim();
                    finalScore += 1;
                }
                else if (part.StartsWith("F$", StringComparison.OrdinalIgnoreCase))
                {
                    isCorrect = false;
                    comment = part.Substring(2).Trim();
                }
                else
                {
                    // Fallback: nếu không có format T$ hoặc F$, thử parse theo cách cũ
                    // Hoặc có thể là chỉ có comment, cần so sánh đáp án
                    comment = part;
                    // So sánh đáp án để xác định đúng/sai (fallback)
                    if (parsedResults.Count < questions.Count)
                    {
                        var q = questions[parsedResults.Count];
                        static string Normalize(string? s)
                        {
                            return (s ?? string.Empty).Trim().ToLowerInvariant().Replace(" ", string.Empty);
                        }
                        isCorrect = Normalize(q.Answer) == Normalize(q.RightAnswer);
                        if (isCorrect) finalScore += 1;
                    }
                }

                parsedResults.Add((isCorrect, comment));
            }

            // Tính IsPassed trước khi tạo response
            bool isPassed = finalScore > questions.Count / 2;

            // Tạo response - chỉ trả về RightAnswer nếu pass
            for (int i = 0; i < questions.Count; i++)
            {
                var q = questions[i];
                var (isCorrect, comment) = i < parsedResults.Count
                    ? parsedResults[i]
                    : (false, string.Empty);

                string newIndex = (i + 1).ToString();

                questions[i].QuestionText = Regex.Replace(
                    questions[i].QuestionText,
                    @"Câu\s*\d+\s*:",
                    $"Câu {newIndex}:"
                );

                result.Add(new FinalQuizMarkResponse
                {
                    Question = q.QuestionText,
                    Answer = q.Answer,
                    RightAnswer = isPassed ? q.RightAnswer : null, // Chỉ trả về đáp án đúng nếu pass
                    IsCorrect = isCorrect,
                    Comment = comment
                });
            }

            finalQuizResult.IsPassed = isPassed;
            finalQuizResult.Score = finalScore;

            await _finalQuizResultRepository.UpdateAsync(finalQuizResult);

            return new BaseResponse<List<FinalQuizMarkResponse>>(
                    "Chấm bài quiz thành công",
                    StatusCodeEnum.OK_200,
                    result);
        }
    }
}
