using AutoMapper;
using BusinessObject.DTOs.Request.AptitudeTestQuestions;
using BusinessObject.DTOs.Request.LearningPaths;
using BusinessObject.DTOs.Request.Quizzes;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.AptitudeTestQuestionResponse;
using BusinessObject.DTOs.Response.AptitudeTestQuestions;
using BusinessObject.DTOs.Response.Courses;
using BusinessObject.DTOs.Response.LearningPaths;
using BusinessObject.DTOs.Response.Modules;
using BusinessObject.DTOs.Response.Quizzes;
using BusinessObject.Model;
using BusinessObject.Payload.Request.OpenAI;
using BusinessObject.Payload.Request.OpenApi;
using ClosedXML.Excel;
using Common.Constants;
using DocumentFormat.OpenXml.Office2010.Drawing;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Org.BouncyCastle.Asn1.Ocsp;
using Repository.IRepositories;
using Repository.Repositories;
using Service.OpenAI;

namespace Service
{
    public class LearningPathService
    {
        private readonly ILearningPathRepository _learningPathRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly MediaService _mediaService;
        private readonly IAptitudeTestQuestionRepository _aptitudeTestQuestionRepository;
        private readonly OpenAIApiService _openAIApiService;

        public LearningPathService(
            ILearningPathRepository learningPathRepository, 
            IModuleRepository moduleRepository, 
            IMapper mapper, 
            ICourseRepository courseRepository, 
            MediaService mediaService,
            IAptitudeTestQuestionRepository aptitudeTestQuestionRepository,
            OpenAIApiService openAIApiService)
        {
            _learningPathRepository = learningPathRepository;
            _moduleRepository = moduleRepository;
            _mapper = mapper;
            _courseRepository = courseRepository;
            _mediaService = mediaService;
            _aptitudeTestQuestionRepository = aptitudeTestQuestionRepository;
            _openAIApiService = openAIApiService;
        }

        public async Task<BaseResponse<LearningPathResponse>> CreateLearningPathAsync(LearningPathCreateRequest request)
        {
            if(request.ModuleId == null)
            {
                throw new Exception("Không có id module.");
            }

            string moduleList = "";
            foreach(var item in request.ModuleId)
            {
                var module = await _moduleRepository.FindOneAsync(x => x.ModuleID == item);
                if (module == null)
                {
                    throw new Exception($"không tìm thấy module {item}");
                }
                moduleList += item.ToString() + "_";
            }

            var learningPath = new Learningpath
            {
                Title = request.Title,
                Description = request.Description,
                ModulePath = moduleList.TrimEnd('_'),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            var query = await _learningPathRepository.AddAsync(learningPath);
            var result = _mapper.Map<LearningPathResponse>(query);

            return new BaseResponse<LearningPathResponse>(
                "Tạo lộ trình học tập thành công.",
                StatusCodeEnum.OK_200,
                result
                );
        }

        public async Task<BaseResponse<LearningPathResponse>> GetLearningPath(int id)
        {
            var learningPath = await _learningPathRepository.FindOneAsync(x => x.LearningPathId == id);
            if (learningPath == null)
            {
                throw new Exception("Không tìm thấy lộ trình học tập tìm kiếm.");
            }
            var result = _mapper.Map<LearningPathResponse>(learningPath);

            result.ModuleResponses = new List<ModuleWithCourseResponse>();
            List<int> moduleIdList = learningPath.ModulePath.TrimEnd('_').Split('_').Select(int.Parse).ToList();
            foreach ( var moduleId in moduleIdList)
            {
                var module = await _moduleRepository.FindOneWithIncludeAsync(x => x.ModuleID == moduleId, c => c.Courses);
                var moduleMapper = _mapper.Map<ModuleWithCourseResponse>(module);
                moduleMapper.Course = _mapper.Map<List<CourseModuleResponse>>(module?.Courses.Where(x => x.Status == "Open"));
                moduleMapper.Course = moduleMapper.Course.Take(3);
                foreach (var course in moduleMapper.Course)
                {
                    course.ImageUrl = string.IsNullOrEmpty(course.ImageUrl) ? "" : await _mediaService.GetMediaUrlAsync(course.ImageUrl);
                }
                result.ModuleResponses.Add(moduleMapper);
            }

            return new BaseResponse<LearningPathResponse>(
                "Tạo lộ trình học tập thành công.",
                StatusCodeEnum.OK_200,
                result
                );
        }

        public async Task<BaseResponse<PaginatedResponse<LearningPathResponse>>> GetAllLearningPath(LearningPathQuery query)
        {
            var learningPath = await _learningPathRepository.GetAllAsync();
            var result = _mapper.Map<IEnumerable<LearningPathResponse>>(learningPath);
            var pagedItem = result
                .Skip(query.Page * query.Size)
                .Take(query.Size)
                .ToList();

            var totalCount = result.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / query.Size);

            return new BaseResponse<PaginatedResponse<LearningPathResponse>>
            {
                Data = new PaginatedResponse<LearningPathResponse>
                {
                    Items = pagedItem,
                    TotalCount = totalCount,
                    Page = query.Page,
                    Size = query.Size,
                    TotalPages = totalPages
                },
                Message = "Lấy toàn bộ lộ trình học tập thành công",
                StatusCode = StatusCodeEnum.OK_200
            };
        }

