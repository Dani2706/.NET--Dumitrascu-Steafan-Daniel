using BookManagement.Features.Pages;

namespace BookManagement.Features.Books;

public class Book(string title, string author, int year)
{ 
    public int Id { get; set; }
    public string Title { get; set; } = title;
    public string Author { get; set; }  = author;
    public int Year { get; set; }   = year;
    public List<Page> Pages { get; set; } = new ();
};