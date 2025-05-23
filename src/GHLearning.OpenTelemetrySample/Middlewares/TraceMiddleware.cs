﻿using System.Diagnostics;

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
			context.Response.Headers[TraceHeaders.TraceParent] = traceParent;
			context.Response.Headers[TraceHeaders.TraceId] = traceId;
			context.Response.Headers[TraceHeaders.ParentId] = spanId;
			context.Response.Headers[TraceHeaders.TraceFlag] = traceFlags;
			return Task.CompletedTask;
		});

		return next(context);
	}
}
