using Moq;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrdersManagementAPI.Features.Orders;
using OrdersManagementAPI.Features.Orders.dto;
using OrdersManagementAPI.Mappers;
using OrdersManagementAPI.Persistence;
using OrdersManagementAPI.Validators;

namespace OrdersManagementAPI.Test;

public class CreateOrderHandlerIntegrationTests : IDisposable
    {
        private readonly OrderManagementContext _context;
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<CreateOrderHandler>> _loggerMock;
        private readonly IValidator<CreateOrderProfileRequest> _validator;
        private readonly CreateOrderHandler _handler;
        private readonly string _dbName;

        public CreateOrderHandlerIntegrationTests()
        {
            _dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<OrderManagementContext>()
                .UseInMemoryDatabase(_dbName)
                .Options;

            _context = new OrderManagementContext(options);
            
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AdvancedOrderMappingProfile>();
            }, new LoggerFactory());
            _mapper = mapperConfig.CreateMapper();
            
            _loggerMock = new Mock<ILogger<CreateOrderHandler>>();
            _validator = new CreateOrderProfileValidator(_context, new LoggerFactory().CreateLogger<CreateOrderProfileValidator>());
            
            
            _handler = new CreateOrderHandler(_context, _loggerMock.Object, _mapper, _validator);
        }

        [Fact]
        public async Task Handle_ValidTechnicalOrderRequest_CreatesOrderWithCorrectMappings()
        {
            var request = new CreateOrderProfileRequest
            {
                ISBN = "TECH-123459",
                Title = "Advanced Networking for Professionals",
                Author = "Jane Doe",
                Price = 45.00m,
                PublishedDate = DateTime.UtcNow.AddYears(-2),
                StockQuantity = 5,
                CoverImageUrl = null,
                Category = OrderCategory.Technical
            };

            var result = await _handler.Handle(request);
            
            var createdResult = Assert.IsType<Created<OrderProfileDto>>(result);
            var dto = Assert.IsType<OrderProfileDto>(createdResult.Value);

            Assert.Equal("Technical & Professional", dto.CategoryDisplayName);

            Assert.Equal("J. D.", dto.AuthorInitials);

            var expectedAge = 2;
            Assert.Equal($"{expectedAge} years old", dto.PublishedAge);

            Assert.False(string.IsNullOrEmpty(dto.FormattedPrice));
            Assert.True(char.IsSymbol(dto.FormattedPrice[0]) || char.IsPunctuation(dto.FormattedPrice[0]));

            var expectedStatus = request.StockQuantity <= 20 ? "Limited Stock" : "In Stock";
            Assert.Equal(expectedStatus, dto.AvailabilityStatus);

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("OrderCreationStarted")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateISBN_ThrowsValidationExceptionWithLogging()
        {
            var existing = new Order
            {
                ISBN = "DUP-ISBN-001",
                Title = "Existing Book",
                Author = "Existing Author",
                Price = 30.0m,
                PublishedDate = DateTime.UtcNow.AddYears(-1),
                StockQuantity = 10,
                Category = OrderCategory.Fiction
            };
            _context.Orders.Add(existing);
            await _context.SaveChangesAsync();
        
            var request = new CreateOrderProfileRequest
            {
                ISBN = existing.ISBN,
                Title = "New Duplicate",
                Author = "New Author",
                Price = 35.0m,
                PublishedDate = DateTime.UtcNow,
                StockQuantity = 5,
                Category = OrderCategory.Fiction
            };
        
            var ex = await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await _handler.Handle(request);
            });
        
            Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
        
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning || l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("OrderCreationFailed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_ChildrenOrderRequest_AppliesDiscountAndConditionalMapping()
        {
            var request = new CreateOrderProfileRequest
            {
                ISBN = "CHILD-00170",
                Title = "Fun Stories for Kids",
                Author = "Amy Kid",
                Price = 40.00m,
                PublishedDate = DateTime.UtcNow.AddYears(-3),
                StockQuantity = 50,
                Category = OrderCategory.Children
            };
        
            var result = await _handler.Handle(request);
            
            var createdResult = Assert.IsType<Created<OrderProfileDto>>(result);
            var dto = Assert.IsType<OrderProfileDto>(createdResult.Value);
        
            Assert.Equal("Children's Orders", dto.CategoryDisplayName);
        
            var expectedPrice = request.Price * 0.9m;
            Assert.Equal(expectedPrice, dto.Price);
        
            Assert.Null(dto.CoverImageUrl);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }