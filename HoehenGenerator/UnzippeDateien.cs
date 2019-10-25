namespace HoehenGenerator
{
    internal class UnzippeDateien
    {
        private string zieldatei;

        public UnzippeDateien(string zieldatei)
        {
            this.zieldatei = zieldatei;
        }

        public string Zieldatei { get => zieldatei; set => zieldatei = value; }
    }
}