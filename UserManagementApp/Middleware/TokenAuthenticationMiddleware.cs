using System.Net;
using System.Text.Json;

namespace UserManagementApp.Middleware
{
    public class TokenAuthenticationMiddleware
    {
        private const string ExpectedScheme = "Bearer";
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenAuthenticationMiddleware> _logger;
        private readonly IConfiguration _configuration;

        // Paths that do not require authentication (Swagger UI/docs).
        private static readonly string[] AnonymousPathPrefixes =
        {
            "/swagger"
        };

        public TokenAuthenticationMiddleware(RequestDelegate next, ILogger<TokenAuthenticationMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            if (AnonymousPathPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            var expectedToken = _configuration["Authentication:ApiToken"];

            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
            {
                await WriteUnauthorizedAsync(context, "Missing Authorization header.");
                return;
            }

            var authHeader = authHeaderValues.ToString();
            if (!authHeader.StartsWith($"{ExpectedScheme} ", StringComparison.OrdinalIgnoreCase))
            {
                await WriteUnauthorizedAsync(context, "Authorization header must use the Bearer scheme.");
                return;
            }

            var token = authHeader[ExpectedScheme.Length..].Trim();

            if (string.IsNullOrEmpty(expectedToken) || !string.Equals(token, expectedToken, StringComparison.Ordinal))
            {
                _logger.LogWarning("Rejected request to {Path} due to invalid token.", path);
                await WriteUnauthorizedAsync(context, "Invalid or expired token.");
                return;
            }

            await _next(context);
        }

        private static async Task WriteUnauthorizedAsync(HttpContext context, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            var response = new
            {
                error = message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
