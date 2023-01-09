using System.Threading.Tasks;

namespace ExceptionBenchmark.Common
{
    public interface IExceptionService
    {
        Task<int> ReturnStatusCodeWithoutTryCatchAsync(int depthToThrowException);
        Task<int> ReturnStatusCodeWithTryCatchAsync(int depthToThrowException);
        Task ThrowExceptionWithoutStackTraceAsync(int depthToThrowException);
        Task ThrowExceptionWithStackTraceAsync(int depthToThrowException);
        int ReturnStatusCodeWithoutTryCatch(int depthToThrowException);
        int ReturnStatusCodeWithTryCatch(int depthToThrowException);
        void ThrowExceptionWithoutStackTrace(int depthToThrowException);
        void ThrowExceptionWithStackTrace(int depthToThrowException);
        Task ThrowExceptionWichoutTryCatchWithStackTraceAsync(int depthToThrowException);
        Task ThrowExceptionWichoutTryCatchWithoutStackTraceAsync(int depthToThrowException);
    }
}