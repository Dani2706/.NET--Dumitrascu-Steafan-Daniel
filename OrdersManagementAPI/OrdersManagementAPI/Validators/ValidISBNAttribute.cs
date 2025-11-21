using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace OrdersManagementAPI.Validators;

public class ValidISBNAttribute : ValidationAttribute, IClientModelValidator
{
    public void AddValidation(ClientModelValidationContext context)
    {
        throw new NotImplementedException();
    }

    public override bool IsValid(object? value)
    {
        if (value is not string s)
        {
            return false;
        }

        string isbn = s.Replace("-", "").Replace(" ", "");
        
        return isbn.Length is 10 or 13;
    }
}