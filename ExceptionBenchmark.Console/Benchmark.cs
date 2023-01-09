
using BenchmarkDotNet.Attributes;
using ExceptionBenchmark.Common;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace ExceptionBenchmark.Console
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        //[Params(1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024)]
        [Params(1)]

        public int depthToThrowException;
        public string baseUrl = "https://localhost:44365/";
        private static HttpClient httpClient = new HttpClient();

        private readonly Common.IExceptionService exceptionBenchmark = new Common.ExceptionService();
        [Benchmark]
        public async Task Async_ReturnStatusCode_WithoutTryCatch()
        {
            await exceptionBenchmark.ReturnStatusCodeWithoutTryCatchAsync(depthToThrowException);
        }
        [Benchmark]
        public async Task Async_ReturnStatusCode_WithTryCatch()
        {
            await exceptionBenchmark.ReturnStatusCodeWithTryCatchAsync(depthToThrowException);
        }
        [Benchmark]
        public async Task Async_Throw_WithoutStackTrace()
        {
            await exceptionBenchmark.ThrowExceptionWithoutStackTraceAsync(depthToThrowException);
        }

        [Benchmark]
        public async Task Async_Throw_WithStackTrace()
        {
            await exceptionBenchmark.ThrowExceptionWithStackTraceAsync(depthToThrowException);
        }
        [Benchmark]
        public void ReturnStatusCode_WithoutTryCatch()
        {
            exceptionBenchmark.ReturnStatusCodeWithoutTryCatch(depthToThrowException);
        }
        [Benchmark]
        public void ReturnStatusCode_WithTryCatch()
        {
            exceptionBenchmark.ReturnStatusCodeWithTryCatch(depthToThrowException);
        }
        [Benchmark]
        public void Throw_WithoutStackTrace()
        {
            exceptionBenchmark.ThrowExceptionWithoutStackTrace(depthToThrowException);
        }
        [Benchmark]
        public void Throw_WithStackTrace()
        {
            exceptionBenchmark.ThrowExceptionWithStackTrace(depthToThrowException);
        }
        [Benchmark]
        public async Task API_Get_WhithoutErrorAsync()
        {
            var result = await httpClient.GetAsync($"{baseUrl}/async-ok");
            var resultString = await result.Content.ReadAsStringAsync();
        }
        [Benchmark]
        public async Task API_ReturnStatusCodeAsync()
        {
            var result = await httpClient.GetAsync($"{baseUrl}/async-status-code/{depthToThrowException}");
            var resultString = await result.Content.ReadAsStringAsync();
        }
        [Benchmark]
        public async Task API_ThrowException_WithoutStackTraceAsync()
        {
            var result = await httpClient.GetAsync($"{baseUrl}/async-throw-exception-no-stack-trace/{depthToThrowException}");
            var resultString = await result.Content.ReadAsStringAsync();
        }
        [Benchmark]
        public async Task API_ThrowException_WithStackTraceAsync()
        {
            var result = await httpClient.GetAsync($"{baseUrl}/async-throw-exception-by-stack-trace/{depthToThrowException}");
            var resultString = await result.Content.ReadAsStringAsync();

        }
    }
}
