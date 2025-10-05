using BusinessObject.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Repository
{
    public class IGCSEContext : IdentityDbContext<Account>
    {
        public IGCSEContext(DbContextOptions<IGCSEContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.UserProfile)
                .WithOne(s => s.Account)
                .HasForeignKey<UserProfile>(s => s.AccountID);
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

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
