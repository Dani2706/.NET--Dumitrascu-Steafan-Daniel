using System.Diagnostics;
using AutoMapper;
using FluentValidation;
using OrdersManagementAPI.Features.Orders.dto;
using OrdersManagementAPI.Logging;
using OrdersManagementAPI.Mappers;
using OrdersManagementAPI.Persistence;
using OrdersManagementAPI.Validators;

namespace OrdersManagementAPI.Features.Orders;

public class CreateOrderHandler(
    OrderManagementContext context, 
    ILogger<CreateOrderHandler> logger, 
    IMapper mapper,
    IValidator<CreateOrderProfileRequest> validator)
{
    public async Task<IResult> Handle(CreateOrderProfileRequest request)
    {
        Stopwatch totalTime = Stopwatch.StartNew(); 
        Stopwatch validationTime = new Stopwatch();
        Stopwatch dbTime = new Stopwatch();
        string operationId = Guid.NewGuid().ToString().Split('-').First();

        try
        {
            using (logger.BeginScope("CreateOrderHandler:{operationId}", operationId))
            {
                logger.LogInformation(
                    "Title: {Title} '\n' Author: {Author} '\n' + Category: {Category} '\n' ISBN: {ISBN}",
                    request.Title, request.Author, request.Category, request.ISBN
                );

                // Validate request
                validationTime.Start();
                logger.LogInformation(LogEvents.IsbnValidationPerformed, "Validating request");

                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
                validationTime.Stop();


                // Validate stock
                logger.LogInformation(LogEvents.StockValidationPerformed, "Validating stock");



                var newOrder = mapper.Map<Order>(request);

                logger.LogInformation(LogEvents.DatabaseOperationCompleted,
                    "Saving order with id: {orderId} in database", newOrder.Id);
                dbTime.Start();
                await context.Orders.AddAsync(newOrder);
                await context.SaveChangesAsync();
                dbTime.Stop();
                logger.LogInformation(LogEvents.DatabaseOperationCompleted,
                    "Saved order with id: {orderId} in database", newOrder.Id);

                var response = mapper.Map<OrderProfileDto>(newOrder);

                var metrics = new OrderCreationMetrics()
                {
                    OperationId = operationId,
                    OrderTitle = newOrder.Title,
                    ISBN = newOrder.ISBN,
                    Category = newOrder.Category,
                    ValidationDuration = validationTime.Elapsed,
                    DatabaseSaveDuration = dbTime.Elapsed,
                    TotalDuration = totalTime.Elapsed,
                    Success = true,
                    ErrorReason = null
                };

                LoggingExtensions.LogOrderCreationMetrics(logger, metrics);

                return Results.Created("OrderProfile", response);
            }
        }
        catch (Exception e)
        {
            validationTime.Stop();
            dbTime.Stop();
            totalTime.Stop();
            var metrics = new OrderCreationMetrics()
            {
                OperationId = operationId,
                OrderTitle = request.Title,
                ISBN = request.ISBN,
                Category = request.Category,
                ValidationDuration = validationTime.Elapsed,
                DatabaseSaveDuration = dbTime.Elapsed,
                TotalDuration = totalTime.Elapsed,
                Success = false,
                ErrorReason = e.Message
            };
            
            LoggingExtensions.LogOrderCreationMetrics(logger, metrics);

            throw e;
        }
    }
}