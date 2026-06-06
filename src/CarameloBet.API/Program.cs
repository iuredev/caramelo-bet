using Serilog;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Prometheus;
using MassTransit;
using FluentValidation;
using MapsterMapper;
using CarameloBet.API.Middlewares;
using CarameloBet.API.Models;
using CarameloBet.API.Extensions;
using CarameloBet.Infrastructure.Persistence;
using CarameloBet.Infrastructure.Persistence.Auth;
using CarameloBet.Infrastructure.Persistence.Game;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);


    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CarameloBetPolicy", policy =>
        {
            policy
                .WithOrigins(
                    builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:3000")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // Prometheus
    builder.Services.AddMetrics();
    // OpenTelemetry
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("CarameloBet.API"))
        .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation()
            .AddOtlpExporter(otlp =>
           {
               otlp.Endpoint = new Uri(builder.Configuration["Jaeger:Endpoint"]
                               ?? "http://localhost:4317");

           }));
    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console()
        .WriteTo.Seq(context.Configuration["Seq:ServeUrl"] ?? "http://localhost:5341")
    );
    // RabbitMQ
    builder.Services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/",
                        host =>
                        {
                            host.Username(builder.Configuration["RabbitMQ:Username"] ?? "caramelo");
                            host.Password(builder.Configuration["RabbitMQ:Password"] ?? "caramelo123");
                        });

                    cfg.UseMessageRetry(retry =>
                        {
                            retry.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
                        });
                    cfg.UseDelayedRedelivery(retry =>
                        {
                            retry.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30));
                        });
                    cfg.ConfigureEndpoints(context);
                });
        });

    //FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // Mapster
    builder.Services.AddScoped<IMapper, Mapper>();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    // Database
    builder.Services.AddDatabaseContexts(builder.Configuration);


    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseCors("CarameloBetPolicy");

    // Prometheus
    app.UseMetricServer();
    app.UseHttpMetrics();
    app.UseExceptionHandler();


    app.MapGet("/", () =>
        {
            return "Hello, World!";
        });

    app.MapGet("/test-error", () =>
        {
            throw new Exception();
        });
    app.MapGet("/api", () => ApiResponse<string>.Ok("V1 API is running"));
    app.MapGet("/health", () => ApiResponse<string>.Ok("CarameloBet API is running"));



    using (var scope = app.Services.CreateScope())
    {
        var authContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var gameContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();

        await SeedData.SeedAsync(authContext, gameContext);
    }


    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}
