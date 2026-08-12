using Microsoft.AspNetCore.Http;
using Serilog;
using System.Diagnostics;

namespace SMAS.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            await _next(context);
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var method = context.Request.Method;
            var path = context.Request.Path;
            var duration = stopwatch.ElapsedMilliseconds;

            Log.Information("{Method} {Path} responded {StatusCode} in {Duration}ms", method, path, statusCode, duration);
        }
    }
}