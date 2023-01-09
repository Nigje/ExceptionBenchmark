using System;

namespace ExceptionBenchmark.Common
{
    public class IgnoreStackTraceException : CustomException
    {
        public IgnoreStackTraceException(string message, string? technicalMessage = null, ErrorCodeEnum? errorCode = null) : base(message, technicalMessage, errorCode, false)
        {
        }

        public IgnoreStackTraceException(string message, string technicalMessage, Exception innerException, ErrorCodeEnum? errorCode = null) : base(message, technicalMessage, innerException, errorCode, false)
        {
        }
    }
}
