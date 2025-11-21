namespace WebApplication1.Utils.Exceptions;

public class ValidationException : AppException
{
    public ValidationException(string subKey, string message)
    : base(subKey, message)
    { }
}
