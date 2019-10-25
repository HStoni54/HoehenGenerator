namespace HoehenGenerator
{
    internal class clZeichneMatrix
    {
        private string macheEs;

        public clZeichneMatrix(string macheEs)
        {
            this.macheEs = macheEs;
        }

        public string MacheEs { get => macheEs; set => macheEs = value; }
    }
}
