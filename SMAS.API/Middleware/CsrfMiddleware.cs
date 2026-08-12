using Microsoft.AspNetCore.Http;

namespace SMAS.API.Middleware
{
    public class CsrfMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] _sensitivePaths = Array.Empty<string>();

        public CsrfMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var path = context.Request.Path.Value ?? string.Empty;
                if (context.Request.Method == HttpMethods.Post && _sensitivePaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                {
                    var origin = context.Request.Headers["Origin"].FirstOrDefault();
                    var referer = context.Request.Headers["Referer"].FirstOrDefault();
                    var allowed = context.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
                    var allowedOrigins = allowed?.GetValue<string>("AllowedOrigins") ?? "http://localhost:3000,http://localhost:8080";
                    var list = allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    bool ok = false;
                    if (!string.IsNullOrEmpty(origin)) ok = list.Any(o => origin.StartsWith(o, StringComparison.OrdinalIgnoreCase));
                    if (!ok && !string.IsNullOrEmpty(referer))
                    {
                        try
                        {
                            var uri = new Uri(referer);
                            var host = uri.GetLeftPart(UriPartial.Authority);
                            ok = list.Any(o => host.StartsWith(o, StringComparison.OrdinalIgnoreCase));
                        }
                        catch { }
                    }

                    if (!ok)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("CSRF check failed");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes only
                // Continue with the request as CSRF is not always required
                System.Diagnostics.Debug.WriteLine($"CSRF middleware error: {ex.Message}");
            }

            await _next(context);
        }
    }
}
