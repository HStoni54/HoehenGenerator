namespace HoehenGenerator
{
    class HgtmitKoordinaten
    {
        string name;
        int dezLat;
        int dezLon;

        public HgtmitKoordinaten(string name, int dezLon, int dezLat)
        {
            this.name = name;
            this.dezLon = dezLon;
            this.dezLat = dezLat;
        }

        //public HgtmitKoordinaten(string name, int dezlon, int dezlat)
        //{
        //    this.name = name;
        //    this.dezLat = dezlat;
        //    this.dezLon = dezlon;
        //}

        public string Name { get => name; set => name = value; }
        public int DezLat { get => dezLat; set => dezLat = value; }
        public int DezLon { get => dezLon; set => dezLon = value; }
    }
}
