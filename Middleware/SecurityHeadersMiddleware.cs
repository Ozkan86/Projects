using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;


namespace Eventify.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // X-Content-Type-Options
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

            // X-Frame-Options
            context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN"); // Veya "DENY"

            // Content-Security-Policy (Örnek - uygulamanız için dikkatlice yapılandırın)
            // Kısıtlayıcı bir politikayla başlayın ve kapsamlı bir şekilde test edin.
            string csp = "default-src 'self'; " +
                         "script-src 'self' 'unsafe-inline'; " + // 'unsafe-inline' betikler için SwaggerUI tarafından gerekebilir, dikkatli olun
                         "style-src 'self' 'unsafe-inline'; " +  // 'unsafe-inline' stiller için SwaggerUI tarafından gerekebilir
                         "img-src 'self' data:; " + // data: Swagger logosu gibi satır içi resimler için
                         "font-src 'self'; " +
                         "object-src 'none'; " +
                         "frame-ancestors 'self'; " + // Daha ayrıntılı kontrol gerekirse X-Frame-Options'ın yerini alır
                         "form-action 'self'; " +
                         "base-uri 'self';";
            context.Response.Headers.Append("Content-Security-Policy", csp);

            // Referrer-Policy
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

            // Permissions-Policy (Örnek)
            context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");


            await _next(context);
        }
    }

    // Kolay kayıt için uzantı metodu (isteğe bağlı ancak iyi bir uygulama)
    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}