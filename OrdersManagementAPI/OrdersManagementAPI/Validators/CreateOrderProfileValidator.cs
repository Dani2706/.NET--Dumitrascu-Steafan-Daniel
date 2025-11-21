using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrdersManagementAPI.Features.Orders;
using OrdersManagementAPI.Persistence;

namespace OrdersManagementAPI.Validators;

public class CreateOrderProfileValidator : AbstractValidator<CreateOrderProfileRequest>
{
    private OrderManagementContext _context;
    private ILogger<CreateOrderProfileValidator> _logger;
    private List<string> _childrenRestrictedWords;
    private List<string> _technicalKeywords;
    private List<string> _inappropriateWords;
    private List<string> _imageExtensions;
    
    public CreateOrderProfileValidator(OrderManagementContext context, ILogger<CreateOrderProfileValidator> logger)
    {
        _context = context;
        _logger = logger;
        
        _childrenRestrictedWords = ["violence", "adult", "gore", "curse"];
        _technicalKeywords =["guide", "reference", "manual", "tutorial", "advanced", "technology", "programming"];
        _inappropriateWords = ["violence", "adult", "gore", "curse"];
        _imageExtensions = ["jpeg", "png", "jpg", "gif", "webp"];
        
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(1).MaximumLength(200).WithMessage("Title must be between 1 and 200 characters.")
            .Must(BeValidTitle).WithMessage("Title contains inappropriate content.")
            .MustAsync(BeUniqueTitle).WithMessage("A title with the same author already exists.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MinimumLength(2).MaximumLength(100).WithMessage("Author must be between 2 and 100 characters.")
            .Must(BeValidAuthorName).WithMessage("Author contains invalid characters (only letters, spaces, hyphens, apostrophes and dots are allowed).");

        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("ISBN is required.")
            .Must(BeValidISBN).WithMessage("ISBN must be a valid ISBN-10 or ISBN-13 (digits, hyphens allowed).")
            .MustAsync(BeUniqueISBN).WithMessage("ISBN already exists in the system.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category is not valid.");

        RuleFor(x => x.Price)
            .GreaterThan(0m).WithMessage("Price must be greater than 0.")
            .LessThan(10000m).WithMessage("Price must be less than 10,000.");

        RuleFor(x => x.PublishedDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Published date cannot be in the future.")
            .GreaterThanOrEqualTo(new DateTime(1400, 1, 1)).WithMessage("Published date cannot be before year 1400.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.")
            .LessThanOrEqualTo(100000).WithMessage("Stock quantity cannot exceed 100,000.");

        RuleFor(x => x.CoverImageUrl)
            .Must(cover => string.IsNullOrWhiteSpace(cover) || BeValidImageUrl(cover))
            .WithMessage("CoverImageUrl must be a valid HTTP/HTTPS image URL ending with .jpg, .jpeg, .png, .gif, or .webp.");

        When(x => x.Category == OrderCategory.Technical, () =>
        {
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(20m).WithMessage("Technical orders must have a minimum price of $20.00.");

            RuleFor(x => x.Title)
                .Must(ContainsTechnicalKeyword)
                .WithMessage("Technical orders must contain technical keywords in the title.");

            RuleFor(x => x.PublishedDate)
                .GreaterThanOrEqualTo(DateTime.UtcNow.AddYears(-5)).WithMessage("Technical orders must be published within the last 5 years.");
        });
        
        When(x => x.Category == OrderCategory.Children, () =>
        {
            RuleFor(x => x.Price)
                .LessThanOrEqualTo(50m).WithMessage("Children's orders must have a maximum price of $50.00.");

            RuleFor(x => x.Title)
                .Must(TitleSuitableForChildren)
                .WithMessage("Children's title contains restricted or inappropriate content.");
        });
        
        When(x => x.Category == OrderCategory.Fiction, () =>
        {
            RuleFor(x => x.Author)
                .MinimumLength(5).WithMessage("Fiction author name must be at least 5 characters (full name required).");
        });
        
        RuleFor(x => x).Custom((req, validationContext) =>
        {
            if (req.Price > 100m && req.StockQuantity > 20)
            {
                validationContext.AddFailure("StockQuantity", "Expensive orders (>$100) must have limited stock (≤20 units).");
            }

            if (req.Category == OrderCategory.Technical && req.PublishedDate < DateTime.UtcNow.AddYears(-5))
            {
                validationContext.AddFailure("PublishedDate", "Technical orders must be published within the last 5 years.");
            }
        });
        
        RuleFor(x => x)
            .MustAsync(async (req, ct) => await PassBusinessRules(req, ct))
            .WithMessage("One or more business rules failed.");
    }

    private bool BeValidTitle(string title)
    {
        foreach (var word in _inappropriateWords)
        {
            if (title.ToLower().Contains(word))
            {
                return false;
            }
        }
        
        return true;
    }

    private async Task<bool> BeUniqueTitle(string title, CancellationToken ct)
    {
        return await _context.Orders.FirstOrDefaultAsync(order => order.Title == title, ct) == null;
    }

    private bool BeValidAuthorName(string author)
    {
        return Regex.IsMatch(author, @"^[\p{L}\s\-\.'`]+$");
    }

    public bool BeValidISBN(string isbn)
    {
        string trimmedIsbn = isbn.Replace("-", "").Replace(" ", "");
        
        return trimmedIsbn.Length is 10 or 13;
    }

    private async Task<bool> BeUniqueISBN(string isbn, CancellationToken ct)
    {
        return await _context.Orders.FirstOrDefaultAsync(order => order.ISBN == isbn, ct) == null;
    }

    private bool BeValidImageUrl(string imageUrl)
    {
        if (imageUrl.Contains('.'))
        {
            string extension = imageUrl.Substring(imageUrl.LastIndexOf('.'));
            foreach (var validExtension in _imageExtensions)
            {
                if (extension.Contains(validExtension))
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    private bool ContainsTechnicalKeyword(string title)
    {
        var lower =  title.ToLower();
        foreach (var word in _technicalKeywords)
        {
            if (lower.Contains(word))
            {
                return true;
            }
        }
        
        return false;
    }

    private bool TitleSuitableForChildren(string title)
    {
        string lower = title.ToLower();
        foreach (var word in _childrenRestrictedWords)
        {
            if (lower.Contains(word))
            {
                return false;
            }
        }
        
        return true;
    }

    private async Task<bool> PassBusinessRules(CreateOrderProfileRequest request, CancellationToken cancellationToken)
    {
        var dayStart = DateTime.UtcNow.Date;
        var dayEnd = dayStart.AddDays(1);
        var todaysCount = await _context.Orders
            .AsNoTracking()
            .CountAsync(o => o.CreatedAt >= dayStart && o.CreatedAt < dayEnd, cancellationToken);

        if (todaysCount >= 500)
        {
            _logger.LogWarning("Business rule failed: daily order limit reached ({Count}/500)", todaysCount);
            return false;
        }
        
        if (request is { Category: OrderCategory.Technical, Price: < 20m })
        {
            _logger.LogWarning("Business rule failed: technical order below minimum price ({Price}) for Title=\"{Title}\"", request.Price, request.Title);
            return false;
        }

        if (request.Category == OrderCategory.Children)
        {
            var loweredTitle = (request.Title ?? string.Empty).ToLowerInvariant();
            var forbidden = _childrenRestrictedWords.Any(w => Regex.IsMatch(loweredTitle, $@"\b{Regex.Escape(w.ToLowerInvariant())}\b"));
            if (forbidden)
            {
                _logger.LogWarning("Business rule failed: children's order Title contains restricted content. Title=\"{Title}\"", request.Title);
                return false;
            }
        }

        if (request is { Price: > 500m, StockQuantity: > 10 })
        {
            _logger.LogWarning("Business rule failed: high-value order exceeds stock limit. Price={Price} Stock={Stock}", request.Price, request.StockQuantity);
            return false;
        }

        _logger.LogDebug("All business rules passed for Title=\"{Title}\" ISBN={ISBN} Author=\"{Author}\"", request.Title, request.ISBN, request.Author);
        return true;
    }
}