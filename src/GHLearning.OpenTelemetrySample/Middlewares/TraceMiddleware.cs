using System.Diagnostics;
using Microsoft.Net.Http.Headers;

namespace GHLearning.OpenTelemetrySample.Middlewares;

public class TraceMiddleware(RequestDelegate next)
{
	public Task InvokeAsync(HttpContext context)
	{
		using var activity = Activity.Current;
		var traceParent = activity?.Id?.ToString() ?? string.Empty;
		var traceId = activity?.TraceId.ToString();
		var spanId = activity?.SpanId.ToString();
		var traceFlags = ((int)(activity?.ActivityTraceFlags ?? ActivityTraceFlags.None)).ToString("D2");

		context.Response.OnStarting(() =>
		{
			context.Response.Headers[HeaderNames.TraceParent] = traceParent;
			return Task.CompletedTask;
		});

		return next(context);
	}
}
