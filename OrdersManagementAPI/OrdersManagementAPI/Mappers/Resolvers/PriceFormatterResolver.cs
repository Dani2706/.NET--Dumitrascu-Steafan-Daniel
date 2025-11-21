using AutoMapper;
using OrdersManagementAPI.Features.Orders;
using OrdersManagementAPI.Features.Orders.dto;

namespace OrdersManagementAPI.Mappers.Resolvers;

public class PriceFormatterResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        return source.Price.ToString("C2");
    }
}