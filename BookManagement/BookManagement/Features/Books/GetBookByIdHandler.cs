using System.Text.Json;
using BookManagement.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Features.Books;

public class GetBookByIdHandler(BookManagementContext context)
{
    private BookManagementContext _context = context;

    public async Task<IResult> Handle(GetBookByIdRequest request)
    {
        var book = await _context.Books
            .Include(x => x.Pages)
            .FirstOrDefaultAsync(b => b.Id == request.Id);

        if (book is null)
        {
            return Results.NotFound();
        }
        
        Console.WriteLine(JsonSerializer.Serialize(book));

        return Results.Ok(book);
    }
}