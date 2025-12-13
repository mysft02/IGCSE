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
using Service.Trello;
using Microsoft.AspNetCore.Hosting;
using System.Text.RegularExpressions;
using Common.Utils;

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
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseCertificateRepository _courseCertificateRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly MediaService _mediaService;

        public FinalQuizService(
            IMapper mapper,
            IFinalQuizRepository finalQuizRepository,
            OpenAIApiService openAIApiService,
            IQuestionRepository questionRepository,
            IFinalQuizResultRepository finalQuizResultRepository,
            TrelloCardService trelloCardService,
            IWebHostEnvironment env,
            IFinalQuizUserAnswerRepository finalQuizUserAnswerRepository,
            ICourseRepository courseRepository,
            ICourseCertificateRepository courseCertificateRepository,
            IAccountRepository accountRepository,
            MediaService mediaService)
        {
            _mapper = mapper;
            _finalQuizRepository = finalQuizRepository;
            _openAIApiService = openAIApiService;
            _questionRepository = questionRepository;
            _finalQuizResultRepository = finalQuizResultRepository;
            _trelloCardService = trelloCardService;
            _env = env;
            _finalQuizUserAnswerRepository = finalQuizUserAnswerRepository;
            _courseRepository = courseRepository;
            _courseCertificateRepository = courseCertificateRepository;
            _accountRepository = accountRepository;
            _mediaService = mediaService;
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

            // Rule-Based Pre-Filtering: Phân loại câu hỏi thành 2 nhóm
            var ruleBasedResults = new List<(int QuestionId, string QuestionText, string Answer, string RightAnswer, bool IsCorrect, decimal Similarity, string Comment)>();
            var questionsNeedingAI = new List<(int QuestionId, QuestionMarkRequest Question)>();

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

                var userAnswerText = userAnswer.Answer ?? string.Empty;
                var correctAnswer = questionText.CorrectAnswer;
                var formattedQuestionText = Regex.Replace(questionText.QuestionContent, @"Câu\s*\d+\s*:", $"Câu {count}:");

                // Rule-Based Pre-Filtering: Kiểm tra xem có thể chấm điểm bằng rule không
                var (canUseRuleBased, isCorrect, similarity) = CommonUtils.CanUseRuleBasedScoring(
                    userAnswerText, 
                    correctAnswer, 
                    similarityThreshold: 0.9m);

                if (canUseRuleBased)
                {
                    // Sử dụng Rule-Based Scoring (không cần gọi OpenAI)
                    var comment = isCorrect 
                        ? "Câu trả lời chính xác." 
                        : $"Câu trả lời chưa chính xác. Đáp án đúng: {correctAnswer}";
                    
                    ruleBasedResults.Add((
                        userAnswer.QuestionId, 
                        formattedQuestionText, 
                        userAnswerText, 
                        correctAnswer, 
                        isCorrect, 
                        similarity, 
                        comment));
                }
                else
                {
                    // Cần AI để chấm điểm (câu trả lời phức tạp, không rõ ràng)
                    var questionRequest = new QuestionMarkRequest
                    {
                        QuestionText = formattedQuestionText,
                        Answer = userAnswerText,
                        RightAnswer = correctAnswer,
                    };
                    questionsNeedingAI.Add((userAnswer.QuestionId, questionRequest));
                }

                // Lưu user answer vào database
                var quizUserAnswer = new Finalquizuseranswer
                {
                    FinalQuizResultId = finalQuizResult.FinalQuizResultId,
                    QuestionId = userAnswer.QuestionId,
                    Answer = userAnswer.Answer
                };

                await _finalQuizUserAnswerRepository.AddAsync(quizUserAnswer);
                count++;
            }

            // Chỉ gọi OpenAI nếu có câu hỏi cần AI scoring
            List<(int QuestionId, string QuestionText, string Answer, string RightAnswer, bool IsCorrect, string Comment)> aiResults = new List<(int, string, string, string, bool, string)>();
            if (questionsNeedingAI.Any())
            {
                var contentText = string.Join("\n", questionsNeedingAI.Select(q =>
                    $"Question: {q.Question.QuestionText}\nAnswer: {q.Question.Answer}\nRightAnswer: {q.Question.RightAnswer}\n"));

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
                        // 🧠 Parse output theo format mới: T$comment hoặc F$comment, phân cách bằng |||
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

                        // Parse từng phần và tạo response
                        for (int i = 0; i < questionsNeedingAI.Count; i++)
                        {
                            var part = parts[i];
                            var isCorrect = part.StartsWith("T$", StringComparison.OrdinalIgnoreCase);
                            var comment = part.StartsWith("T$", StringComparison.OrdinalIgnoreCase) 
                                ? part.Substring(2).Trim() 
                                : part.StartsWith("F$", StringComparison.OrdinalIgnoreCase) 
                                    ? part.Substring(2).Trim() 
                                    : part;

                            var q = questionsNeedingAI[i];
                            aiResults.Add((
                                q.QuestionId, 
                                q.Question.QuestionText, 
                                q.Question.Answer, 
                                q.Question.RightAnswer, 
                                isCorrect, 
                                comment));
                        }
                    }
                }
            }

            // Tính điểm số
            decimal finalScore = 0;
            var totalQuestions = ruleBasedResults.Count + aiResults.Count;
            
            // Đếm điểm từ rule-based results
            foreach (var (_, _, _, _, isCorrect, _, _) in ruleBasedResults)
            {
                if (isCorrect) finalScore += 1;
            }
            
            // Đếm điểm từ AI results
            foreach (var (_, _, _, _, isCorrect, _) in aiResults)
            {
                if (isCorrect) finalScore += 1;
            }

            // Tính IsPassed
            bool isPassed = totalQuestions > 0 && finalScore > totalQuestions / 2;

            // Tạo response từ rule-based results
            foreach (var (questionId, questionText, answer, rightAnswer, isCorrect, similarity, comment) in ruleBasedResults)
            {
                result.Add(new FinalQuizMarkResponse
                {
                    Question = questionText,
                    Answer = answer,
                    RightAnswer = isPassed ? rightAnswer : null,
                    IsCorrect = isCorrect,
                    Comment = comment
                });
            }

            // Tạo response từ AI results
            foreach (var (questionId, questionText, answer, rightAnswer, isCorrect, comment) in aiResults)
            {
                result.Add(new FinalQuizMarkResponse
                {
                    Question = questionText,
                    Answer = answer,
                    RightAnswer = isPassed ? rightAnswer : null,
                    IsCorrect = isCorrect,
                    Comment = comment
                });
            }

            finalQuizResult.IsPassed = isPassed;
            finalQuizResult.Score = finalScore;

            await _finalQuizResultRepository.UpdateAsync(finalQuizResult);

            if(finalQuizResult.IsPassed == true)
            {
                var fquiz = await _finalQuizRepository.FindOneWithIncludeAsync(x => x.FinalQuizId == request.FinalQuizID, c => c.Course);
                var course = fquiz?.Course;
                var user = await _accountRepository.FindOneAsync(x => x.Id == userId);

                var certificateUrl = await _mediaService.GenerateCertificate(user.NormalizedUserName, course.Name);

                var certificate = new Coursecertificate
                {
                    UserId = userId,
                    CourseId = fquiz.CourseId,
                    ImageUrl = certificateUrl,
                    CreatedAt = DateTime.UtcNow,
                };

                await _courseCertificateRepository.AddAsync(certificate);
            }

            return new BaseResponse<List<FinalQuizMarkResponse>>(
                    "Chấm bài quiz thành công",
                    StatusCodeEnum.OK_200,
                    result);
        }
    }
}
