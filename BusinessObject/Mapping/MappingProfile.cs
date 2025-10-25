using AutoMapper;
using BusinessObject.Model;
using DTOs.Request.Categories;
using DTOs.Request.Courses;
using DTOs.Response.Categories;
using DTOs.Response.Courses;
using DTOs.Response.CourseRegistration;
using DTOs.Response.CourseContent;
using BusinessObject.DTOs.Response.Quizzes;
using BusinessObject.DTOs.Response.Modules;
using BusinessObject.DTOs.Request.Modules;
using BusinessObject.DTOs.Request.Chapters;
using DTOs.Request.CourseContent;

namespace BusinessObject.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Account mappings
            //CreateMap<Account, xxxx>().ReverseMap();

            // Course mappings
            CreateMap<Course, CourseResponse>()
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ReverseMap();
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
            CreateMap<Chapter, ChapterResponse>().ReverseMap();
            CreateMap<ModuleRequest, Module>();
            CreateMap<ChapterRequest, Chapter>();
            // Tree mappings for detail response with children
            CreateMap<Module, ModuleDetailResponse>().ReverseMap();
            CreateMap<Chapter, ChapterDetailResponse>().ReverseMap();
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
        }
    }
}
