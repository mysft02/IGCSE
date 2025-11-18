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

    public virtual DbSet<Course> Courses { get; set; }

    // public virtual DbSet<Coursekey> Coursekeys { get; set; } // removed coursekey usage

    public virtual DbSet<Coursesection> Coursesections { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<Lessonitem> Lessonitems { get; set; }

    public virtual DbSet<Process> Processes { get; set; }

    public virtual DbSet<Processitem> Processitems { get; set; }

    public virtual DbSet<Transactionhistory> Transactionhistories { get; set; }
    public virtual DbSet<Finalquiz> Finalquizzes { get; set; }
    public virtual DbSet<Finalquizresult> Finalquizresults { get; set; }
    public virtual DbSet<Finalquizuseranswer> Finalquizuseranswers { get; set; }
    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<Quizresult> Quizresults { get; set; }

    public virtual DbSet<Module> Modules { get; set; }

    // public virtual DbSet<Chapter> Chapters { get; set; } // Chapter functionality removed

    public virtual DbSet<Parentstudentlink> Parentstudentlinks { get; set; }

    public virtual DbSet<Studentenrollment> Studentenrollments { get; set; }

    public virtual DbSet<TrelloToken> TrelloTokens { get; set; }
    public virtual DbSet<Mocktest> Mocktests { get; set; }
    public virtual DbSet<Mocktestquestion> Mocktestquestions { get; set; }
    public virtual DbSet<Mocktestresult> Mocktestresults { get; set; }
    public virtual DbSet<Mocktestuseranswer> Mocktestuseranswers { get; set; }
    public virtual DbSet<Package> Packages { get; set; }
    public virtual DbSet<Userpackage> Userpackages { get; set; }
    public virtual DbSet<Quizuseranswer> Quizuseranswers { get; set; }
    public virtual DbSet<Createslot> Createslots { get; set; }

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

        modelBuilder.Entity<Finalquiz>(entity =>
        {
            entity.HasKey(e => e.FinalQuizId).HasName("PRIMARY");

            entity.ToTable("finalquiz");

            entity.Property(e => e.FinalQuizId).HasColumnName("FinalQuizID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Finalquizresult>(entity =>
        {
            entity.HasKey(e => e.FinalQuizResultId).HasName("PRIMARY");

            entity.ToTable("finalquizresult");

            entity.Property(e => e.FinalQuizResultId).HasColumnName("FinalQuizResultID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.FinalQuizId).HasColumnName("FinalQuizID");
            entity.Property(e => e.Score).HasPrecision(18, 2);
            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<Finalquizuseranswer>(entity =>
        {
            entity.HasKey(e => e.FinalQuizUserAnswerId).HasName("PRIMARY");

            entity.ToTable("finalquizuseranswer");

            entity.Property(e => e.FinalQuizUserAnswerId).HasColumnName("FinalQuizUserAnswerID");
            entity.Property(e => e.Answer).HasMaxLength(255);
            entity.Property(e => e.FinalQuizResultId).HasColumnName("FinalQuizResultID");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
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

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PRIMARY");

            entity.ToTable("course");

            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.ModuleId).HasColumnName("ModuleID");
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
            
            // Configure the relationship with Module
            entity.HasOne(d => d.Module)
                  .WithMany(p => p.Courses)
                  .HasForeignKey(d => d.ModuleId)
                  .HasConstraintName("FK_Course_Module_ModuleID");
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
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<Quizuseranswer>(entity =>
        {
            entity.HasKey(e => e.QuizUserAnswerId).HasName("PRIMARY");

            entity.ToTable("quizuseranswer");

            entity.Property(e => e.QuizUserAnswerId).HasColumnName("QuizUserAnswerID");
            entity.Property(e => e.Answer).HasMaxLength(500);
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
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
            entity.Property(e => e.Order).HasColumnName("Order");
            entity.Property(e => e.IsActive).HasColumnName("IsActive");
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
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
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

        modelBuilder.Entity<Studentenrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("PRIMARY");
            entity.ToTable("studentenrollment");
            entity.Property(e => e.EnrollmentId).HasColumnName("EnrollmentID");
            entity.Property(e => e.StudentId).HasMaxLength(450).HasColumnName("StudentID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.EnrolledAt).HasColumnType("datetime");
            entity.Property(e => e.ParentId).HasMaxLength(450).HasColumnName("ParentID");

            entity.HasIndex(e => new { e.StudentId, e.CourseId }, "uk_student_course_enrollment").IsUnique();

            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_studentenrollment_student");

            entity.HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_studentenrollment_course");

            entity.HasOne(e => e.Parent)
                .WithMany()
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_studentenrollment_parent");
        });

        modelBuilder.Entity<Quizresult>(entity =>
        {
            entity.HasKey(e => e.QuizResultId).HasName("PRIMARY");

            entity.ToTable("quizresult");

            entity.Property(e => e.QuizResultId).HasColumnName("QuizResultID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.Score).HasPrecision(18, 2);
            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.ModuleID).HasName("PRIMARY");
            entity.ToTable("module");
            entity.Property(e => e.ModuleID).HasColumnName("ModuleID");
            entity.Property(e => e.EmbeddingDataSubject).HasColumnName("EmbeddingDataSubject");
            entity.Property(e => e.ModuleName).HasMaxLength(255).HasColumnName("ModuleName");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.IsActive).HasColumnType("tinyint(1)").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Chapter>(entity =>
        {
            entity.HasKey(e => e.ChapterID).HasName("PRIMARY");
            entity.ToTable("chapter");
            entity.Property(e => e.ChapterID).HasColumnName("ChapterID");
            entity.Property(e => e.ModuleID).IsRequired();
            entity.Property(e => e.ChapterName).HasMaxLength(255);
            entity.Property(e => e.ChapterDescription).HasColumnType("text");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
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
            entity.Property(e => e.Mark).HasPrecision(18, 2);
            entity.Property(e => e.MockTestId).HasColumnName("MockTestID");
            entity.Property(e => e.PartialMark).HasMaxLength(255);
            entity.Property(e => e.QuestionContent).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Mocktestresult>(entity =>
        {
            entity.HasKey(e => e.MockTestResultId).HasName("PRIMARY");

            entity.ToTable("mocktestresult");

            entity.Property(e => e.MockTestResultId).HasColumnName("MockTestResultID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.MockTestId).HasColumnName("MockTestID");
            entity.Property(e => e.Score).HasPrecision(18, 2);
            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<Mocktestuseranswer>(entity =>
        {
            entity.HasKey(e => e.MockTestUserAnswerId).HasName("PRIMARY");

            entity.ToTable("mocktestuseranswer");

            entity.Property(e => e.MockTestUserAnswerId).HasColumnName("MockTestUserAnswerID");
            entity.Property(e => e.Answer).HasMaxLength(255);
            entity.Property(e => e.MockTestQuestionId).HasColumnName("MockTestQuestionID");
            entity.Property(e => e.MockTestResultId).HasColumnName("MockTestResultID");
            entity.Property(e => e.Score).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.PackageId).HasName("PRIMARY");

            entity.ToTable("package");

            entity.Property(e => e.PackageId).HasColumnName("PackageID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Userpackage>(entity =>
        {
            entity.HasKey(e => new { e.PackageId, e.UserId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("userpackage");

            entity.Property(e => e.PackageId).HasColumnName("PackageID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Createslot>(entity =>
        {
            entity.HasKey(e => e.CreateSlotId).HasName("PRIMARY");

            entity.ToTable("createslot");

            entity.Property(e => e.TeacherId).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}