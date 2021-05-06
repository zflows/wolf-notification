using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Wolf.Notification.Config;

namespace Wolf.Notification.Middlewares
{
    /// <summary>
    /// Adopted from https://exceptionnotfound.net/using-middleware-to-log-requests-and-responses-in-asp-net-core/
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly RequestResponseLoggerOptions _settings;

        public RequestLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IOptions<RequestResponseLoggerOptions> options)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<RequestLoggingMiddleware>();
            _settings = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            string requestBody= _settings.LogRequest? await FormatRequest(context.Request) : string.Empty;

            Stream originalBodyWriteStream=null;
            MemoryStream newBodyStream=null;

            try
            {
                originalBodyWriteStream = context.Response.Body;
                if (_settings.LogResponse)
                {
                    newBodyStream = new MemoryStream();
                    context.Response.Body = newBodyStream;
                }
                await _next(context);
            }
            finally
            {
                HttpRequest req = context.Request;
                string traceId = context.TraceIdentifier;
                _logger.LogInformation("Request {traceId}:{method} {url} => {statusCode}\r\n{request_body}",
                    traceId,
                    req?.Method,
                    req?.Path.Value,
                    context.Response?.StatusCode,
                    requestBody
                );
				if (_settings.LogResponse)
				{                    
                    string response = await GetResponse(context.Response);

                    _logger.LogInformation("Response {traceId}:\r\n{response_body}", traceId, response);

                    if (null != newBodyStream)
                    {
                        await newBodyStream.CopyToAsync(originalBodyWriteStream);
                        await newBodyStream.DisposeAsync();
                    }
                }
            }
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            if (null == request || null==request.ContentLength) return null;
            Stream bodyStream = request.Body;

            //This line allows us to set the reader for the request back at the beginning of its stream.
            request.EnableBuffering();

            //We now need to read the request stream.  First, we create a new byte[] with the same length as the request stream...
            var buffer = new byte[(int)request.ContentLength];

            //...Then we copy the entire request stream into the new buffer.
            await request.Body.ReadAsync(buffer, 0, buffer.Length);

            //We convert the byte[] into a string using UTF8 encoding...
            string bodyAsText = Encoding.UTF8.GetString(buffer);

            //..and finally, assign the read body back to the request body, which is allowed because of EnableRewind()
            request.Body.Position = 0;

            return bodyAsText;
        }

        private async Task<string> GetResponse(HttpResponse response)
        {
            string textOut;

            response.Body.Position = 0;
            using (var streamReader = new StreamReader(response.Body, leaveOpen:true)) {
                if (_settings.MaxResponseLength <= 0)
                {
                    textOut = await streamReader.ReadToEndAsync();
                }
                else
                {
                    char[] buffer = new char[_settings.MaxResponseLength];
                    int bytesRead= await streamReader.ReadAsync(buffer, 0, buffer.Length);
                    textOut = new string(buffer);
					if (bytesRead < _settings.MaxResponseLength)
					{
                        textOut += " ...";
					}
                }
            }

            response.Body.Position = 0;
            return textOut;
        }
    }
}