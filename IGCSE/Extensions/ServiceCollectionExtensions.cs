using Repository.IRepositories;
using Repository.Repositories;
using Repository.BaseRepository;
using Repository.IBaseRepository;
using Service;
using Service.Trello;
using Service.OpenAI;
using Service.VnPay;
using BusinessObject.Mapping;
using Service.OAuth;
using Service.Background;
using Service.Background.Interfaces;
using Service.Background.Executors;

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
            services.AddScoped<ITrelloTokenRepository, TrelloTokenRepository>();
            services.AddScoped<IMockTestRepository, MockTestRepository>();
            services.AddScoped<IMockTestQuestionRepository, MockTestQuestionRepository>();
            services.AddScoped<IMockTestResultRepository, MockTestResultRepository>();
            services.AddScoped<IPackageRepository, PackageRepository>();
            services.AddScoped<IFinalQuizUserAnswerRepository, FinalQuizUserAnswerRepository>();
            services.AddScoped<IFinalQuizResultRepository, FinalQuizResultRepository>();
            services.AddScoped<IFinalQuizRepository, FinalQuizRepository>();
            services.AddScoped<IQuizUserAnswerRepository, QuizUserAnswerRepository>();
            services.AddScoped<IMockTestUserAnswerRepository, MockTestUserAnswerRepository>();

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
            services.AddScoped<QuizService>();
            services.AddScoped<TrelloOAuthService>();
            services.AddScoped<TrelloTokenService>();
            services.AddScoped<TrelloBoardService>();
            services.AddScoped<TrelloCardService>();
            services.AddScoped<TrelloListService>();
            services.AddScoped<LessonService>();
            services.AddScoped<SectionService>();
            services.AddScoped<MediaService>();
            services.AddScoped<MockTestService>();
            services.AddScoped<PackageService>();
            services.AddScoped<FinalQuizService>();
            services.AddScoped<MockTestQuestionService>();

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            return services;
        }

        /// <summary>
        /// Đăng ký background task services
        /// </summary>
        public static IServiceCollection AddBackgroundTaskServices(this IServiceCollection services)
        {
            // Executor registry (singleton)
            services.AddSingleton<IBackgroundExecutorRegistry, BackgroundExecutorRegistry>();

            // Queue executor (singleton) - dùng để queue tasks
            services.AddSingleton<QueueBackgroundExecutor>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<QueueBackgroundExecutor>>();
                return new QueueBackgroundExecutor(capacity: 100, logger);
            });

            // Sync executor (singleton) - chạy ngay lập tức
            services.AddSingleton<SyncBackgroundExecutor>(sp =>
            {
                var serviceProvider = sp.GetRequiredService<IServiceProvider>();
                var logger = sp.GetRequiredService<ILogger<SyncBackgroundExecutor>>();
                return new SyncBackgroundExecutor(serviceProvider, logger);
            });

            // Background hosted service để xử lý queue (singleton)
            services.AddHostedService<QueuedHostedService>();

            // Background task invoker (scoped - vì có thể cần scoped services)
            services.AddScoped<IBackgroundTaskInvoker, BackgroundTaskInvoker>();

            // Đăng ký executors vào registry khi app khởi động
            services.AddHostedService<BackgroundExecutorRegistrationService>();

            return services;
        }

        /// <summary>
        /// Service để đăng ký executors vào registry khi app khởi động
        /// </summary>
        private class BackgroundExecutorRegistrationService : IHostedService
        {
            private readonly IBackgroundExecutorRegistry _registry;
            private readonly QueueBackgroundExecutor _queueExecutor;
            private readonly SyncBackgroundExecutor _syncExecutor;
            private readonly ILogger<BackgroundExecutorRegistrationService> _logger;

            public BackgroundExecutorRegistrationService(
                IBackgroundExecutorRegistry registry,
                QueueBackgroundExecutor queueExecutor,
                SyncBackgroundExecutor syncExecutor,
                ILogger<BackgroundExecutorRegistrationService> logger)
            {
                _registry = registry;
                _queueExecutor = queueExecutor;
                _syncExecutor = syncExecutor;
                _logger = logger;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                // Đăng ký default executor (queue)
                _registry.RegisterExecutor("default", _queueExecutor);

                // Đăng ký sync executor
                _registry.RegisterExecutor("syncExecutor", _syncExecutor);

                _logger.LogInformation("Background executors registered: default, syncExecutor");
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}


