namespace ExceptionBenchmark.WebApi.Models
{
    /// <summary>
    /// Api error result details.
    /// </summary>
    public class ErrorInfo
    {
        /// <summary>
        /// Error code.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Error details.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        ///     An unique id to track exceptions in system.
        /// </summary>
        public string TraceId { get; set; }
    }
}
