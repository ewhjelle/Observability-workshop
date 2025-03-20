using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var openTelemetryResourceBuilder = ResourceBuilder.CreateDefault().AddService("frontend");

var openTelemetryEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

// Build a resource configuration action to set service information.
Action<ResourceBuilder> configureResource = r => r.AddService(
    serviceName: "frontend",
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

app.UseDefaultFiles();
app.UseStaticFiles();

HttpClient client = new HttpClient();

app.MapGet("/one", ([FromServices]ILogger<Program> logger) =>
{
  string serviceTwoEndpoint = Environment.GetEnvironmentVariable("SERVICE_ONE_ENDPOINT");
  client.GetAsync(serviceTwoEndpoint + "/one");
});

app.MapGet("/two", ([FromServices]ILogger<Program> logger) =>
{
  string serviceTwoEndpoint = Environment.GetEnvironmentVariable("SERVICE_ONE_ENDPOINT");
  client.GetAsync(serviceTwoEndpoint + "/two");
});

app.Run();