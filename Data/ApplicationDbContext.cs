// ========================================
// Data/ApplicationDbContext.cs
// ========================================
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyBlogApi.Models;

namespace MyBlogApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // PostgreSQL-specific: Use lowercase table names (PostgreSQL convention)
            builder.Entity<User>().ToTable("users");
            builder.Entity<Post>().ToTable("posts");

            // Configure the relationship between User and Post
            builder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // PostgreSQL
            // This ensures all table and column names are lowercase
            foreach (var entity in builder.Model.GetEntityTypes())
            {
                // Replace table names
                entity.SetTableName(entity.GetTableName()?.ToLower());

                // Replace column names
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName().ToLower());
                }

                // Replace key names
                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key.GetName()?.ToLower());
                }

                // Replace foreign key names
                foreach (var key in entity.GetForeignKeys())
                {
                    key.SetConstraintName(key.GetConstraintName()?.ToLower());
                }

                // Replace index names
                foreach (var index in entity.GetIndexes())
                {
                    index.SetDatabaseName(index.GetDatabaseName()?.ToLower());
                }
            }
        }
    }
}