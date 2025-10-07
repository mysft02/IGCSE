using AutoMapper;
using BusinessObject.Model;
using DTOs.Request.Categories;
using DTOs.Request.Courses;
using DTOs.Response.Categories;
using DTOs.Response.Courses;
namespace BusinessObject.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Account mappings
            //CreateMap<Account, xxxx>().ReverseMap();

            // Course mappings
            CreateMap<Course, CourseResponse>().ReverseMap();
            CreateMap<CourseRequest, Course>();

            // Category mappings
            CreateMap<Category, CategoryResponse>().ReverseMap();
            CreateMap<CategoryRequest, Category>();
        }
    }
}
