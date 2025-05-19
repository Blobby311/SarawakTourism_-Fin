using Microsoft.EntityFrameworkCore;
using SarawakTourismApi.Models;

namespace SarawakTourism.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TouristSpot> TouristSpots { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Post>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            // Configure unique constraint for PostLike
            modelBuilder.Entity<PostLike>()
                .HasIndex(pl => new { pl.PostId, pl.Username })
                .IsUnique();
        }
    }
} 