        public async Task<BaseResponse<AptitudeTestQuestionCreateResponse>> AddAptitudeTestQuestionAsync(AptitudeTestQuestionCreateRequest request)
        {
            var questions = new List<Aptitudetestquestion>();

            using (var stream = request?.Questions?.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet?.RangeUsed().RowsUsed().Skip(1); // bỏ header

                foreach (var row in rows)
                {
                    var question = new Aptitudetestquestion
                    {
                        QuestionContent = row.Cell(1).GetString(),
                        Answer = row.Cell(2).GetString(),
                        ModuleId = int.Parse(row.Cell(3).GetString())
                    };

                    await _aptitudeTestQuestionRepository.AddAsync(question);
                    questions.Add(question);
                }
            }
            var result = new AptitudeTestQuestionCreateResponse
            {
                Questions = _mapper.Map<List<AptitudeQuestionResponse>>(questions)
            };

            return new BaseResponse<AptitudeTestQuestionCreateResponse>(
                "Thêm câu hỏi thành công.",
                StatusCodeEnum.OK_200,
                result);
        }

        public async Task<BaseResponse<AptitudeTestResponse>> GetAptitudeTest()
        {
            var questions = new List<AptitudeQuestionResponse>();
            var questionList = await _aptitudeTestQuestionRepository.GetAllAsync();
            var randomQuestionList = questionList
                .OrderBy(x => Guid.NewGuid())
                .Take(10)
                .ToList();

            var questionCount = 1;
            foreach(var question in randomQuestionList)
            {
                var questionMap = _mapper.Map<AptitudeQuestionResponse>(question);
                questionMap.QuestionContent = $"Câu {questionCount}: " + question.QuestionContent;
                questionCount++;
                questions.Add (questionMap);
            }

            var result = new AptitudeTestResponse
            {
                AptitudeQuestions = questions
            };

            return new BaseResponse<AptitudeTestResponse>(
                "Lấy danh sách câu hỏi thành công.",
                StatusCodeEnum.OK_200,
                result);
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

            YÊU CẦU ĐẦU RA:
            - Chỉ trả về danh sách các nhận xét.
            - Mỗi nhận xét cách nhau bằng chuỗi phân tách: |||
            - Ý đúng và sai sẽ được biểu hiện bằng kí tự T và F, cách văn bản nhận xét bằng kí tự $
            - KHÔNG thêm đánh số, tiêu đề, nhãn, hay văn bản thừa.

            Chỉ xuất kết quả theo đúng định dạng yêu cầu.";


        public async Task<BaseResponse<AptitudeTestMarkResponse>> MarkAptitudeTestAsync(AptitudeTestMarkRequest request)
        {
            List<AptitudeTestQuestionMarkRequest> questions = new List<AptitudeTestQuestionMarkRequest>();

            // Validate input list
            if (request?.UserAnswers == null || request.UserAnswers.Count == 0)
            {
                return new BaseResponse<AptitudeTestMarkResponse>(
                    "Không có câu trả lời nào.",
                    StatusCodeEnum.OK_200,
                    new AptitudeTestMarkResponse());
            }

            foreach (var userAnswer in request.UserAnswers)
            {
                if (userAnswer == null)
                {
                    continue;
                }
                var questionText = await _aptitudeTestQuestionRepository.GetByIdAsync(userAnswer.QuestionId);
                if (questionText == null)
                {
                    // Bỏ qua nếu không tìm thấy câu hỏi tương ứng
                    continue;
                }

                var questionRequest = new AptitudeTestQuestionMarkRequest
                {
                    ModuleId = questionText.ModuleId,
                    QuestionText = questionText.QuestionContent,
                    Answer = userAnswer.Answer ?? string.Empty,
                    RightAnswer = questionText.Answer,
                };

                questions.Add(questionRequest);
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
                MaxTokens = 200
            };
            var apiRequest = OpenApiRequest.Builder()
            .CallUrl("/responses")
                .Body(body)
                .Build();

            var response = await _openAIApiService.PostAsync<OpenAIBody, OpenAIResponse>(apiRequest, body);

            if (response?.Output == null || response.Output.Count == 0)
                return new BaseResponse<AptitudeTestMarkResponse>(
                    "Chấm bài đánh giá năng lực thành công.",
                    StatusCodeEnum.OK_200,
                    new AptitudeTestMarkResponse());

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
                    }
                }

                parsedResults.Add((isCorrect, comment));
            }

            List<int> moduleList = new List<int>();
            // Tạo response - chỉ trả về RightAnswer nếu pass
            for (int i = 0; i < questions.Count; i++)
            {
                var q = questions[i];
                var (isCorrect, comment) = i < parsedResults.Count
                    ? parsedResults[i]
                    : (false, string.Empty);

                if (!isCorrect)
                {
                    moduleList.Add(questions[i].ModuleId);
                }
            }

            var learningPath = await _learningPathRepository.GetMatchLearningPath(moduleList);
            var result = _mapper.Map<AptitudeTestMarkResponse>(learningPath);

            return new BaseResponse<AptitudeTestMarkResponse>(
                    "Chấm bài quiz thành công",
                    StatusCodeEnum.OK_200,
                    result);
        }
    }
}
