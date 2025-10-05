using BusinessObject.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Repository.BaseRepository
{
    public class IGCSEContext : IdentityDbContext<Account>
    {
        public IGCSEContext(DbContextOptions<IGCSEContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Account <-> UserProfile (existing)
            modelBuilder.Entity<Account>()
                .HasOne(a => a.UserProfile)
                .WithOne(s => s.Account)
                .HasForeignKey<UserProfile>(s => s.AccountID);

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");

                entity.HasKey(e => e.CategoryID);

                entity.Property(e => e.CategoryName)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Description)
                      .HasMaxLength(4000);

                entity.Property(e => e.IsActive)
                      .IsRequired()
                      .HasDefaultValue(true);

                // One Category has many Courses
                entity.HasMany(e => e.Courses)
                      .WithOne(c => c.Category)
                      .HasForeignKey(c => c.CategoryID)
                      .OnDelete(DeleteBehavior.Restrict);

                // optional: index on name
                entity.HasIndex(e => e.CategoryName);
            });

            // Course configuration (Course is in BusinessObject.Model)
            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Courses");

                entity.HasKey(e => e.CourseID);

                entity.Property(e => e.CourseName)
                      .IsRequired()
                      .HasMaxLength(250);

                entity.Property(e => e.Description)
                      .HasMaxLength(4000);

                // enum -> int
                entity.Property(e => e.Status)
                      .HasConversion<int>()
                      .IsRequired()
                      .HasDefaultValue(CourseStatus.Draft);

                entity.Property(e => e.Price)
                      .HasColumnType("decimal(18,2)")
                      .HasDefaultValue(0m);

                entity.Property(e => e.ImageUrl)
                      .HasMaxLength(1000);

                // timestamps default to UTC now in DB
                entity.Property(e => e.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("GETUTCDATE()");

                // FK to Category (explicit property Course.CategoryID)
                entity.Property(e => e.CategoryID)
                      .IsRequired();

                entity.HasIndex(e => e.CourseName);
                entity.HasIndex(e => e.CategoryID);
            });
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(GetConnectionString());
            }
        }

        private string GetConnectionString()
        {
            IConfiguration config = new ConfigurationBuilder()
                 .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "IGCSE"))
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                 .Build();
            var strConn = config["ConnectionStrings:DbConnection"];
            return strConn;
        }
    }
}
