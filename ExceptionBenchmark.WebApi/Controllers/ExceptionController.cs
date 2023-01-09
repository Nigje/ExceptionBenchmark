using ExceptionBenchmark.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExceptionBenchmark.WebApi.Controllers
{
    [Route("api/exception")]
    [ApiController]
    public class ExceptionController : ControllerBase
    {
        private readonly IExceptionService exceptionService;
        public ExceptionController(IExceptionService exceptionService)
        {
            this.exceptionService = exceptionService;
        }
        [HttpGet]
        [Route("async-ok")]
        public async Task<IActionResult> Get()
        {
            return Ok();
        }
        [HttpGet]
        [Route("async-status-code/{depth}")]
        public async Task<IActionResult> ReturnStatusCodeWithoutTryCatch([FromRoute] int depth)
        {
            var statusCode = await exceptionService.ReturnStatusCodeWithoutTryCatchAsync(depth);
            if (statusCode > 0)
                return Ok();
            return BadRequest("Error in deep");
        }
        [HttpGet]
        [Route("async-throw-exception-no-stack-trace/{depth}")]
        public async Task ThrowExceptionWichoutTryCatchWithoutStackTrace([FromRoute] int depth)
        {
            await exceptionService.ThrowExceptionWichoutTryCatchWithoutStackTraceAsync(depth);

        }
        [HttpGet]
        [Route("async-throw-exception-by-stack-trace/{depth}")]
        public async Task ThrowExceptionWichoutTryCatchWithStackTrace([FromRoute] int depth)
        {
            await exceptionService.ThrowExceptionWichoutTryCatchWithStackTraceAsync(depth);

        }
    }
}
