namespace HoehenGenerator
{
    class Filemitauflösung
    { // TODO: Umkehr Auflösung 3 -> 1 1 -> 3
        string dateiname;
        int auflösung;

        public Filemitauflösung(string dateiname, int auflösung)
        {
            this.dateiname = dateiname;
            this.auflösung = auflösung;
        }

        public string Dateiname { get => dateiname; set => dateiname = value; }
        public int Auflösung { get => auflösung; set => auflösung = value; }
    }
}
