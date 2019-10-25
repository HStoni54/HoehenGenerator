namespace HoehenGenerator
{
    internal class ClZeichneMatrix
    {
        private string macheEs;

        public ClZeichneMatrix(string macheEs)
        {
            this.macheEs = macheEs;
        }

        public string MacheEs { get => macheEs; set => macheEs = value; }
    }
}
