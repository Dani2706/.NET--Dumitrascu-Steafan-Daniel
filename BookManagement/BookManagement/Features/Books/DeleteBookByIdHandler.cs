using BookManagement.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Features.Books;

public class DeleteBookByIdHandler(BookManagementContext context)
{
    private BookManagementContext _context= context;

    public async Task<IResult> Handle(DeleteBookByIdRequest request)
    {
        var result2 = await _context.Books.Where(book => book.Id == request.Id).ExecuteDeleteAsync();
        
        return result2 == 0 ? Results.NotFound() : Results.NoContent();
    }
}