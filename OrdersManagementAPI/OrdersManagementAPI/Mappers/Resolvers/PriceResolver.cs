using AutoMapper;
using OrdersManagementAPI.Features.Orders;
using OrdersManagementAPI.Features.Orders.dto;

namespace OrdersManagementAPI.Mappers.Resolvers;

public class PriceResolver: IValueResolver<Order, OrderProfileDto, decimal>
{
    public decimal Resolve(Order source, OrderProfileDto destination, decimal destMember, ResolutionContext context)
    {
        return source.Category switch
        {
            OrderCategory.Children => source.Price * 0.9m,
            _ => source.Price
        };
    }
}