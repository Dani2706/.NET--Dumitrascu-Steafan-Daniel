using AutoMapper;
using OrdersManagementAPI.Features.Orders;
using OrdersManagementAPI.Features.Orders.dto;

namespace OrdersManagementAPI.Mappers.Resolvers;

public class AvailabilityStatusResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        if (!source.IsAvailable)
        {
            return "Out of Stock";
        }

        return source.StockQuantity switch
        {
            0 => "Unavailable",
            1 => "Last Copy",
            <= 5 => "Limited Stock",
            _ => "In Stock"
        };
    }
}