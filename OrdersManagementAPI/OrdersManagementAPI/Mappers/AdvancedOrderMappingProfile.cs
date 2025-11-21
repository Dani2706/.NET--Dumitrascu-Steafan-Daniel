using OrdersManagementAPI.Features.Orders;
using OrdersManagementAPI.Features.Orders.dto;

namespace OrdersManagementAPI.Mappers;

using AutoMapper;
using Features;
using Resolvers;

public class AdvancedOrderMappingProfile : Profile
{
    public AdvancedOrderMappingProfile()
    {
        CreateMap<CreateOrderProfileRequest, Order>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.StockQuantity > 0))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<Order, OrderProfileDto>()
            .ForMember(dest => dest.CategoryDisplayName, opt => opt.MapFrom<CategoryDisplayNameResolver>())
            .ForMember(dest => dest.FormattedPrice, opt => opt.MapFrom<PriceFormatterResolver>())
            .ForMember(dest => dest.PublishedAge, opt => opt.MapFrom<PublishedAgeResolver>())
            .ForMember(dest => dest.AvailabilityStatus, opt => opt.MapFrom<AvailabilityStatusResolver>())
            .ForMember(dest => dest.AuthorInitials, opt => opt.MapFrom<AuthorInitialsResolver>());
    }
}