using AutoMapper;
using BusinessObject.Model;
using BusinessObject.DTOs.Response.Quizzes;
using BusinessObject.DTOs.Response.ParentStudentLink;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Accounts;
using BusinessObject.DTOs.Response.Courses;
using BusinessObject.DTOs.Request.Courses;
using BusinessObject.DTOs.Response.Categories;
using BusinessObject.DTOs.Request.Categories;
using BusinessObject.DTOs.Response.CourseRegistration;
using BusinessObject.DTOs.Response.CourseContent;
using BusinessObject.DTOs.Response.Modules;
using BusinessObject.DTOs.Response.Chapters;
using BusinessObject.DTOs.Request.Chapters;
using BusinessObject.DTOs.Request.Modules;
using BusinessObject.DTOs.Request.CourseContent;

namespace BusinessObject.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Account mappings
            CreateMap<Account, AccountResponse>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.isActive, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();

            // Course mappings
            CreateMap<Course, CourseResponse>()
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ReverseMap();
                
            // Course detail mappings
            CreateMap<Course, CourseDetailResponse>()
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName));
            CreateMap<CourseRequest, Course>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            // Category mappings
            CreateMap<Category, CategoryResponse>().ReverseMap();
            CreateMap<CategoryRequest, Category>();

            // Course Registration mappings
            CreateMap<Coursekey, CourseRegistrationResponse>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CourseId.ToString()))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => "Unknown"))
                .ForMember(dest => dest.CourseKey, opt => opt.MapFrom(src => $"{src.CourseId}-{src.StudentId}-{src.CreatedAt.Value.Ticks}"));

            // Course Content mappings
            CreateMap<Coursesection, CourseSectionResponse>()
                .ForMember(dest => dest.CourseSectionId, opt => opt.MapFrom(src => src.CourseSectionId))
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1));

            CreateMap<Lesson, LessonResponse>()
                .ForMember(dest => dest.LessonId, opt => opt.MapFrom(src => src.LessonId))
                .ForMember(dest => dest.CourseSectionId, opt => opt.MapFrom(src => src.CourseSectionId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1));

            CreateMap<Lessonitem, LessonItemResponse>()
                .ForMember(dest => dest.LessonItemId, opt => opt.MapFrom(src => src.LessonItemId))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content ?? ""))
                .ForMember(dest => dest.ItemType, opt => opt.MapFrom(src => src.ItemType ?? "text"));

            // Module, Chapter mappings
            CreateMap<Module, ModuleResponse>().ReverseMap();
            
            // Map from Chapter to ChapterResponse
            CreateMap<Chapter, ChapterResponse>()
                .ForMember(dest => dest.ChapterID, opt => opt.MapFrom(src => src.ChapterID))
                .ForMember(dest => dest.ModuleID, opt => opt.MapFrom(src => src.ModuleID))
                .ForMember(dest => dest.ChapterName, opt => opt.MapFrom(src => src.ChapterName))
                .ForMember(dest => dest.ChapterDescription, opt => opt.MapFrom(src => src.ChapterDescription))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
                
            // Map from ChapterRequest to Chapter
            CreateMap<ChapterRequest, Chapter>()
                .ForMember(dest => dest.ChapterName, opt => opt.MapFrom(src => src.ChapterName))
                .ForMember(dest => dest.ChapterDescription, opt => opt.MapFrom(src => src.ChapterDescription))
                .ForMember(dest => dest.ModuleID, opt => opt.MapFrom(src => src.ModuleID))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
                
            CreateMap<ModuleRequest, Module>();
            
            // Tree mappings for detail response with children
            CreateMap<Module, ModuleDetailResponse>()
                .ForMember(dest => dest.ModuleID, opt => opt.MapFrom(src => src.ModuleID))
                .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.ModuleName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ? (sbyte)1 : (sbyte)0))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
                
            CreateMap<Chapter, ChapterDetailResponse>()
                .ForMember(dest => dest.ChapterID, opt => opt.MapFrom(src => src.ChapterID))
                .ForMember(dest => dest.ModuleID, opt => opt.MapFrom(src => src.ModuleID))
                .ForMember(dest => dest.ChapterName, opt => opt.MapFrom(src => src.ChapterName))
                .ForMember(dest => dest.ChapterDescription, opt => opt.MapFrom(src => src.ChapterDescription))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
                
            // Course section mappings
            CreateMap<Coursesection, CourseSectionDetailResponse>()
                .ForMember(dest => dest.CourseSectionId, opt => opt.MapFrom(src => src.CourseSectionId))
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.Order))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1));
                
            // Lesson mappings
            CreateMap<Lesson, LessonDetailResponse>()
                .ForMember(dest => dest.LessonId, opt => opt.MapFrom(src => src.LessonId))
                .ForMember(dest => dest.CourseSectionId, opt => opt.MapFrom(src => src.CourseSectionId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.Order))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1));
                
            // Lesson item mappings
            CreateMap<Lessonitem, LessonItemResponse>()
                .ForMember(dest => dest.LessonItemId, opt => opt.MapFrom(src => src.LessonItemId))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content ?? string.Empty))
                .ForMember(dest => dest.ItemType, opt => opt.MapFrom(src => src.ItemType ?? "text"));
            // Tree with children: List<Chapter> to List<ChapterDetailResponse> & List<Module> to List<ModuleDetailResponse>
            // Use ForMember with child mapping if necessary or configure AllowNullCollections, PreserveReferences if recursion.
            // Tương tự cho các class detail khác nếu cần custom.

            // Student Progress mappings
            CreateMap<Process, LessonProgressResponse>()
                .ForMember(dest => dest.LessonName, opt => opt.MapFrom(src => src.Lesson.Name))
                .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.ItemProgress, opt => opt.Ignore());

            CreateMap<Processitem, LessonItemProgressResponse>()
                .ForMember(dest => dest.LessonItemName, opt => opt.MapFrom(src => src.LessonItem.Name));

            CreateMap<CourseSectionRequest, Coursesection>();
            CreateMap<Quiz, QuizResponse>().ReverseMap();

            CreateMap<Parentstudentlink, ParentStudentLinkResponse>().ReverseMap()
                .ForMember(dest => dest.Student, opt => opt.MapFrom(src => src.Student));

            // TrelloToken mappings
            CreateMap<TrelloToken, TrelloTokenResponse>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
                .ReverseMap();
        }
    }
}
