
using System.Drawing;

namespace HoehenGenerator
{
    internal class ZeichneBitMap
    {
        private readonly Bitmap bitmap;
        private readonly int Höhe, Breite;
        private readonly Color[,] color;
        private readonly Color color1;

        public Bitmap Bitmap { get => bitmap; }
        public int Höhe1 { get => Höhe; }
        public int Breite1 { get => Breite; }
        public Color[,] Color { get => color; }
        public Color Color1 { get => color1; }

        public ZeichneBitMap(Bitmap bitmap, Color color1)
        {
            this.bitmap = bitmap;
            Höhe = bitmap.Height;
            Breite = bitmap.Width;
            this.color1 = color1;
            this.color = new Color[bitmap.Height, bitmap.Width];
            for (int i = 0; i < bitmap.Width; i++)
                for (int j = 0; j < bitmap.Height; j++)
                {
                    color[j, i] = color1;
                }
            {

            }


        }

        public ZeichneBitMap(Bitmap bitmap, Color[,] color)
        {
            this.bitmap = bitmap;
            Höhe = bitmap.Height;
            Breite = bitmap.Width;
            this.color = color;

        }


        public void SetzeBitmapPixel(int höhe, int breite, Color color)
        {
            bitmap.SetPixel(höhe, breite, color);
        }

        private Color[,] colors(int höhe, int breite)
        {
            Color[,] color = new Color[höhe, breite];
            return color;

        }

        public void FülleBitmap()
        {
            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    bitmap.SetPixel(j, i, color[bitmap.Height - i - 1, j]);
                }
            }

        }

    }
}
