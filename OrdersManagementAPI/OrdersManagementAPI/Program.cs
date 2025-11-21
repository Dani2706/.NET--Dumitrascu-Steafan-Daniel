using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrdersManagementAPI.Features.Orders;
using OrdersManagementAPI.Mappers;
using OrdersManagementAPI.Middleware;
using OrdersManagementAPI.Persistence;
using OrdersManagementAPI.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.IncludeScopes = true;
});
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orders API", Version = "v1" });
});

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<OrderManagementContext>(options =>
        options.UseInMemoryDatabase("ordersmanagement.TestDb"));
}
else
{
    builder.Services.AddDbContext<OrderManagementContext>(options =>
        options.UseSqlite("Data Source=ordersmanagement.db"));
}

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AdvancedOrderMappingProfile>(), typeof(AdvancedOrderMappingProfile));

builder.Services.AddScoped<CreateOrderHandler>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderProfileValidator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OrderManagementContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseMiddleware<CorrelationIdMiddleware>();

// app.UseHttpsRedirection();

app.MapPost("/orders", async (CreateOrderProfileRequest request, CreateOrderHandler handler) =>
    await handler.Handle(request));

app.Run();
