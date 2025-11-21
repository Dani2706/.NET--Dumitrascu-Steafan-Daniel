using AutoMapper;
using OrdersManagementAPI.Features.Orders;
using OrdersManagementAPI.Features.Orders.dto;

namespace OrdersManagementAPI.Mappers.Resolvers;

public class PublishedAgeResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        var days = DateTime.UtcNow - source.PublishedDate;
        
        return days.TotalDays switch
        {
            < 30 => "New Release",
            < 365 => (int) days.TotalDays / 30 + " months old",
            < 1825 => (int) days.TotalDays / 365 + " years old", 
            1825 => "Classic",
            _ => "Uncategorized"
        };
    }
}