using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EcoStudy.Models;

namespace EcoStudy.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Chapter> Chapters => Set<Chapter>();
        public DbSet<Note> Notes => Set<Note>();
        public DbSet<ImportantQuestion> ImportantQuestions => Set<ImportantQuestion>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Chapter configurations
            builder.Entity<Chapter>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(150);
                entity.Property(c => c.SubjectArea).HasMaxLength(100);
            });

            // Note configurations
            builder.Entity<Note>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
                entity.Property(n => n.Description).HasMaxLength(2000);
                entity.Property(n => n.FileName).IsRequired().HasMaxLength(255);
                entity.Property(n => n.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(n => n.FileType).IsRequired().HasMaxLength(20);

                entity.HasOne(n => n.Chapter)
                      .WithMany(c => c.Notes)
                      .HasForeignKey(n => n.ChapterId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.Teacher)
                      .WithMany(u => u.AuthoredNotes)
                      .HasForeignKey(n => n.TeacherId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ImportantQuestion configurations
            builder.Entity<ImportantQuestion>(entity =>
            {
                entity.HasKey(q => q.Id);
                entity.Property(q => q.Question).IsRequired();
                entity.Property(q => q.QuestionType).IsRequired().HasMaxLength(50);

                entity.HasOne(q => q.Chapter)
                      .WithMany(c => c.ImportantQuestions)
                      .HasForeignKey(q => q.ChapterId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
