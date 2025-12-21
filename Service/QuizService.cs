using AutoMapper;
using BusinessObject.DTOs.Request.Quizzes;
using BusinessObject.DTOs.Response.Quizzes;
using BusinessObject.Model;
using BusinessObject.Payload.Request.OpenAI;
using BusinessObject.Payload.Request.OpenApi;
using Common.Constants;
using Repository.IRepositories;
using Service.OpenAI;
using BusinessObject.DTOs.Response;
using BusinessObject.Payload.Response.Trello;
using Common.Utils;
using Service.Trello;
using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml.ConditionalFormatting.Contracts;
using System.Collections.Generic;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;

namespace Service
{
    public class QuizService
    {
        private readonly IMapper _mapper;
        private readonly IQuizRepository _quizRepository;
        private readonly OpenAIApiService _openAIApiService;
        private readonly IQuestionRepository _questionRepository;
        private readonly IQuizResultRepository _quizResultRepository;
        private readonly TrelloCardService _trelloCardService;
        private readonly IWebHostEnvironment _env;
        private readonly IQuizUserAnswerRepository _quizUserAnswerRepository;
        private readonly MediaService _mediaService;
        private readonly IProcessRepository _processRepository;
        private readonly ILessonRepository _lessonRepository;

        public QuizService(
            IMapper mapper, 
            IQuizRepository quizRepository, 
            OpenAIApiService openAIApiService, 
            IQuestionRepository questionRepository, 
            IQuizResultRepository quizResultRepository, 
            TrelloCardService trelloCardService,
            IWebHostEnvironment env,
            IQuizUserAnswerRepository quizUserAnswerRepository,
            MediaService mediaService,
            IProcessRepository processRepository,
            ILessonRepository lessonRepository)
        {
            _mapper = mapper;
            _quizRepository = quizRepository;
            _openAIApiService = openAIApiService;
            _questionRepository = questionRepository;
            _quizResultRepository = quizResultRepository;
            _trelloCardService = trelloCardService;
            _env = env;
            _quizUserAnswerRepository = quizUserAnswerRepository;
            _mediaService = mediaService;
            _processRepository = processRepository;
            _lessonRepository = lessonRepository;
        }

        public async Task<BaseResponse<QuizResponse>> GetQuizByIdAsync(int quizId, string userId)
        {
            var checkAllowance = await _quizRepository.CheckAllowance(userId, quizId);
            if (!checkAllowance)
            {
                throw new Exception("Bạn chưa mở khoá bài quiz này. Vui lòng hoàn thành bài học trước.");
            }

            var quiz = await _quizRepository.GetByQuizIdAsync(quizId);

            if (quiz == null)
            {
                throw new Exception("Không tìm thấy bài quiz này.");
            }

            return new BaseResponse<QuizResponse>(
                "Lấy bài quiz thành công.",
                StatusCodeEnum.OK_200,
                quiz
            );
        }

        public async Task<BaseResponse<object>> GetQuizByIdOrReviewAsync(int quizId, string userId, string userRole)
        {
            if(userRole != "Parent" && userRole != "Student")
            {
                var quizReview = await _quizRepository.GetQuizWithAnswerAsync(quizId);
                return new BaseResponse<object>
                {
                    Message = "Lấy kết quả quiz thành công.",
                    StatusCode = StatusCodeEnum.OK_200,
                    Data = quizReview
                };
            }

            // Kiểm tra nếu quiz đã được làm (có result) thì trả về review
            var quizResultReview = await _quizResultRepository.GetQuizResultWithReviewAsync(quizId, userId);
            if (quizResultReview != null)
            {
                return new BaseResponse<object>
                {
                    Message = "Lấy kết quả quiz thành công.",
                    StatusCode = StatusCodeEnum.OK_200,
                    Data = quizResultReview
                };
            }

            // Nếu chưa làm, trả về quiz để làm
            var checkAllowance = await _quizRepository.CheckAllowance(userId, quizId);
            if (!checkAllowance)
            {
                throw new Exception("Bạn chưa mở khoá bài quiz này. Vui lòng hoàn thành bài học trước.");
            }

            var quiz = await _quizRepository.GetByQuizIdAsync(quizId);

            if (quiz == null)
            {
                throw new Exception("Không tìm thấy bài quiz này.");
            }

            return new BaseResponse<object>
            {
                Message = "Lấy bài quiz thành công.",
                StatusCode = StatusCodeEnum.OK_200,
                Data = quiz
            };
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


        public async Task<BaseResponse<List<QuizMarkResponse>>> MarkQuizAsync(QuizMarkRequest request, string userId)
        {
            List<QuizMarkResponse> result = new List<QuizMarkResponse>();

            List<QuestionMarkRequest> questions = new List<QuestionMarkRequest>();
            int? quizIdFromQuestions = null;

            // Validate input list
            if (request?.UserAnswers == null || request.UserAnswers.Count == 0)
            {
                return new BaseResponse<List<QuizMarkResponse>>(
                    "No answers to mark",
                    StatusCodeEnum.OK_200,
                    new List<QuizMarkResponse>());
            }

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

                if (quizIdFromQuestions == null)
                {
                    quizIdFromQuestions = questionText.QuizId;
                }

                var userAnswerText = userAnswer.Answer ?? string.Empty;
                var correctAnswer = questionText.CorrectAnswer;

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
                        questionText.QuestionContent, 
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
                        QuestionText = questionText.QuestionContent,
                        Answer = userAnswerText,
                        RightAnswer = correctAnswer,
                    };
                    questionsNeedingAI.Add((userAnswer.QuestionId, questionRequest));
                }

