using Repository.IRepositories;
using Repository.Repositories;
using Repository.BaseRepository;
using Repository.IBaseRepository;
using Service;
using Service.Trello;
using Service.OpenAI;
using Service.VnPay;
using BusinessObject.Mapping;

namespace IGCSE.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped<ICourseRepository, CourseRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICoursekeyRepository, CoursekeyRepository>();
            services.AddScoped<ICoursesectionRepository, CoursesectionRepository>();
            services.AddScoped<ILessonRepository, LessonRepository>();
            services.AddScoped<ILessonitemRepository, LessonitemRepository>();
            services.AddScoped<IProcessRepository, ProcessRepository>();
            services.AddScoped<IProcessitemRepository, ProcessitemRepository>();
            services.AddScoped<IQuizRepository, QuizRepository>();
            services.AddScoped<IQuizResultRepository, QuizResultRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<IParentStudentLinkRepository, ParentStudentLinkRepository>();

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // AutoMapper
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            // Application services
            services.AddScoped<AccountService>();
            services.AddScoped<CourseService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<CourseRegistrationService>();
            services.AddScoped<TrelloApiService>();
            services.AddHttpClient<ApiService>();
            services.AddScoped<VnPayApiService>();
            services.AddScoped<OpenAIApiService>();
            services.AddScoped<PaymentService>();
            services.AddScoped<OpenAIEmbeddingsApiService>();
            services.AddScoped<TestService>();
            services.AddScoped<QuizService>();

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            return services;
        }
    }
}


