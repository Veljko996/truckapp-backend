namespace WebApplication1.Utils.Exceptions;

public class NotFoundException:AppException
{
    public NotFoundException(string subKey, object? value = null)
    : base(subKey, "Nije pronađeno.", value)
    { }
}
