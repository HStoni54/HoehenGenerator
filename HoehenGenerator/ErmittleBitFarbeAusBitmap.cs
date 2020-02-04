using System;
using System.Collections.Generic;
using System.Drawing;
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
        Bitmap ausgangsbild;
        Bitmap bearbeitungsbild;
        double osmhöhe;
        double osmbreite;
        int bildbreite;
        int bildhöhe;

        public ErmittleBitFarbeAusBitmap(string bilddateiname, double osmhöhe, double osmbreite)
        {
            this.bilddateiname = bilddateiname;
            this.osmhöhe = osmhöhe;
            this.osmbreite = osmbreite;
            if (this.bilddateiname.EndsWith(".png",true, CultureInfo.CurrentCulture))
                WandleBildUm();
        }

        private void WandleBildUm()
        {
            throw new NotImplementedException();
        }

        public Color PixelColor { get => pixelColor; set => pixelColor = value; }
    }
}
