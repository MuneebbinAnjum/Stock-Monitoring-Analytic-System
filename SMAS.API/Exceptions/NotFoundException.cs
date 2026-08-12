namespace SMAS.API.Exceptions
{
    /// <summary>
    /// Represents a not found error (HTTP 404) that occurs when a requested resource does not exist.
    /// Examples: product not found, order not found, user not found, etc.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
