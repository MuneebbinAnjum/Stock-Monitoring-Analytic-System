using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;

namespace SMAS.API.Middleware
{
    /// <summary>
    /// In-memory sliding window rate limiter per IP address.
    /// No Redis dependency required.
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private const int DefaultRateLimit = 120;  // Requests per window
        private const int WindowMinutes = 1;  // Time window in minutes
        private readonly int _limit;
        private readonly TimeSpan _window;
        private static readonly ConcurrentDictionary<string, RateLimitEntry> _clients = new();

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
            _limit = DefaultRateLimit;
            _window = TimeSpan.FromMinutes(WindowMinutes);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var now = DateTime.UtcNow;

            var entry = _clients.GetOrAdd(ip, _ => new RateLimitEntry());

            bool isRateLimited = false;
            lock (entry)
            {
                // Remove expired timestamps
                while (entry.Timestamps.Count > 0 && now - entry.Timestamps.Peek() > _window)
                {
                    entry.Timestamps.Dequeue();
                }

                if (entry.Timestamps.Count >= _limit)
                {
                    isRateLimited = true;
                }
                else
                {
                    entry.Timestamps.Enqueue(now);
                }
            }

            if (isRateLimited)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = WindowMinutes * 60 + "";
                await context.Response.WriteAsync("Too many requests. Please slow down.");
                return;
            }

            await _next(context);
        }

        private class RateLimitEntry
        {
            public Queue<DateTime> Timestamps { get; } = new();
        }
    }
}
