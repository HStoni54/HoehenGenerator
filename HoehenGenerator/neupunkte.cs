using System.Windows.Media;

namespace HoehenGenerator
{
    internal class NeuPunkte
    {
        private readonly PointCollection punkte = new PointCollection();
        private readonly double fläche;

        public PointCollection Punkte { get => punkte; }
        public double Fläche { get => fläche; }


        public NeuPunkte(PointCollection points, double fläche)
        {
            this.punkte = points;
            this.fläche = fläche;

        }
    }
}
