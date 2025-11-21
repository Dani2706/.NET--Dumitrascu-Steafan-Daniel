using Microsoft.EntityFrameworkCore;
using OrdersManagementAPI.Features.Orders;

namespace OrdersManagementAPI.Persistence;  

public class OrderManagementContext(DbContextOptions<OrderManagementContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
}