
using System.Drawing;

namespace HoehenGenerator
{
    internal class ZeichneBitMap
    {
        public Bitmap Bitmap { get; }
        public int Höhe1 { get; }
        public int Breite1 { get; }
        public Color[,] Color { get; }
        public Color Color1 { get; }

        public ZeichneBitMap(Bitmap bitmap, Color color1)
        {
            Bitmap = bitmap;
            Höhe1 = bitmap.Height;
            Breite1 = bitmap.Width;
            Color1 = color1;
            Color = new Color[bitmap.Height, bitmap.Width];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    Color[j, i] = color1;
                }
            }

            {

            }


        }

        public ZeichneBitMap(Bitmap bitmap, Color[,] color)
        {
            Bitmap = bitmap;
            Höhe1 = bitmap.Height;
            Breite1 = bitmap.Width;
            Color = color;

        }


        public void SetzeBitmapPixel(int höhe, int breite, Color color)
        {
            Bitmap.SetPixel(höhe, breite, color);
        }

        private Color[,] Colors(int höhe, int breite)
        {
            Color[,] color = new Color[höhe, breite];
            return color;

        }

        public void FülleBitmap()
        {
            for (int i = 0; i < Bitmap.Height; i++)
            {
                for (int j = 0; j < Bitmap.Width; j++)
                {
                    Bitmap.SetPixel(j, i, Color[Bitmap.Height - i - 1, j]);
                }
            }

        }

    }
}
