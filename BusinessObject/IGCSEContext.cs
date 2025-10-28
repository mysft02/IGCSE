using BusinessObject.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;


namespace BusinessObject;

public partial class IGCSEContext : IdentityDbContext<Account>
{
    public IGCSEContext() { }

    public IGCSEContext(DbContextOptions<IGCSEContext> options)
        : base(options) { }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Course> Courses { get; set; }
    public virtual DbSet<Coursekey> Coursekeys { get; set; }
    public virtual DbSet<Coursesection> Coursesections { get; set; }
    public virtual DbSet<Lesson> Lessons { get; set; }
    public virtual DbSet<Lessonitem> Lessonitems { get; set; }
    public virtual DbSet<Process> Processes { get; set; }
    public virtual DbSet<Processitem> Processitems { get; set; }
    public virtual DbSet<Transactionhistory> Transactionhistories { get; set; }
    public virtual DbSet<Useranswer> Useranswers { get; set; }
    public virtual DbSet<Question> Questions { get; set; }
    public virtual DbSet<Quiz> Quizzes { get; set; }
    public virtual DbSet<Quizresult> Quizresults { get; set; }
    public virtual DbSet<Parentstudentlink> Parentstudentlinks { get; set; }
    public virtual DbSet<TrelloToken> TrelloTokens { get; set; }
    public virtual DbSet<Mocktest> Mocktests { get; set; }

    public virtual DbSet<Mocktestquestion> Mocktestquestions { get; set; }

