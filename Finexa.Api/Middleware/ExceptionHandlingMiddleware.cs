using System.Net;
using System.Text.Json;

namespace Finexa.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex, ILogger logger)
        {
            logger.LogError(ex, ex.Message);

            int statusCode = 500;
            string message = "An unexpected error occurred.";
            string title = "Server Error";

            switch (ex)
            {
                case ArgumentException:
                    statusCode = 400;
                    title = "Bad Request";
                    message = ex.Message;
                    break;

                case UnauthorizedAccessException:
                    statusCode = 401;
                    title = "Unauthorized";
                    message = ex.Message;
                    break;

                case KeyNotFoundException:
                    statusCode = 404;
                    title = "Not Found";
                    message = ex.Message;
                    break;

                case InvalidOperationException:
                    statusCode = 409;
                    title = "Conflict";
                    message = ex.Message;
                    break;

                default:
                    statusCode = 500;
                    title = "Server Error";
                    message = string.IsNullOrWhiteSpace(ex.Message)
                        ? "Something went wrong."
                        : ex.Message;
                    break;
            }

            var response = new
            {
                success = false,
                statusCode,
                title,
                message
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
