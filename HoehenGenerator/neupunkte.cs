﻿using System.Windows.Media;

namespace HoehenGenerator
{
    class NeuPunkte
    {
        PointCollection punkte = new PointCollection();
        double fläche;

        public PointCollection Punkte { get => punkte; }
        public double Fläche { get => fläche; }


        public NeuPunkte(PointCollection points, double fläche)
        {
            this.punkte = points;
            this.fläche = fläche;

        }
    }
}
