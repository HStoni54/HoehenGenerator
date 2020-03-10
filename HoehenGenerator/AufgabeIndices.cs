namespace HoehenGenerator
{
    internal class AufgabeIndices
    {
        public AufgabeIndices(string hgtart, int auflösung, string pfad)
        {
            Hgtart = hgtart;
            Auflösung = auflösung;
            Pfad = pfad;
        }

        public string Hgtart { get; set; }
        public int Auflösung { get; set; }
        public string Pfad { get; set; }
    }
}
