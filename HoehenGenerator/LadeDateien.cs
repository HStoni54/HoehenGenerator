namespace HoehenGenerator
{
    class LadeDateien
    {
        string url;
        string zieldatei;

        public LadeDateien(string url, string zieldatei)
        {
            this.url = url;
            this.zieldatei = zieldatei;
        }

        public string Url { get => url; set => url = value; }
        public string Zieldatei { get => zieldatei; set => zieldatei = value; }
    }
}
