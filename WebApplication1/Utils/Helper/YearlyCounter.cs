namespace WebApplication1.Utils.Helper;

public static class YearlyCounters
{
    private static int _turaCounter = 1;   // jer već imaš 1
    private static int _nalogCounter = 0;  // da sutra krene od 1
    private static int _year;

    private static void EnsureYear()
    {
        var currentYear = DateTime.UtcNow.Year;

        if (_year != currentYear)
        {
            _year = currentYear;

            _turaCounter = 1;   
            _nalogCounter = 0;
        }
    }

    public static int NextTura()
    {
        EnsureYear();
        _turaCounter++;
        return _turaCounter;
    }

    public static int NextNalog()
    {
        EnsureYear();
        _nalogCounter++;
        return _nalogCounter;
    }
}

