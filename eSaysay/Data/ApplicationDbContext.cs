using eSaysay.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using eSaysay.Models;

namespace eSaysay.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Analytics> Analytics { get; set; }
        public DbSet<AdaptiveLearning> AdaptiveLearning { get; set; }
        public DbSet<SecurityLog> SecurityLog { get; set; }
        public DbSet<Language> Language { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<InteractiveExercise> InteractiveExercises { get; set; }
        public DbSet<SpeechAssessment> SpeechAssessment { get; set; }
        public DbSet<UserProgress> UserProgress{ get; set; }
        public DbSet<UserResponse> UserResponse{ get; set; }
        public DbSet<Notification> Notification { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.MiddleName).HasMaxLength(100);
                entity.Property(e => e.Age);
                entity.Property(e => e.Gender).HasMaxLength(50);
                entity.Property(e => e.Birthday);
                entity.Property(e => e.RegistrationDate);
            });
        }
    }
}
