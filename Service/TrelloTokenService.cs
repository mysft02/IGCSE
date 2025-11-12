using BusinessObject.DTOs.Response;
using BusinessObject.Model;
using BusinessObject.Payload.Request.Filter;
using Repository.IRepositories;
using AutoMapper;
using BusinessObject.DTOs.Response.Trellos;
using BusinessObject.Payload.Response.Trello;
using Common.Utils;
using Service.Background.Attributes;
using Service.Trello;
using Service.Background.Interfaces;
using TrelloTokenResponse = BusinessObject.DTOs.Response.TrelloTokenResponse;

namespace Service;

public class TrelloTokenService
{
    private readonly ITrelloTokenRepository _trelloTokenRepository;
    private readonly TrelloBoardService _trelloBoardService;
    private readonly TrelloListService _trelloListService;
    private readonly MockTestService _mockTestService;
    private readonly CourseService _courseService;
    private readonly SectionService _sectionService;
    private readonly LessonService _lessonService;
    private readonly QuizService _quizService;
    private readonly IMapper _mapper;
    private readonly MockTestQuestionService _mockTestQuestionService;
    private readonly IBackgroundTaskInvoker? _backgroundTaskInvoker;

    public TrelloTokenService(
        ITrelloTokenRepository trelloTokenRepository, 
        IMapper mapper, 
        TrelloBoardService trelloBoardService,
        TrelloListService trelloListService,
        CourseService courseService,
        SectionService sectionService,
        LessonService lessonService,
        QuizService quizService,
        MockTestService mockTestService,
        MockTestQuestionService mockTestQuestionService,
        IBackgroundTaskInvoker? backgroundTaskInvoker = null)
    {
        _trelloTokenRepository = trelloTokenRepository;
        _mapper = mapper;
        _trelloBoardService = trelloBoardService;
        _trelloListService = trelloListService;
        _courseService = courseService;
        _sectionService = sectionService;
        _lessonService = lessonService;
        _quizService = quizService;
        _mockTestService = mockTestService;
        _mockTestQuestionService = mockTestQuestionService;
        _backgroundTaskInvoker = backgroundTaskInvoker;
    }

    /// <summary>
    /// Search Trello tokens with filtering and pagination
    /// </summary>
    public async Task<PaginatedResponse<TrelloTokenResponse>> SearchTrelloTokensAsync(TrelloTokenQueryRequest request)
    {
        // Build filter expression
        var filter = request.BuildFilter<TrelloToken>();
        
        // Get total count first (for pagination info)
        var totalCount = await _trelloTokenRepository.CountAsync(filter);
        
        // Get filtered data with pagination
        var items = await _trelloTokenRepository.FindWithPagingAsync(
            filter, 
            request.Page, 
            request.GetPageSize()
        );

        // Apply sorting to the paged results
        var sortedItems = request.ApplySorting(items);
        
        var totalPages = (int)Math.Ceiling((double)totalCount / request.GetPageSize());

        // Map to response
        return new PaginatedResponse<TrelloTokenResponse>
        {
            Items = sortedItems.Select(token => _mapper.Map<TrelloTokenResponse>(token)).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.GetPageSize(),
            TotalPages = totalPages
        };
    }

    public async Task<List<TrelloBoardDtoResponse>> GetTrelloBoardByUserIdAsync(string userId, string trelloId)
    {
        var trelloToken = await _trelloTokenRepository.FindOneAsync(token =>
            token.TrelloId == trelloId &&
            token.UserId == userId
        );
        if(CommonUtils.IsEmptyObject(trelloToken)) 
        {
            throw new Exception("Không tìm thấy token Trello cho người dùng này");
        }
        var trelloBoard = _trelloBoardService.GetTrelloBoard(trelloToken);
        var boards = trelloBoard.Result
            .Select(board =>
            {
                var dto = _mapper.Map<TrelloBoardDtoResponse>(board);
                dto.trelloId = trelloId;
                return dto;
            })
            .ToList();
        return boards;
    }
    
    public async Task<string> AutoUploadFromTrelloAsync(string userId, string trelloId, string boardId)
    {
        var trelloToken = await _trelloTokenRepository.FindOneAsync(token =>
            token.TrelloId == trelloId &&
            token.UserId == userId
        );
        if(CommonUtils.IsEmptyObject(trelloToken)) 
        {
            throw new Exception("Không tìm thấy token Trello cho người dùng này");
        }

        if (trelloToken.IsSync)
        {
            throw new Exception("Tài khoản trello này đang được đồng bộ hóa. Vui lòng thử lại sau.");
        }
        // Invoke background task to sync and upload course
        await _backgroundTaskInvoker.InvokeBackgroundTaskAsync(
            this,
            nameof(SyncAndUploadCourse),
            trelloToken,
            boardId
            );
        return "Upload bài giảng từ Trello đang được tiến hành trong nền. Vui lòng kiểm tra lại sau.";
    }
    
