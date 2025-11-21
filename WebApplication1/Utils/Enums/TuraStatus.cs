namespace WebApplication1.Utils.Enums
{
    public static class TuraStatus
    {
        public const string UPripremi = "U pripremi";
        public const string UtovarUToku = "Utovar u toku";
        public const string UtovarZavrsen = "Utovar završen";
        public const string NaPutu = "Na putu";
        public const string Carina = "Carina";
        public const string CarinaZavrsena = "Carina završena";
        public const string IstovarUToku = "Istovar u toku";
        public const string IstovarZavrsen = "Istovar završen";
        public const string Zakasnjenje = "Zakašnjenje";
        public const string Zavrseno = "Završeno";
        public const string Otkazano = "Otkazano";

        public static readonly string[] All =
        {
        UPripremi, UtovarUToku, UtovarZavrsen, NaPutu, Carina,
        CarinaZavrsena, IstovarUToku, IstovarZavrsen,
        Zakasnjenje, Zavrseno, Otkazano
    };
    }

}
