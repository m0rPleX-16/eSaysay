using eSaysay.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eSaysay.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Analytics> Analytics { get; set; }
        public DbSet<AdaptiveLearning> AdaptiveLearning { get; set; }
        public DbSet<SecurityLog> SecurityLog { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<Language> Language { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<InteractiveExercise> InteractiveExercises { get; set; }
        public DbSet<SpeechAssessment> SpeechAssessment { get; set; }
        public DbSet<UserBadge> UserBadges { get; set; }
        public DbSet<UserProgress> UserProgress{ get; set; }
        public DbSet<UserResponse> UserResponse{ get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

    }
}
