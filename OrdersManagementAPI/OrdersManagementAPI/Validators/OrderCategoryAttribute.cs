using System.ComponentModel.DataAnnotations;
using OrdersManagementAPI.Features.Orders;

namespace OrdersManagementAPI.Validators;

public class OrderCategoryAttribute : ValidationAttribute
{
    private readonly HashSet<OrderCategory> _allowed;

    public OrderCategoryAttribute(OrderCategory[] allowedCategories)
    {
        _allowed = new HashSet<OrderCategory>(allowedCategories);
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not OrderCategory category)
        {
            var allowedList = _allowed.Count != 0 ? string.Join(", ", _allowed) : "none";
            return new ValidationResult($"Invalid category. Allowed categories: {allowedList}");
        }

        if (!_allowed.Contains(category))
        {
            var allowedList = _allowed.Count != 0  ? string.Join(", ", _allowed) : "none";
            return new ValidationResult($"Category '{category}' is not allowed. Allowed categories: {allowedList}");
        }

        return ValidationResult.Success;
    }
}