namespace HoehenGenerator
{
    internal class UnzippeDateien
    {
        string zieldatei;

        public UnzippeDateien(string zieldatei)
        {
            this.zieldatei = zieldatei;
        }

        public string Zieldatei { get => zieldatei; set => zieldatei = value; }
    }
}