                // Lưu user answer vào database
                var quizUserAnswer = new Quizuseranswer
                {
                    QuestionId = userAnswer.QuestionId,
                    QuizId = questionText.QuizId,
                    Answer = userAnswer.Answer
                };
                await _quizUserAnswerRepository.AddAsync(quizUserAnswer);
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
                    MaxTokens = 200
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
                result.Add(new QuizMarkResponse
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
                result.Add(new QuizMarkResponse
                {
                    Question = questionText,
                    Answer = answer,
                    RightAnswer = isPassed ? rightAnswer : null,
                    IsCorrect = isCorrect,
                    Comment = comment
                });
            }

            var quizResult = new Quizresult
            {
                QuizId = (int)quizIdFromQuestions,
                UserId = userId,
                Score = finalScore,
                IsPassed = isPassed,
                CreatedAt = DateTime.UtcNow
            };

            await _quizResultRepository.AddAsync(quizResult);

            // Nếu quiz pass, unlock lesson tiếp theo
            if (quizResult.IsPassed)
            {
                try
                {
                    // Lấy thông tin quiz để biết lessonId và courseId
                    var quiz = await _quizRepository.GetByIdAsync(quizResult.QuizId);
                    if (quiz != null)
                    {
                        // Lấy lesson tiếp theo
                        var nextLesson = await _lessonRepository.GetNextLessonAsync(quiz.LessonId);
                        
                        if (nextLesson != null)
                        {
                            // Unlock lesson tiếp theo cho student
                            await _processRepository.UnlockLessonForStudentAsync(
                                userId, 
                                nextLesson.LessonId, 
                                quiz.CourseId
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log lỗi nhưng không làm fail toàn bộ request
                    // Có thể thêm logging service ở đây nếu cần
                    Console.WriteLine($"Error unlocking next lesson: {ex.Message}");
                }
            }

            return new BaseResponse<List<QuizMarkResponse>>(
                    "Chấm bài quiz thành công",
                    StatusCodeEnum.OK_200,
                    result);
        }
        
        public async Task CreateQuizForTrelloAsync(int courseId, int lessonId, string quizTitle ,List<TrelloCardResponse> trelloCardResponses, TrelloToken trelloToken)
        {
            quizTitle = quizTitle.Replace("[test]", "").Trim(); 
            string quizDescription = "This is a quiz imported from Trello.";
            List<Question> questions = new List<Question>();
            foreach (var trelloCardResponse in trelloCardResponses)
            {
                if (trelloCardResponse.Name.Contains("[Description]"))
                {
                    quizDescription = trelloCardResponse.Name.Replace("[Description]", "").Trim();
                }
                else
                {
                    var attachments = await _trelloCardService.GetTrelloCardAttachments(trelloCardResponse.Id, trelloToken);
                    
                    // get first attachment that is image
                    var imageUrl = string.Empty;
                    if (!CommonUtils.isEmtyList(attachments))
                    {
                        var imageAttachment = attachments.FirstOrDefault();
                        imageUrl = await _trelloCardService.DownloadTrelloCardAttachment(imageAttachment.Url, trelloToken);
                    }
                    
                    questions.Add(new Question
                    {
                        QuestionContent = trelloCardResponse.Name,
                        CorrectAnswer = trelloCardResponse.Description,
                        PictureUrl = imageUrl,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
            Quiz quiz = new Quiz
            {
                CourseId = courseId,
                LessonId = lessonId,
                QuizTitle = quizTitle,
                QuizDescription = quizDescription,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Questions = questions
            }; 
            await _quizRepository.AddAsync(quiz);
        }

        public async Task<BaseResponse<QuizCreateResponse>> CreateQuizAsync(QuizCreateRequest request)
        {
            var quiz = new Quiz
            {
                CourseId = request.CourseId,
                LessonId = request.LessonId,
                QuizTitle = request.QuizTitle,
                QuizDescription = request.QuizDescription,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var quizCreate = await _quizRepository.AddAsync(quiz);

            var questions = new List<Question>();

            using (var stream = request?.ExcelFile?.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet?.RangeUsed().RowsUsed().Skip(1); // bỏ header

                // Lấy tất cả hình ảnh với thông tin row và column
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
                    string pictureUrl = "";
                    
                    // Tìm hình ảnh ở cột 5 của row hiện tại
                    var picture = pictures.FirstOrDefault(p => 
                        p.Row == row.RowNumber() && 
                        p.Column == 3);
                    
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

                        pictureUrl = await FileUploadHelper.UploadQuestionImageAsync(imageFile, _env.WebRootPath);
                        
                        // Dispose stream sau khi upload xong
                        imageStream.Dispose();
                    }

                    var question = new Question
                    {
                        QuizId = quizCreate.QuizId,
                        QuestionContent = row.Cell(1).GetString(),
                        CorrectAnswer = row.Cell(2).GetString(),
                        PictureUrl = pictureUrl,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };
                    questions.Add(question);
                    await _questionRepository.AddAsync(question);
                }
            }

            var result = _mapper.Map<QuizCreateResponse>(quizCreate);
            result.Questions = _mapper.Map<List<QuestionCreateResponse>>(questions);

            // Convert PictureUrl từ relative path sang full URL
            foreach (var question in result.Questions)
            {
                question.PictureUrl = string.IsNullOrEmpty(question.PictureUrl) ? "" : await _mediaService.GetMediaUrlAsync(question.PictureUrl);
            }

            return new BaseResponse<QuizCreateResponse>(
                "Tạo bài quiz thành công",
                StatusCodeEnum.OK_200,
                result);
        }
    }
}


