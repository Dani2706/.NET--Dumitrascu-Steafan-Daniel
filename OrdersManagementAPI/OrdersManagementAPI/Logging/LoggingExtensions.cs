namespace OrdersManagementAPI.Logging;

public class LoggingExtensions
{
    public static void LogOrderCreationMetrics(ILogger logger, OrderCreationMetrics orderCreationMetrics)
    {
        logger.LogInformation(
            "OperationId={OperationId} | OrderTitle={OrderTitle} | ISBN={ISBN} | Category={Category} | " +
            "ValidationMs={ValidationMs} | DatabaseMs={DatabaseMs} | TotalMs={TotalMs} | Success={Success} | Error={ErrorReason}",
            orderCreationMetrics.OperationId,
            orderCreationMetrics.OrderTitle,
            orderCreationMetrics.ISBN,
            orderCreationMetrics.Category,
            orderCreationMetrics.ValidationDuration.TotalMilliseconds,
            orderCreationMetrics.DatabaseSaveDuration.TotalMilliseconds,
            orderCreationMetrics.TotalDuration.TotalMilliseconds,
            orderCreationMetrics.Success,
            orderCreationMetrics.ErrorReason ?? "-"
        );
    }
}