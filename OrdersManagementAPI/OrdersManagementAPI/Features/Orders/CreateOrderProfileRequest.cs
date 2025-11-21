using OrdersManagementAPI.Validators;

namespace OrdersManagementAPI.Features.Orders;

public class CreateOrderProfileRequest
{
    public string Title { get; set; }
    public string Author { get; set; }
    [ValidISBN]
    public string ISBN { get; set; }
    [OrderCategory([OrderCategory.Fiction, OrderCategory.NonFiction, OrderCategory.Technical, OrderCategory.Children])]
    public OrderCategory Category { get; set; }
    [PriceRange(0.0, 5000.0)]
    public decimal Price { get; set; }
    public DateTime PublishedDate { get; set; }
    public string? CoverImageUrl { get; set; }
    public int StockQuantity { get; set; } = 1;
}