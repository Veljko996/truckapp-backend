namespace WebApplication1.Utils.Enums;

public static class OpcijaPrevoza
{
    public const string Solo = "Solo";
    public const string Kombinovano = "Kombinovano";
    public const string Parcijalno = "Parcijalno";
    public const string FTL = "FTL"; // Full Truck Load
    public const string LTL = "LTL"; // Less Than Truck Load

    public static readonly string[] All =
    {
        Solo, Kombinovano, Parcijalno, FTL, LTL
    };

    public static bool IsValid(string value)
    {
        return All.Contains(value, StringComparer.OrdinalIgnoreCase);
    }
}

