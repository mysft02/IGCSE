using BusinessObject.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148.
        => optionsBuilder.UseMySql("server=127.0.0.1;port=3306;database=IGCSE;user=root;password=12345;allow user variables=True", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.43-mysql"));

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
            entity.ToTable("refreshtoken");
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
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
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
            entity.Property(e => e.StudentId).HasMaxLength(255).HasColumnName("StudentID");
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}