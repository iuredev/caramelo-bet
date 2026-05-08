using Serilog;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Prometheus;
using MassTransit;
using FluentValidation;
using MapsterMapper;
using CarameloBet.API.Middleware;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);


    // Prometheus
    builder.Services.AddMetrics();

    // OpenTelemetry
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("CarameloBet.API"))
        .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation().AddOtlpExporter(otlp =>
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
