using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Wolf.Notification.Exceptions;

namespace Wolf.Notification.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context /* other dependencies */)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled excpetion for {context?.TraceIdentifier}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var code = HttpStatusCode.InternalServerError; // 500 if unexpected

            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            if (ex is NotFoundException) code = HttpStatusCode.NotFound;
            else if (ex is Exception) code = HttpStatusCode.BadRequest;
            else if (ex is NullModelException) code = HttpStatusCode.BadRequest;
            else if (ex is IncorrectFileException) code = HttpStatusCode.BadRequest;
            else if (ex is IncorrectModelException) code = HttpStatusCode.BadRequest;

            var result = string.Empty;
            if (ex is BaseException)
            {
                var bEx = ex as BaseException;

                if (bEx.InsteadOfMessage != null)
                {
                    result = JsonConvert.SerializeObject(bEx.InsteadOfMessage);
                }
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                result = JsonConvert.SerializeObject(new { message = ex.Message, trace_id = context?.TraceIdentifier });
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }
    }
}