    [BackgroundTask("syncExecutor")]
    public async Task<string> SyncAndUploadCourse(TrelloToken trelloToken, string boardId)
    {
        try
        {
            trelloToken.IsSync = true;
            await _trelloTokenRepository.UpdateAsync(trelloToken);
            // step 1: get all lists from board
            List<TrelloListResponse> trelloListResponses =
                await _trelloBoardService.GetTrelloLists(boardId, trelloToken);

            Course course = new Course();
            Coursesection section = new Coursesection();
            Lesson lesson = new Lesson();
            int countSection = 1;
            int countLesson = 1;

            // Step 2: get all card of each list
            foreach (var list in trelloListResponses)
            {
                List<TrelloCardResponse> trelloCardResponses =
                    await _trelloListService.GetTrelloCardByList(list.Id, trelloToken);

                if (ExtractTypeTrelloListContent(list.Name) == "Course")
                {
                    //create course
                    course = await _courseService.CreateCourseForTrelloAsync(list.Name, trelloCardResponses);
                }
                else if (ExtractTypeTrelloListContent(list.Name) == "Section")
                {
                    //create section
                    section = await _sectionService.CreateCourseSectionForTrelloAsync(course.CourseId, list.Name,
                        countSection, trelloCardResponses);
                    countSection++;
                    countLesson = 1;
                }
                else if (ExtractTypeTrelloListContent(list.Name) == "Lesson")
                {
                    //create lesson
                    await _lessonService.CreateLessonForTrelloAsync(section.CourseSectionId, list.Name, countLesson,
                        trelloCardResponses, trelloToken);
                    countLesson++;
                }
                else if (ExtractTypeTrelloListContent(list.Name) == "Test")
                {
                    await _quizService.CreateQuizForTrelloAsync(course.CourseId, lesson.LessonId, list.Name,
                        trelloCardResponses, trelloToken);
                }
                else if (ExtractTypeTrelloListContent(list.Name) == "Other")
                {
                    // Skip other types
                    continue;
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception("Đồng bộ hóa và tải lên từ Trello thất bại: " + e.Message);
        }
        finally
        {
            trelloToken.IsSync = false;
            await _trelloTokenRepository.UpdateAsync(trelloToken);
        }
  
        return "";
    }
    
    private string ExtractTypeTrelloListContent(string listName)
    {
        if (string.IsNullOrWhiteSpace(listName))
        {
            return "Other";
        }

        var normalizedName = listName.ToLowerInvariant();
        
        return normalizedName switch
        {
            var name when name.ToLower().Contains("[course]") => "Course",
            var name when name.ToLower().Contains("[section]") => "Section",
            var name when name.ToLower().Contains("[lesson]") => "Lesson",
            var name when name.ToLower().Contains("[test]") => "Test",
            _ => "Other"
        };
    }

    public async Task<string> AutoUploadMockTestFromTrelloAsync(string userId, string trelloId, string boardId)
    {
        var trelloToken = await _trelloTokenRepository.FindOneAsync(token =>
            token.TrelloId == trelloId &&
            token.UserId == userId
        );
        if (CommonUtils.IsEmptyObject(trelloToken))
        {
            throw new Exception("Không tìm thấy token Trello cho người dùng này");
        }

        if (trelloToken.IsSync)
        {
            throw new Exception("Tài khoản trello này đang được đồng bộ hóa. Vui lòng thử lại sau.");
        }
        // Invoke background task to sync and upload course
        await _backgroundTaskInvoker.InvokeBackgroundTaskAsync(
            this,
            nameof(SyncAndUploadMockTest),
            trelloToken,
            boardId
            );
        return "Upload bài thi thử từ Trello đang được tiến hành trong nền. Vui lòng kiểm tra lại sau.";
    }

    [BackgroundTask("syncExecutor")]
    public async Task<string> SyncAndUploadMockTest(TrelloToken trelloToken, string boardId)
    {
        try
        {
            trelloToken.IsSync = true;
            await _trelloTokenRepository.UpdateAsync(trelloToken);
            // step 1: get all lists from board
            List<TrelloListResponse> trelloListResponses =
                await _trelloBoardService.GetTrelloLists(boardId, trelloToken);

            Mocktest mocktest = new Mocktest();

            // Step 2: get all card of each list
            foreach (var list in trelloListResponses)
            {
                List<TrelloCardResponse> trelloCardResponses =
                    await _trelloListService.GetTrelloCardByList(list.Id, trelloToken);

                if (!list.Name.Contains("Question"))
                {
                    mocktest = await _mockTestService.CreateMockTestForTrelloAsync(list.Name, trelloCardResponses, trelloToken.UserId);
                }
                else
                {
                    await _mockTestQuestionService.CreateMockTestQuestionForTrelloAsync(mocktest.MockTestId, list.Name, trelloCardResponses, trelloToken);
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception("Đồng bộ hóa và tải lên từ Trello thất bại: " + e.Message);
        }
        finally
        {
            trelloToken.IsSync = false;
            await _trelloTokenRepository.UpdateAsync(trelloToken);
        }

        return "";
    }
}
