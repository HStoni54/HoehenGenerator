namespace HoehenGenerator
{
    class Filemitauflösung
    { 
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
