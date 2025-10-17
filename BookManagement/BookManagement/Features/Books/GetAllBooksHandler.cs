using BookManagement.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Features.Books;

public class GetAllBooksHandler(BookManagementContext context)
{
    private readonly BookManagementContext _context = context;
    
    public async Task<IResult> Handle(int page, int pageSize)
    {
        var books = await _context.Books
            .Include(x => x.Pages)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return Results.Ok(books);
    }
}