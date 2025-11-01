using AutoMapper;
using BusinessObject.Model;
using BusinessObject.DTOs.Response.Quizzes;
using BusinessObject.DTOs.Response.ParentStudentLink;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.MockTest;
using BusinessObject.Payload.Response.MockTest;
using BusinessObject.DTOs.Response.MockTestQuestion;
using BusinessObject.DTOs.Response.Questions;
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
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1));

            CreateMap<Lesson, LessonResponse>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1));

            CreateMap<Lessonitem, LessonItemResponse>()
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content ?? ""))
                .ForMember(dest => dest.ItemType, opt => opt.MapFrom(src => src.ItemType ?? "text"));

            // Module, Chapter mappings
            CreateMap<Module, ModuleResponse>().ReverseMap();
            
            // Map from Chapter to ChapterResponse
            CreateMap<Chapter, ChapterResponse>()
                .ReverseMap();
                
            // Map from ChapterRequest to Chapter
            CreateMap<ChapterRequest, Chapter>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ReverseMap();
                
            CreateMap<ModuleRequest, Module>();
            
            // Tree mappings for detail response with children
            CreateMap<Module, ModuleDetailResponse>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ? (sbyte)1 : (sbyte)0))
                .ReverseMap();
                
            CreateMap<Chapter, ChapterDetailResponse>()
                .ReverseMap();
                
            // Course section mappings
            CreateMap<Coursesection, CourseSectionDetailResponse>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1))
                .ReverseMap();
                
            // Lesson mappings
            CreateMap<Lesson, LessonDetailResponse>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1))
                .ReverseMap();
                
            // Lesson item mappings
            CreateMap<Lessonitem, LessonItemResponse>()
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content ?? string.Empty))
                .ForMember(dest => dest.ItemType, opt => opt.MapFrom(src => src.ItemType ?? "text"))
                .ReverseMap();

            // Student Progress mappings
            CreateMap<Process, LessonProgressResponse>()
                .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.ItemProgress, opt => opt.Ignore());

            CreateMap<Processitem, LessonItemProgressResponse>()
                .ForMember(dest => dest.LessonItemName, opt => opt.MapFrom(src => src.LessonItem.Name));

            CreateMap<CourseSectionRequest, Coursesection>();
            CreateMap<Quiz, QuizResponse>().ReverseMap();

            CreateMap<Question, QuestionResponse>().ReverseMap();

            CreateMap<Parentstudentlink, ParentStudentLinkResponse>().ReverseMap()
                .ForMember(dest => dest.Student, opt => opt.MapFrom(src => src.Student));

            // TrelloToken mappings
            CreateMap<TrelloToken, TrelloTokenResponse>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
                .ReverseMap();

            CreateMap<Mocktest, MockTestResponse>().ReverseMap();
            CreateMap<Mocktest, MockTestQueryResponse>().ReverseMap();

            CreateMap<Mocktestquestion, MockTestQuestionResponse>().ReverseMap();
        }
    }
}
