using System.Windows.Media;

namespace HoehenGenerator
{
    internal class NeuPunkte
    {
        public PointCollection Punkte { get; } = new PointCollection();
        public double Fläche { get; }


        public NeuPunkte(PointCollection points, double fläche)
        {
            Punkte = points;
            Fläche = fläche;

        }
    }
}
