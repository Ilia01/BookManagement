using BookManager.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookManager.db
{
    public class BookDb : DbContext
    {
        public BookDb(DbContextOptions<BookDb> options)
            : base(options) { }

        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Book>()
                .HasIndex(b => b.Title)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        }
    }
}
