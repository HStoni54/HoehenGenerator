using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoehenGenerator
{
    class MapReader
    {
        private bool read = false;
        private Bitmap buffer, tempbuffer, tempbuffer2;
        public string path, mappath;
        private int BildGröße;
        private string mapname;
        int auflösung;
        int lat, lon;
        string[] maptype;
        string dateiname;

        public MapReader(int lat, int lon, string mapPath, string[] maptype, int auflösung, int BildGröße)
        {
            this.BildGröße = BildGröße;
            mappath = mapPath;
            this.auflösung = auflösung;
            this.maptype = maptype;
            this.lat = lat;
            this.lon = lon;
            //GeoPunkt geoPunkt = new GeoPunkt(lon, lat);
            //OSM_Koordinaten oSM_Koordinaten = new OSM_Koordinaten(geoPunkt,auflösung);
            //oSM_Koordinaten.BerechneOSMKachel();
            //mapname = maptype + "_" + oSM_Koordinaten.Dateiname;
            // TODO: Bitmapdateien identifizieren und gegebenenfalls downloaden
        }
        public void PrepRead()
        {     
            buffer = new Bitmap(BildGröße, BildGröße, PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(tempbuffer);
            graphics.CompositingMode = CompositingMode.SourceOver;


            foreach (string maptyp in maptype)
            {
  
                mapname = maptyp + "_" + auflösung.ToString(CultureInfo.CurrentCulture) + "_" + lat.ToString(CultureInfo.CurrentCulture) + "_" + lon.ToString(CultureInfo.CurrentCulture);

                if (!File.Exists(mappath + "\\" + mapname + ".png"))
                {
                    OSM_Fileliste.HoleOsmDaten(auflösung, maptyp, path, lat, lon);
                    System.Threading.Thread.Sleep(1000);
                }
                path = mappath + "\\" + mapname + ".png";
                tempbuffer = new Bitmap(mappath + "\\" + mapname + ".png");
                tempbuffer2 = new Bitmap(tempbuffer, BildGröße, BildGröße);
                graphics.DrawImage(tempbuffer2, 0, 0);
                tempbuffer.Dispose();
                tempbuffer2.Dispose();

            }
            buffer.Save(mappath + "\\" + mapname + ".bmp",ImageFormat.Bmp);
            //buffer = new Bitmap(tempbuffer); 
            read = true;

            // TODO: Bitmap(s) einlesen
        }
        public Color farbe(int x, int y)
        {
            Color color = Color.FromArgb(255, 200, 200, 200);
            if (!read && path != null)
            {
                PrepRead();
            }
            if (buffer == null)
                return color;
            if (x >= 0 && x < BildGröße && y >= 0 && y < BildGröße)
            {



                return buffer.GetPixel(x, y);
            }
            else
                return color;
        }
        public bool FreeBuf()
        {
            if (buffer == null)
                return false;
            buffer = null;
            read = false;
            return true;
        }
    }
}
