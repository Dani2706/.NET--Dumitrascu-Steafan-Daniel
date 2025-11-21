using OrdersManagementAPI.Features.Orders;

namespace OrdersManagementAPI.Logging;

public record OrderCreationMetrics()
{
    public string OperationId { get; init; } = string.Empty;
    public string OrderTitle { get; init; } = string.Empty;
    public string ISBN { get; init; } = string.Empty;
    public OrderCategory Category { get; init; }
    public TimeSpan ValidationDuration { get; init; }
    public TimeSpan DatabaseSaveDuration { get; init; }
    public TimeSpan TotalDuration { get; init; }
    public bool Success { get; init; }
    public string? ErrorReason { get; init; }
};