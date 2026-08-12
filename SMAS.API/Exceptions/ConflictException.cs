namespace SMAS.API.Exceptions
{
    /// <summary>
    /// Represents a conflict error (HTTP 409) that occurs when attempting to create or modify a resource
    /// that violates a unique constraint or business rule.
    /// Examples: duplicate SKU, duplicate email, etc.
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }

        public ConflictException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
