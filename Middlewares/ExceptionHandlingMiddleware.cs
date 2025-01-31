using System.Net;
using System.Text.Json;
using TransactionAPI.Exceptions;
using TransactionAPI.Models;
namespace TransactionAPI.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode status;
            string message;

            var exceptionType = exception.GetType();

            if (exceptionType == typeof(TransactionNotFoundException))
            {
                message = exception.Message;
                status = HttpStatusCode.NotFound;
            }
            else
            {
                message = exception.Message;
                status = HttpStatusCode.InternalServerError;
            }

            var errorResponse = new ErrorResponse
            {
                Status = status,
                Message = message
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            var exceptionResult = JsonSerializer.Serialize(errorResponse);

            return context.Response.WriteAsync(exceptionResult);
        }
    }
}
