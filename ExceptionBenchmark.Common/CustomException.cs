using System;
using System.Diagnostics.SymbolStore;

namespace ExceptionBenchmark.Common
{
    public class CustomException : Exception
    {
        /// <summary>
        ///     Error code that indicates a summary of error by using some words or numbers.
        ///     Ex: its value can be USER_NOT_FOUND when the user is not found in the applicaiton.
        /// </summary>
        public ErrorCodeEnum? ErrorCode { get; protected set; }

        /// <summary>
        ///     Technical-details are not allowed to be shown to the user.
        ///     Just log them or use them internally by software-technicians.
        /// </summary>
        public string? TechnicalMessage { get; protected set; }

        /// <summary>
        ///     Severity of the exception. The main usage will be for distinguish logs and monitoring.
        ///     Think about the difference of between severity of a ValidationException and an Exception related to DB connection or Infrastructure. 
        ///     Default: Error.
        /// </summary>
        public LogSeverityEnum Severity { get; protected set; }

        /// <summary>
        ///     A temporal variable that shows whether StackTrace is valuable or not.
        /// </summary>
        public bool LogStackTrace { get; protected set; }

        public CustomException(string message, string? technicalMessage = null, ErrorCodeEnum? errorCode = null, bool logStackTrace = true)
            : base(message)
        {
            TechnicalMessage = technicalMessage;
            Severity = LogSeverityEnum.Error;
            ErrorCode = errorCode;
            LogStackTrace = logStackTrace;
        }
        public CustomException(string message, string technicalMessage, Exception innerException, ErrorCodeEnum? errorCode = null, bool logStackTrace = true)
            : base(message, innerException)
        {
            TechnicalMessage = technicalMessage;
            Severity = LogSeverityEnum.Error;
            ErrorCode = errorCode;
            LogStackTrace = logStackTrace;
        }
    }
}
