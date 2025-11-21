namespace OrdersManagementAPI.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
    {
        var id = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString().Split('-').First();
        }

        context.Response.Headers["X-Correlation-ID"] = id;

        using (logger.BeginScope("CorrelationId:{CorrelationId}", id))
        {
            await _next(context);
        }
    }
}