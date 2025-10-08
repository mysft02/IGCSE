using AutoMapper;
using BusinessObject.Model;
using DTOs.Request.Categories;
using DTOs.Request.Courses;
using DTOs.Response.Categories;
using DTOs.Response.Courses;
using DTOs.Response.CourseRegistration;
using DTOs.Response.CourseContent;
using DTOs.Request.CourseRegistration;
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
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.Name))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Course.Category.CategoryName))
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

            // Student Progress mappings
            CreateMap<Process, LessonProgressResponse>()
                .ForMember(dest => dest.LessonName, opt => opt.MapFrom(src => src.Lesson.Name))
                .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.ItemProgress, opt => opt.Ignore());

            CreateMap<Processitem, LessonItemProgressResponse>()
                .ForMember(dest => dest.LessonItemName, opt => opt.MapFrom(src => src.LessonItem.Name));
        }
    }
}
