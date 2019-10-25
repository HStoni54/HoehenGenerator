namespace HoehenGenerator
{
    internal class LadeDateien
    {
        private string url;
        private string zieldatei;

        public LadeDateien(string url, string zieldatei)
        {
            this.url = url;
            this.zieldatei = zieldatei;
        }

        public string Url { get => url; set => url = value; }
        public string Zieldatei { get => zieldatei; set => zieldatei = value; }
    }
}
