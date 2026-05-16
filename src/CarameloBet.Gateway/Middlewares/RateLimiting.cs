using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using StackExchange.Redis;


namespace CarameloBet.Gateway.Middlewares;

// in memory rate limiting. I create it to study
// public static class RateLimitingExtensions
// {
//     public static IServiceCollection AddRateLimiting(
//         this IServiceCollection services,
//         IConfiguration configuration)
//     {
//         services.AddRateLimiter(options =>
//             {
//                 options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
//                     {
//                         var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";


//                         return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ =>
//                         {
//                             return new FixedWindowRateLimiterOptions
//                             {
//                                 PermitLimit = 100,
//                                 Window = TimeSpan.FromSeconds(10),
//                                 QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
//                                 QueueLimit = 0
//                             };
//                         });

//                     });

//                 options.AddFixedWindowLimiter("auth", opt =>
//                     {
//                         opt.PermitLimit = 5;
//                         opt.Window = TimeSpan.FromMinutes(1);
//                         opt.QueueLimit = 0;
//                     });

//                 options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

//             });

//         return services;
//     }
// }


// Redis rate limiting middleware
public class RedisRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDatabase _redis;
    private readonly int _permitLimit;
    private readonly int _windowSeconds;

    public RedisRateLimitingMiddleware(
        RequestDelegate next,
        IConnectionMultiplexer redis,
        IConfiguration configuration)
    {
        _next = next;
        _redis = redis.GetDatabase();
        _permitLimit = configuration.GetValue<int>("RateLimit:PermitLimit", 100);
        _windowSeconds = configuration.GetValue<int>("RateLimit:WindowSeconds", 10);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"rate_limit:{clientIp}";

        var current = await _redis.StringIncrementAsync(key);

        if (current == 1)
            await _redis.KeyExpireAsync(key, TimeSpan.FromSeconds(_windowSeconds));

        if (current > _permitLimit)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = _windowSeconds.ToString();
            await context.Response.WriteAsync("Too many requests. Please try again later.");
            return;
        }

        await _next(context);
    }
}

public static class RedisRateLimitingExtensions
{
    public static IServiceCollection AddRedisRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(connectionString));

        return services;
    }

    public static IApplicationBuilder UseRedisRateLimiting(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<RedisRateLimitingMiddleware>();
    }
}
