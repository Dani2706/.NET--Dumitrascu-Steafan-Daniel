using BookManagement.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Features.Books;

public class UpdateBookByIdHandler(BookManagementContext context)
{
    private BookManagementContext _context= context;

    public async Task<IResult> Handle(int Id, UpdateBookByIdRequest request)
    {
        var book = await _context.Books.FindAsync(Id);
        if (book is null)
        {
            return Results.NotFound();
        }
        
        book.Title =  request.Title;
        book.Year =  request.Year;
        await _context.SaveChangesAsync();
        return Results.NoContent();
    }
}