using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Serilog;

namespace Eventify.Middleware //bulunduğumuz dizin
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next; //sıradaki middleware bileşenine geçmek için kullanılır

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext) //middleware bileşenleri yazılırken InvokeAsync kullanılır
        {
            try
            {
                await _next(httpContext); //sıradaki middleware'a geç
            }
            catch (Exception ex)
            {
                Log.Error($"Something went wrong: {ex.Message}"); //hata oluşursa logla
                httpContext.Response.StatusCode = 500; //http yanıtına 500 durum kodu ver
                await httpContext.Response.WriteAsync("An unexpected error occurred.");
            }
        }
    }
}
