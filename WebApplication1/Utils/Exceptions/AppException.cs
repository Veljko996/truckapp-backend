namespace WebApplication1.Utils.Exceptions;

public class AppException : Exception
{
    public string SubKey { get; }
    public object? Value { get; }

    protected AppException(string subKey, string message, object? value = null)
        : base(message)
    {
        SubKey = subKey;
        Value = value;
    }
}
