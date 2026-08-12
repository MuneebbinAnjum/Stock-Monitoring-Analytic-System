namespace SMAS.API.Exceptions
{
    /// <summary>
    /// Represents a service unavailable error (HTTP 503) that occurs when the service
    /// cannot process the request due to temporary issues like deadlocks or timeouts.
    /// </summary>
    public class ServiceUnavailableException : Exception
    {
        public ServiceUnavailableException(string message) : base(message) { }

        public ServiceUnavailableException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
