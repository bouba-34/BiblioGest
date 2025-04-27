// BiblioGestDbContext.cs (updated)

using System.IO;
using BiblioGest.Models;
using Microsoft.EntityFrameworkCore;

namespace BiblioGest.Data
{
    public class BiblioGestDbContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Create path for database in AppData folder
            string databaseFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BiblioGest");
                
            // Create directory if it doesn't exist
            Directory.CreateDirectory(databaseFolder);
            
            string dbPath = Path.Combine(databaseFolder, "BiblioGest.db");
            
            // Using SQLite for development and deployment
            optionsBuilder
                .UseSqlite($"Data Source={dbPath}")
                .EnableSensitiveDataLogging(false);  // Set to true for development
            
            // For SQL Server, use this instead (uncomment as needed):
            // optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BiblioGest;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure entity relationships
            
            // Book-Category relationship
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId);
                
            // Book-Loan relationship
            modelBuilder.Entity<Book>()
                .HasMany(b => b.Loans)
                .WithOne(l => l.Book)
                .HasForeignKey(l => l.BookId);
                
            // Member-Loan relationship
            modelBuilder.Entity<Member>()
                .HasMany(m => m.Loans)
                .WithOne(l => l.Member)
                .HasForeignKey(l => l.MemberId);
            
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            
            // Seed categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Fiction", Description = "Fictional literature" },
                new Category { Id = 2, Name = "Non-Fiction", Description = "Factual content" },
                new Category { Id = 3, Name = "Science", Description = "Scientific books" },
                new Category { Id = 4, Name = "History", Description = "Historical books" },
                new Category { Id = 5, Name = "Biography", Description = "Biographical content" }
            );
        }
    }
}