using AutoMapper;
using BusinessObject.DTOs.Request.Quizzes;
using BusinessObject.DTOs.Response.Quizzes;
using BusinessObject.Model;
using BusinessObject.Payload.Request.OpenAI;
using BusinessObject.Payload.Request.OpenApi;
using Common.Constants;
using DTOs.Response.Accounts;
using Repository.IRepositories;
using Service.OpenAI;

namespace Service
{
    public class QuizService
    {
        private readonly IMapper _mapper;
        private readonly IQuizRepository _quizRepository;
        private readonly OpenAIApiService _openAIApiService;
        private readonly IQuestionRepository _questionRepository;
        private readonly IQuizResultRepository _quizResultRepository;

        public QuizService(IMapper mapper, IQuizRepository quizRepository, OpenAIApiService openAIApiService, IQuestionRepository questionRepository, IQuizResultRepository quizResultRepository)
        {
            _mapper = mapper;
            _quizRepository = quizRepository;
            _openAIApiService = openAIApiService;
            _questionRepository = questionRepository;
            _quizResultRepository = quizResultRepository;
        }

        public async Task<BaseResponse<QuizResponse>> GetQuizByIdAsync(int quizId)
        {
            var quiz = await _quizRepository.GetByIdAsync(quizId);
            if (quiz == null)
            {
                throw new Exception("Quiz not found");
            }

            var quizResponse = _mapper.Map<QuizResponse>(quiz);

            return new BaseResponse<QuizResponse>(
                "Quiz retrieved successfully",
                StatusCodeEnum.OK_200,
                quizResponse
            );
        }
        
        public async Task<BaseResponse<List<QuizMarkResponse>>> MarkQuizAsync(QuizMarkRequest request)
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

				foreach(var userAnswer in request.UserAnswers)
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
                        "Quiz mark successfully",
                        StatusCodeEnum.OK_200,
                        new List<QuizMarkResponse>());

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
                    result.Add(new QuizMarkResponse
                    {
                        Question = q.QuestionText,
                        Answer = q.Answer,
                        RightAnswer = q.RightAnswer,
                        IsCorrect = isCorrect,
                        Comment = comment
                    });
                    if(isCorrect == true)
                    {
                        finalScore += 1;
                    }
                }

                var quizResult = new Quizresult
                {
                    QuizId = request.UserAnswers[0].QuestionId,
                    CreatedBy = request.UserAnswers[0].UserId,
                    Score = finalScore,
                    CreatedAt = DateTime.UtcNow
                };

                if(finalScore <= 60)
                {
                    quizResult.IsPassed = false;
                }
                else
                {
                    quizResult.IsPassed = true;
                }

                await _quizResultRepository.AddAsync(quizResult);

                return new BaseResponse<List<QuizMarkResponse>>(
                        "Quiz marked successfully",
                        StatusCodeEnum.OK_200,
                        result);
        }
    }
}
