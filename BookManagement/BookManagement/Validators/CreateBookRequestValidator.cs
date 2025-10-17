using BookManagement.Features.Books;
using BookManagement.Features.Pages;

namespace BookManagement.Validators;

using FluentValidation;

public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");
        
        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(100).WithMessage("Author cannot exceed 100 characters.");
        
        RuleFor(x => x.Year)
            .InclusiveBetween(1450, DateTime.Now.Year)
            .WithMessage($"Year must be between 1450 and {DateTime.Now.Year}.");
        
        RuleFor(x => x.Pages)
            .NotNull().WithMessage("Pages list cannot be null.")
            .Must(p => p.Count > 0).WithMessage("Book must have at least one page.");
        
        RuleForEach(x => x.Pages).SetValidator(new PageValidator());
    }
}
public class PageValidator : AbstractValidator<Page>
{
    public PageValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Page body cannot be empty.")
            .MaximumLength(5000).WithMessage("Page body is too long.");
    }
}
