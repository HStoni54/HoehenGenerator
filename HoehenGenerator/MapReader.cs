using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoehenGenerator
{
    class MapReader
    {
        private Color[] buffer;

        public MapReader(int lat, int lon, string mapPath, string maptype, int auflösung, int BildGröße)
        {
            // TODO: Bitmapdateien identifizieren und gegebenenfalls downloaden
        }
        public void Prepread()
        {
            // TODO: Bitmap(s) einlesen
        }
        public Color farbe(int x, int y)
        {
            Color color =  Color.FromArgb(255, 200, 200, 200); // TODO: Dummy
            return color;
        }
    }
}
