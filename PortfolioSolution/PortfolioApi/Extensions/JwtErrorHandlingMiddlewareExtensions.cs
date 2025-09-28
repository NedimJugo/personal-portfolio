using Portfolio.WebAPI.Middleware;

namespace Portfolio.WebAPI.Extensions
{
    public static class JwtErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtErrorHandlingMiddleware>();
        }
    }
}
