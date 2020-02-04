using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoehenGenerator
{
    class ErmittleBitFarbeAusBitmap
    {
        Color pixelColor;
        String bilddateiname;
        String pngbildname;
        String bmpbildname;
        String pfad;
        Bitmap ausgangsbild;
        Bitmap bearbeitungsbild;
        Rectangle rechteck;
        double doubleosmbreite;
        double doubleosmlänge;
        int bildbreite;
        int bildhöhe;
        PixelFormat pixelFormat = PixelFormat.Format24bppRgb;

        public  ErmittleBitFarbeAusBitmap(OSM_Koordinaten oSM_Koordinaten, string osmpfad)
        {
            bilddateiname = oSM_Koordinaten.Dateiname;
            doubleosmbreite = oSM_Koordinaten.Kachelb;
            doubleosmlänge = oSM_Koordinaten.Kachell;
            this.pfad = osmpfad;
           
            pngbildname = bilddateiname + ".png";
            bmpbildname = bilddateiname + ".bmp";
            if (!File.Exists(pngbildname))
                OSM_Fileliste.HoleOsmDaten(oSM_Koordinaten.Osmauflösung,"OSM", pfad,oSM_Koordinaten.Osmbreite,oSM_Koordinaten.Osmlänge);
            if (!File.Exists(bmpbildname))
                WandleBildUm();
            bearbeitungsbild = new Bitmap(bmpbildname);
            pixelColor = bearbeitungsbild.GetPixel((int)(bearbeitungsbild.Height * oSM_Koordinaten.Kachelb), (int)(bearbeitungsbild.Width * oSM_Koordinaten.Kachell));
        }

        private void WandleBildUm()
        {
            ausgangsbild = new Bitmap(pngbildname);
            bildbreite = ausgangsbild.Width;
            bildhöhe = ausgangsbild.Height;
            rechteck = new Rectangle(0, 0, bildbreite, bildhöhe);
            bearbeitungsbild = ausgangsbild.Clone(rechteck, pixelFormat);
       
            bearbeitungsbild.Save(bmpbildname, ImageFormat.Bmp);

        }

        public Color PixelColor { get => pixelColor; set => pixelColor = value; }
    }
}
