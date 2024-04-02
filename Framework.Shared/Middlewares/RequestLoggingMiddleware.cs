using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Framework.Shared.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string requestId = context.TraceIdentifier;
            StringBuilder message = new($"Request started. | RequestId: {requestId} | HTTP Method: {context.Request.Method} | Path: {context.Request.Path} | ");

            if (context.Request.ContentLength > 0 && context.Request.Body.CanRead) //If has body
            {
                context.Request.EnableBuffering();

                var requestBodyStream = new MemoryStream();
                await context.Request.Body.CopyToAsync(requestBodyStream);
                requestBodyStream.Seek(0, SeekOrigin.Begin);

                using (var requestBodyReader = new StreamReader(requestBodyStream))
                {
                    var requestBody = await requestBodyReader.ReadToEndAsync();
                    message.Append($"Body: {requestBody} | ");
                }

                context.Request.Body.Seek(0, SeekOrigin.Begin);
            }

            if (context.Request.Query.Count != 0) //If has query string in path
                message.Append($"QueryString: {context.Request.QueryString}");

            _logger.LogInformation(message.ToString());

            try
            {
                await _next(context);

                _logger.LogInformation($"Request completed. | RequestId: {requestId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RequestId: {requestId}");
                _logger.LogInformation($"Request failed! | RequestId: {requestId}");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status200OK;

                var response = new //GeneralResponse
                {
                    RequestId = requestId,
                    IsSuccess = false,
                    Message = ex.Message
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
            }
        }
    }
}
