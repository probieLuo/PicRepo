using Microsoft.EntityFrameworkCore;
using PicRepo.Client.Models;

namespace PicRepo.Client.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<UploadHistory> UploadHistorys { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public AppDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=app.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UploadHistory>(entity =>
            {
                entity.ToTable("UploadHistory");
                entity.HasKey(h => h.Id);
                entity.HasIndex(h => h.UploadTime);
            });
        }
    }
}