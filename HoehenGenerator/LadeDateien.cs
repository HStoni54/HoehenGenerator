namespace HoehenGenerator
{
    internal class LadeDateien
    {
        public LadeDateien(string url, string zieldatei)
        {
            Url = url;
            Zieldatei = zieldatei;
        }

        public string Url { get; set; }
        public string Zieldatei { get; set; }
    }
}
