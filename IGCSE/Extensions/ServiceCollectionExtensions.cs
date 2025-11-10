using Repository.IRepositories;
using Repository.Repositories;
using Repository.BaseRepository;
using Repository.IBaseRepository;
using Service;
using Service.Trello;
using Service.OpenAI;
using BusinessObject.Mapping;
using Service.PayOS;
using Service.OAuth;

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
            services.AddScoped<ICoursesectionRepository, CoursesectionRepository>();
            services.AddScoped<ILessonRepository, LessonRepository>();
            services.AddScoped<ILessonitemRepository, LessonitemRepository>();
            services.AddScoped<IProcessRepository, ProcessRepository>();
            services.AddScoped<IProcessitemRepository, ProcessitemRepository>();
            services.AddScoped<IQuizRepository, QuizRepository>();
            services.AddScoped<IQuizResultRepository, QuizResultRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<IModuleRepository, ModuleRepository>();
            // services.AddScoped<IChapterRepository, ChapterRepository>(); // Chapter disabled
            // services.AddScoped<ChapterService>(); // Chapter disabled
            services.AddScoped<IParentStudentLinkRepository, ParentStudentLinkRepository>();
            services.AddScoped<ITrelloTokenRepository, TrelloTokenRepository>();
            services.AddScoped<IMockTestRepository, MockTestRepository>();
            services.AddScoped<IMockTestQuestionRepository, MockTestQuestionRepository>();
            services.AddScoped<IMockTestResultRepository, MockTestResultRepository>();

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // AutoMapper
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            // Application services
            services.AddScoped<AccountService>();
            services.AddScoped<CourseService>();
            services.AddScoped<CourseRegistrationService>();
            services.AddScoped<TrelloApiService>();
            services.AddHttpClient<ApiService>();
            services.AddScoped<PayOSApiService>();
            services.AddScoped<OpenAIApiService>();
            services.AddScoped<PaymentService>();
            services.AddScoped<OpenAIEmbeddingsApiService>();
            services.AddScoped<QuizService>();
            services.AddScoped<TrelloOAuthService>();
            services.AddScoped<TrelloTokenService>();
            services.AddScoped<MockTestService>();
            services.AddScoped<MediaService>();
            services.AddScoped<ModuleService>();

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


