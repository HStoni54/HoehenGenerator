using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoehenGenerator
{
    class ErmittleBitFarbeAusBitmap
    {
        Color pixelColor;
        String bilddateiname;
        String neuerbildname;
        Bitmap ausgangsbild;
        Bitmap bearbeitungsbild;
        Rectangle rechteck;
        double osmhöhe;
        double osmbreite;
        int bildbreite;
        int bildhöhe;
        PixelFormat pixelFormat = PixelFormat.Format24bppRgb;

        public ErmittleBitFarbeAusBitmap(string bilddateiname, double osmhöhe, double osmbreite)
        {
            this.bilddateiname = bilddateiname;
            this.osmhöhe = osmhöhe;
            this.osmbreite = osmbreite;
            if (this.bilddateiname.EndsWith(".png", true, CultureInfo.CurrentCulture))
                WandleBildUm();
            else if (this.bilddateiname.EndsWith(".bmp", true, CultureInfo.CurrentCulture))
                neuerbildname = bilddateiname;
        }

        private void WandleBildUm()
        {
            ausgangsbild = new Bitmap(bilddateiname);
            bildbreite = ausgangsbild.Width;
            bildhöhe = ausgangsbild.Height;
            rechteck = new Rectangle(0, 0, bildbreite, bildhöhe);
            bearbeitungsbild = ausgangsbild.Clone(rechteck, pixelFormat);
            neuerbildname = bilddateiname.Substring(0, bilddateiname.Length - 4) + ".bmp";
            bearbeitungsbild.Save(neuerbildname, ImageFormat.Bmp);

        }

        public Color PixelColor { get => pixelColor; set => pixelColor = value; }
    }
}
