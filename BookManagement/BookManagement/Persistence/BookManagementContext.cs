using Microsoft.EntityFrameworkCore;
using BookManagement.Features.Books;
using BookManagement.Features.Pages;

namespace BookManagement.Persistence;

public class BookManagementContext(DbContextOptions<BookManagementContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; }
    
    public DbSet<Page> Pages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>()
            .OwnsMany(x => x.Pages, p =>
            {
                p.WithOwner().HasForeignKey("BookId");
            })
            .Property(b => b.Id)
            .ValueGeneratedOnAdd();
    }
    
    
}