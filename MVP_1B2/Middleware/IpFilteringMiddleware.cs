using System.Net;
using Microsoft.Extensions.Options;

namespace MVP_1B2.Middleware
{
    public class IpFilteringOptions
    {
        public bool Enabled { get; set; } = true;

        public List<string> AllowedIPs { get; set; } = new();
    }

    public class IpFilteringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IpFilteringOptions _options;
        private readonly ILogger<IpFilteringMiddleware> _logger;

        public IpFilteringMiddleware(
            RequestDelegate next,
            IOptions<IpFilteringOptions> options,
            ILogger<IpFilteringMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // IP filtering disabled
            if (!_options.Enabled)
            {
                await _next(context);
                return;
            }

            var remoteIp =
                context.Connection.RemoteIpAddress;

            if (remoteIp == null)
            {
                _logger.LogWarning(
                    "Request rejected because client IP could not be determined.");

                context.Response.StatusCode = StatusCodes.Status403Forbidden;

                await context.Response.WriteAsync(
                    "Access denied: client IP address could not be determined.");

                return;
            }

            var remoteIpString = remoteIp.ToString();

            // Normalize IPv4-mapped IPv6 addresses
            if (remoteIp.IsIPv4MappedToIPv6)
            {
                remoteIpString =
                    remoteIp.MapToIPv4().ToString();
            }

            var allowed =
                _options.AllowedIPs.Any(
                    ip => string.Equals(
                        ip,
                        remoteIpString,
                        StringComparison.OrdinalIgnoreCase));

            _logger.LogInformation(
                "IP filtering: ClientIP={ClientIP}, Allowed={Allowed}",
                remoteIpString,
                allowed);

            if (!allowed)
            {
                context.Response.StatusCode =
                    StatusCodes.Status403Forbidden;

                context.Response.ContentType =
                    "text/plain";

                await context.Response.WriteAsync(
                    $"Access denied. IP address {remoteIpString} is not allowed.");

                return;
            }

            await _next(context);
        }
    }
}