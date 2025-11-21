namespace WebApplication1.Utils.Exceptions;

public class ConflictException :AppException
{
    public ConflictException(string subKey, string message)
    : base(subKey, message)
    { }
}
