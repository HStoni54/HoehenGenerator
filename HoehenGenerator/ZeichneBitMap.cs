using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace HoehenGenerator
{
    class ZeichneBitMap
    {
        Bitmap bitmap;
        int Höhe, Breite;
        Color[,] color;
        Color color1;

        public Bitmap Bitmap { get => bitmap; set => bitmap = value; }
        public int Höhe1 { get => Höhe; set => Höhe = value; }
        public int Breite1 { get => Breite; set => Breite = value; }
        public Color[,] Color { get => color; set => color = value; }
        public Color Color1 { get => color1; set => color1 = value; }

        public ZeichneBitMap(Bitmap bitmap, int höhe, int breite, Color color1)
        {
            this.bitmap = bitmap;
            Höhe = höhe;
            Breite = breite;
            this.color1 = color1;
            this.color = colors(höhe,breite);
            

        }

        public ZeichneBitMap(Bitmap bitmap, int höhe, int breite, Color[,] color)
        {
            this.bitmap = bitmap;
            Höhe = höhe;
            Breite = breite;
            this.color = color;
            
        }

       

        public ZeichneBitMap(Bitmap bitmap)
        {
            Bitmap = bitmap;
            
        }

        private Color[,] colors(int höhe, int breite)
        {
            Color[,] color = new Color[höhe, breite];
            return color;
            
        }

        public Bitmap FülleBitmap()
        {
            for (int i = 0; i < Höhe; i++)
            {
                for (int j = 0; j < Breite; j++)
                {
                    bitmap.SetPixel(i, j, color[i, j]);
                }
            }
            return bitmap;
        }

    }
}
