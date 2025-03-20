using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var openTelemetryResourceBuilder = ResourceBuilder.CreateDefault().AddService("serviceTwo");

var openTelemetryEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

// Build a resource configuration action to set service information.
Action<ResourceBuilder> configureResource = r => r.AddService(
    serviceName: "serviceTwo",
    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
    serviceInstanceId: Environment.MachineName);

// Configure OpenTelemetry tracing & metrics with auto-start using the
// AddOpenTelemetry extension from OpenTelemetry.Extensions.Hosting.
builder.Services.AddOpenTelemetry()
.ConfigureResource(configureResource)
.WithTracing(builder => 
{
    builder
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri(openTelemetryEndpoint + "/v1/traces");
        options.Protocol = OtlpExportProtocol.HttpProtobuf;
    });
})
.WithMetrics(builder =>
{
    builder
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri(openTelemetryEndpoint + "/v1/metrics");
        options.Protocol = OtlpExportProtocol.HttpProtobuf;
    });
});

// Clear default logging providers used by WebApplication host.
builder.Logging.ClearProviders();

// Configure OpenTelemetry logging with the OpenTelemetryLoggerProvider.
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.SetResourceBuilder(openTelemetryResourceBuilder);
    options.AddConsoleExporter();
    options.AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri(openTelemetryEndpoint + "/v1/logs");
        options.Protocol = OtlpExportProtocol.HttpProtobuf;
    });
});

var app = builder.Build();

HttpClient client = new HttpClient();


app.MapGet("/one", ([FromServices]ILogger<Program> logger) =>
{
  string serviceThreeEndpoint = Environment.GetEnvironmentVariable("SERVICE_THREE_ENDPOINT");
  logger.LogInformation("Connecting to: " + serviceThreeEndpoint);
  client.GetAsync(serviceThreeEndpoint + "/one");
});

app.MapGet("/two", ([FromServices]ILogger<Program> logger) =>
{
  string serviceThreeEndpoint = Environment.GetEnvironmentVariable("SERVICE_THREE_ENDPOINT");
  logger.LogInformation("The server is not started correctly");
  logger.LogInformation("Restarting server...");
  logger.LogInformation("Server is running");
  logger.LogInformation("Operation pending for user 198");
  logger.LogError("Operation failed for user 931");
  logger.LogInformation("Operation completed for user 425");
  logger.LogInformation("Operation started for user 629");
  logger.LogInformation("Operation completed for user 349");
  logger.LogError("Operation failed for user 901");
  logger.LogInformation("Operation completed for user 425");
  logger.LogInformation("Operation started for user 629");
  logger.LogInformation("Operation completed for user 349");
  logger.LogError("Operation failed for user 982");
  logger.LogInformation("Operation started for user 927");
  logger.LogInformation("Test log in function");
  logger.LogInformation("Operation started for user 943");
  logger.LogError("Operation failed for user 101");
  logger.LogError("Operation failed for user 647");
  logger.LogInformation("Operation completed for user 22");
  logger.LogWarning("Operation pending for user 943");
  logger.LogError("Operation failed for user 251");
  logger.LogError("Operation failed for user 122");
  logger.LogError("Operation failed for user 152");
  logger.LogInformation("Operation pending for user 882");
  logger.LogInformation("Operation started for user 792");
  logger.LogError("Operation failed for user 293");
  logger.LogError("Operation failed for user 334");
  logger.LogWarning("Operation pending for user 914");
  logger.LogInformation("Operation started for user 579");
  client.GetAsync(serviceThreeEndpoint + "/two");
});

app.Run();