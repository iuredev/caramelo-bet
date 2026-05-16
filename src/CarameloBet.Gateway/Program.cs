using Serilog;
using Prometheus;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console()
        .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));

    // YARP
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    // Prometheus
    builder.Services.AddMetrics();

    // OpenTelemetry
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService("CarameloBet.Gateway"))
        .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation()
            .AddOtlpExporter(otlp =>
            {
                otlp.Endpoint = new Uri(builder.Configuration["Jaeger:Endpoint"]
                    ?? "http://localhost:4317");
            }));

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseMetricServer();
    app.UseHttpMetrics();

    app.MapReverseProxy();

    app.MapGet("/", () => "CarameloBet Gateway is running");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway failed to start");
}
finally
{
    Log.CloseAndFlush();
}
