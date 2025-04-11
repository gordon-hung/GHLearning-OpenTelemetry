using System.Net.Mime;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
	.AddRouting(options => options.LowercaseUrls = true)
	.AddControllers(options =>
	{
		options.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json));
		options.Filters.Add(new ConsumesAttribute(MediaTypeNames.Application.Json));
	})
	.AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Learn more about configuring HttpLogging at https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-logging/?view=aspnetcore-8.0
builder.Services.AddHttpLogging(logging =>
{
	logging.LoggingFields = HttpLoggingFields.All;
	logging.RequestBodyLogLimit = 4096;
	logging.ResponseBodyLogLimit = 4096;
	logging.CombineLogs = false;
});

//Learn more about configuring OpenTelemetry at https://learn.microsoft.com/zh-tw/dotnet/core/diagnostics/observability-with-otel
builder.Services.AddOpenTelemetry()
	.ConfigureResource(resource => resource
	.AddService(builder.Configuration["ServiceName"]!))
	.UseOtlpExporter(OtlpExportProtocol.Grpc, new Uri(builder.Configuration["OtlpEndpointUrl"]!))
	.WithMetrics(metrics => metrics
		.AddMeter("GHLearning.")
		.AddAspNetCoreInstrumentation()
		.AddRuntimeInstrumentation()
		.AddProcessInstrumentation()
		.AddPrometheusExporter())
	.WithTracing(tracing => tracing
		.AddEntityFrameworkCoreInstrumentation()
		.AddHttpClientInstrumentation()
		.AddAspNetCoreInstrumentation(options => options.Filter = (httpContext) => !httpContext.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/live", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/healthz", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/metrics", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/favicon.ico", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.Value!.Equals("/api/events/raw", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.Value!.EndsWith(".js", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/_vs", StringComparison.OrdinalIgnoreCase)));

//Learn more about configuring HealthChecks at https://learn.microsoft.com/zh-tw/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-9.0
builder.Services.AddHealthChecks()
	.AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseHttpLogging();

app.UseAuthorization();

app.MapControllers();

app.UseHealthChecks("/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
	Predicate = check => check.Tags.Contains("live"),
	ResultStatusCodes =
	{
		[HealthStatus.Healthy] = StatusCodes.Status200OK,
		[HealthStatus.Degraded] = StatusCodes.Status200OK,
		[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
	}
});
app.UseHealthChecks("/healthz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
	Predicate = _ => true,
	ResultStatusCodes =
	{
		[HealthStatus.Healthy] = StatusCodes.Status200OK,
		[HealthStatus.Degraded] = StatusCodes.Status200OK,
		[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
	}
});

// Prometheus 提供服務數據資料源
app.MapMetrics();
app.UseHttpMetrics();

app.Run();
