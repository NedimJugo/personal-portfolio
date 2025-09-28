using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text.Json;

namespace Portfolio.WebAPI.Middleware
{
    public class JwtErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtErrorHandlingMiddleware> _logger;

        public JwtErrorHandlingMiddleware(RequestDelegate next, ILogger<JwtErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("JWT token expired for request {Path}", context.Request.Path);
                await HandleTokenExpiredAsync(context);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "JWT token validation failed for request {Path}", context.Request.Path);
                await HandleUnauthorizedAsync(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in JWT middleware for request {Path}", context.Request.Path);
                await HandleInternalServerErrorAsync(context);
            }
        }

        private static async Task HandleTokenExpiredAsync(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "token_expired",
                message = "The JWT token has expired",
                timestamp = DateTime.UtcNow
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }

        private static async Task HandleUnauthorizedAsync(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "invalid_token",
                message = "The JWT token is invalid",
                timestamp = DateTime.UtcNow
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }

        private static async Task HandleInternalServerErrorAsync(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "internal_server_error",
                message = "An unexpected error occurred",
                timestamp = DateTime.UtcNow
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
