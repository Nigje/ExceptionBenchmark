using ExceptionBenchmark.Common;
using Newtonsoft.Json;
using System.Net;


namespace ExceptionBenchmark.WebApi.Models
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception exception)
            {
                await HandleGlobalExceptionAsync(httpContext, exception);
            }
        }

        /// <summary>
        ///     Handle exception.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private async Task HandleGlobalExceptionAsync(HttpContext context, Exception exception)
        {

            //An unique id to track exceptions in system.
            context.Response.StatusCode = (int)GetHttpStatusCode(exception);
            string errorCode = (((exception as CustomException)?.ErrorCode) ?? ErrorCodeEnum.INTERNAL_SERVER_ERROR).ToString();
            ErrorInfo result = new ErrorInfo
            {
                Details = GetDetails(exception),
                ErrorCode = errorCode,
                Message = exception.Message,
                TraceId = Guid.NewGuid().ToString()
            };
            var json = JsonConvert.SerializeObject(result);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(json);
        }

        /// <summary>
        /// Get HttpStatusCode by using exception type.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private HttpStatusCode GetHttpStatusCode(Exception exception)
        {
            if (exception is CustomException)
                return HttpStatusCode.BadRequest;
            return HttpStatusCode.InternalServerError;
        }
        private string GetDetails(Exception exception)
        {
            if (exception is CustomException)
            {
                var customException = exception as CustomException;
                if (customException.LogStackTrace)
                {
                    var stackTrace = customException.StackTrace;
                }
            }
            return "Details according to the environment.";
        }
    }
}
