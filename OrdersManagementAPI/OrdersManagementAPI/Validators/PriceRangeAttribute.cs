using System.ComponentModel.DataAnnotations;

namespace OrdersManagementAPI.Validators;

public class PriceRangeAttribute(double minPrice, double maxPrice) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not decimal d)
        {
            return new ValidationResult("Price must be a decimal value");
        }


        if (d > (decimal)minPrice && d < (decimal)maxPrice)
        {
            return new ValidationResult($"Price must be between {minPrice} and {maxPrice}");
        }
        
        return ValidationResult.Success;
    }
}