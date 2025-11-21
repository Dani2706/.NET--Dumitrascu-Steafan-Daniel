using AutoMapper;
using OrdersManagementAPI.Features.Orders;
using OrdersManagementAPI.Features.Orders.dto;

namespace OrdersManagementAPI.Mappers.Resolvers;

public class AuthorInitialsResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        List<string> authorNames = source.Author.Split(' ').ToList();
        int numberOfNames = authorNames.Count;

        return numberOfNames switch
        {
            1 => authorNames[0][..1] + '.',
            > 1 => authorNames[0][..1].ToUpper() + ". " + authorNames.Last()[..1].ToUpper() +  '.',
            _ => "?"
        };
    }
}