    public virtual DbSet<Mocktestresult> Mocktestresults { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        // ?u ti�n bi?n m�i tr??ng (Jenkins truy?n v�o) r?i m?i t?i appsettings.json
        var envConnection = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        if (!string.IsNullOrWhiteSpace(envConnection))
        {
            optionsBuilder.UseMySql(envConnection, ServerVersion.Parse("8.0.43-mysql"));
            return;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var fileConnection = configuration.GetConnectionString("DbConnection");
        if (!string.IsNullOrWhiteSpace(fileConnection))
        {
            optionsBuilder.UseMySql(fileConnection, ServerVersion.Parse("8.0.43-mysql"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseCollation("utf8mb4_0900_ai_ci").HasCharSet("utf8mb4");

        modelBuilder.Entity<Account>(b =>
        {
            b.ToTable("AspNetUsers");

            b.Property(a => a.Name).HasMaxLength(255);
            b.Property(a => a.Address).HasMaxLength(1000);
            b.Property(a => a.Phone).HasMaxLength(50);

            var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                d => d.ToDateTime(TimeOnly.MinValue),
                dt => DateOnly.FromDateTime(dt));

            b.Property(a => a.DateOfBirth).HasConversion(dateOnlyConverter);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id)
                  .HasColumnType("char(36)")
                  .UseCollation("ascii_general_ci");

            entity.Property(e => e.AccountID)
                  .HasColumnType("varchar(255)")
                  .IsRequired();

            entity.Property(e => e.Token)
                  .HasColumnType("longtext")
                  .IsRequired();

            entity.Property(e => e.JwtID)
                  .HasColumnType("longtext")
                  .IsRequired();

            entity.Property(e => e.IsUsed)
                  .HasColumnType("tinyint(1)")
                  .IsRequired();

            entity.Property(e => e.IsRevoked)
                  .HasColumnType("tinyint(1)")
                  .IsRequired();

            entity.Property(e => e.CreateAt)
                  .HasColumnType("datetime(6)")
                  .IsRequired();

            entity.Property(e => e.ExpiredAt)
                  .HasColumnType("datetime(6)")
                  .IsRequired();

            entity.HasOne(d => d.Account)
                  .WithMany()
                  .HasForeignKey(d => d.AccountID)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_RefreshTokens_AspNetUsers_AccountID");

            entity.HasIndex(e => e.AccountID)
                  .HasDatabaseName("IX_RefreshTokens_AccountID");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");
            entity.ToTable("category");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PRIMARY");

            entity.ToTable("course");

            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedBy).HasMaxLength(255);
        });

        modelBuilder.Entity<Coursekey>(entity =>
        {
            entity.HasKey(e => e.CourseKeyId).HasName("PRIMARY");

            entity.ToTable("coursekey");

            entity.Property(e => e.CourseKeyId).HasColumnName("CourseKeyID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.KeyValue).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'available'");
            entity.Property(e => e.StudentId)
                .HasMaxLength(255)
                .HasColumnName("StudentID");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PRIMARY");

            entity.ToTable("question");

            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.CorrectAnswer).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.PictureUrl).HasMaxLength(255);
            entity.Property(e => e.QuestionContent).HasMaxLength(500);
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Transactionhistory>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PRIMARY");

            entity.ToTable("transactionhistory");

            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<Useranswer>(entity =>
        {
            entity.HasKey(e => e.UserAnswerId).HasName("PRIMARY");

            entity.ToTable("useranswer");

            entity.Property(e => e.UserAnswerId).HasColumnName("UserAnswerID");
            entity.Property(e => e.Answer).HasMaxLength(500);
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Question)
                  .WithMany()
                  .HasForeignKey(d => d.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Useranswer_Question_QuestionID");

            entity.HasOne(d => d.User)
                  .WithMany()
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Useranswer_AspNetUsers_UserID");

            entity.HasIndex(e => e.QuestionId)
                  .HasDatabaseName("IX_Useranswer_QuestionID");

            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("IX_Useranswer_UserID");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.QuizId).HasName("PRIMARY");

            entity.ToTable("quiz");

            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.QuizDescription).HasMaxLength(255);
            entity.Property(e => e.QuizTitle).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Coursesection>(entity =>
        {
            entity.HasKey(e => e.CourseSectionId).HasName("PRIMARY");
            entity.ToTable("coursesection");
            entity.Property(e => e.CourseSectionId).HasColumnName("CourseSectionID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PRIMARY");
            entity.ToTable("lesson");
            entity.Property(e => e.LessonId).HasColumnName("LessonID");
            entity.Property(e => e.CourseSectionId).HasColumnName("CourseSectionID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Lessonitem>(entity =>
        {
            entity.HasKey(e => e.LessonItemId).HasName("PRIMARY");
            entity.ToTable("lessonitem");
            entity.Property(e => e.LessonItemId).HasColumnName("LessonItemID");
            entity.Property(e => e.Content).HasColumnType("text");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ItemType).HasMaxLength(50);
            entity.Property(e => e.LessonId).HasColumnName("LessonID");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Process>(entity =>
        {
            entity.HasKey(e => e.ProcessId).HasName("PRIMARY");
            entity.ToTable("process");
            entity.Property(e => e.ProcessId).HasColumnName("ProcessID");
            entity.Property(e => e.CourseKeyId).HasColumnName("CourseKeyID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.LessonId).HasColumnName("LessonID");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsUnlocked).HasColumnName("IsUnlocked").HasDefaultValue(true);
        });

        modelBuilder.Entity<Processitem>(entity =>
        {
            entity.HasKey(e => e.ProcessItemId).HasName("PRIMARY");
            entity.ToTable("processitem");
            entity.Property(e => e.ProcessItemId).HasColumnName("ProcessItemID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.LessonItemId).HasColumnName("LessonItemID");
            entity.Property(e => e.ProcessId).HasColumnName("ProcessID");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Quizresult>(entity =>
        {
            entity.HasKey(e => e.QuizResultId).HasName("PRIMARY");

            entity.ToTable("quizresult");

            entity.Property(e => e.QuizResultId).HasColumnName("QuizResultID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.Score).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Parentstudentlink>(entity =>
        {
            entity.HasKey(e => e.LinkId).HasName("PRIMARY");

            entity.ToTable("parentstudentlink");

            entity.Property(e => e.LinkId).HasColumnName("LinkID");
            entity.Property(e => e.ParentId)
                .HasMaxLength(255)
                .HasColumnName("ParentID");
            entity.Property(e => e.StudentId)
                .HasMaxLength(255)
                .HasColumnName("StudentID");
        });

        modelBuilder.Entity<TrelloToken>(entity =>
        {
            entity.HasKey(e => new { e.TrelloId, e.UserId }).HasName("PRIMARY");

            entity.ToTable("trello_token");

            entity.Property(e => e.TrelloId)
                .HasMaxLength(100)
                .HasColumnName("trello_id");

            entity.Property(e => e.TrelloApiToken)
                .HasMaxLength(100)
                .HasColumnName("trello_api_token");

            entity.Property(e => e.UserId)
                .HasMaxLength(100)
                .HasColumnName("user_id");

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.Property(e => e.IsSync)
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_sync");

            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TrelloToken_AspNetUsers_UserId");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_TrelloToken_UserId");
        });

        modelBuilder.Entity<Mocktest>(entity =>
        {
            entity.HasKey(e => e.MockTestId).HasName("PRIMARY");

            entity.ToTable("mocktest");

            entity.Property(e => e.MockTestId).HasColumnName("MockTestID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.MockTestDescription).HasColumnType("text");
            entity.Property(e => e.MockTestTitle).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Mocktestquestion>(entity =>
        {
            entity.HasKey(e => e.MockTestQuestionId).HasName("PRIMARY");

            entity.ToTable("mocktestquestion");

            entity.Property(e => e.MockTestQuestionId).HasColumnName("MockTestQuestionID");
            entity.Property(e => e.CorrectAnswer).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.MockTestId).HasColumnName("MockTestID");
            entity.Property(e => e.QuestionContent).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Mocktestresult>(entity =>
        {
            entity.HasKey(e => e.MockTestResultId).HasName("PRIMARY");

            entity.ToTable("mocktestresult");

            entity.Property(e => e.MockTestResultId).HasColumnName("MockTestResultID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.MockTestId).HasColumnName("MockTestID");
            entity.Property(e => e.Score).HasPrecision(18, 2);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}