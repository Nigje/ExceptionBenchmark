using System;
using System.Threading.Tasks;

namespace ExceptionBenchmark.Common
{
    public class ExceptionService : IExceptionService
    {
        public async Task ThrowExceptionWithStackTraceAsync(int depthToThrowException = 1)
        {
            try
            {
                await ThrowExceptionInDeepMethodAsync(depthToThrowException);
            }
            catch (CustomException ex)
            {
                var stackTrace = ex.StackTrace;
            }
        }
        public async Task ThrowExceptionWithoutStackTraceAsync(int depthToThrowException = 1)
        {
            try
            {
                await ThrowExceptionInDeepMethodAsync(depthToThrowException);
            }
            catch (CustomException ex)
            {
                var stackTrace = ex.Message;
            }
        }
        public async Task<int> ReturnStatusCodeWithTryCatchAsync(int depthToThrowException = 1)
        {
            try
            {
                int statusCode = await ReturnStatusCodeAsync(depthToThrowException);
                return statusCode;
            }
            catch (CustomException ex)
            {
                return -1;
            }
            finally
            {
            }
        }
        public async Task<int> ReturnStatusCodeWithoutTryCatchAsync(int depthToThrowException = 1)
        {
            int statusCode = await ReturnStatusCodeAsync(depthToThrowException);
            return statusCode;
        }
        public void ThrowExceptionWithStackTrace(int depthToThrowException = 1)
        {
            try
            {
                ThrowExceptionInDeepMethod(depthToThrowException);
            }
            catch (CustomException ex)
            {
                var stackTrace = ex.StackTrace;
            }
        }
        public void ThrowExceptionWithoutStackTrace(int depthToThrowException = 1)
        {
            try
            {
                ThrowExceptionInDeepMethod(depthToThrowException);
            }
            catch (CustomException ex)
            {
                var stackTrace = ex.Message;
            }
        }
        public int ReturnStatusCodeWithTryCatch(int depthToThrowException = 1)
        {
            try
            {
                int statusCode = ReturnStatusCode(depthToThrowException);
                return statusCode;
            }
            catch (CustomException ex)
            {
                return -1;
            }
            finally
            {
            }
        }
        public int ReturnStatusCodeWithoutTryCatch(int depthToThrowException = 1)
        {
            int statusCode = ReturnStatusCode(depthToThrowException);
            return statusCode;
        }
        private async Task ThrowExceptionInDeepMethodAsync(int depthToThrowException, int depth = 0)
        {
            if (depth == depthToThrowException)
            {
                throw new CustomException("Deep exception", $"Depth: {depth}");
            }
            else
            {
                await ThrowExceptionInDeepMethodAsync(depthToThrowException, depth + 1);
            }
        }
        private async Task<int> ReturnStatusCodeAsync(int depthToThrowException, int depth = 0)
        {
            if (depth == depthToThrowException)
            {
                return -1;
            }
            else
            {
                return await ReturnStatusCodeAsync(depthToThrowException, depth + 1);
            }
        }
        private void ThrowExceptionInDeepMethod(int depthToThrowException, int depth = 0)
        {
            if (depth == depthToThrowException)
            {
                throw new CustomException("Deep exception", $"Depth: {depth}");
            }
            else
            {
                ThrowExceptionInDeepMethod(depthToThrowException, depth + 1);
            }
        }
        private int ReturnStatusCode(int depthToThrowException, int depth = 0)
        {
            if (depth == depthToThrowException)
            {
                return -1;
            }
            else
            {
                return ReturnStatusCode(depthToThrowException, depth + 1);
            }
        }

        public async Task ThrowExceptionWichoutTryCatchWithStackTraceAsync(int depthToThrowException)
        {
            await ThrowExceptionInDeepMethodAsync(depthToThrowException);
        }

        public async Task ThrowExceptionWichoutTryCatchWithoutStackTraceAsync(int depthToThrowException)
        {
            await ThrowExceptionInDeepMethodWithoutStackTraceAsync(depthToThrowException);
        }
        private async Task ThrowExceptionInDeepMethodWithoutStackTraceAsync(int depthToThrowException=1,int depth=0)
        {
            if (depth == depthToThrowException)
            {
                throw new IgnoreStackTraceException("Deep exception", $"Depth: {depth}");
            }
            else
            {
                await ThrowExceptionInDeepMethodWithoutStackTraceAsync(depthToThrowException, depth + 1);
            }
        }
    }
}
