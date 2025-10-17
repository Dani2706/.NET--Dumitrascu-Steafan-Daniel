using BookManagement.Features.Pages;

namespace BookManagement.Features.Books;

public record CreateBookRequest(string Title, string Author, int Year, List<Page> Pages);