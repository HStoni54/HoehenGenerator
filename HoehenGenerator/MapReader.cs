using System;
using System.Collections.Generic;
using System.Drawing;
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
        private Bitmap buffer, tempbuffer;
        public string path, mappath;
        private int BildGröße;
        private string mapname;
        int auflösung;
        int lat, lon;
        string maptype;

        public MapReader(double lat, double lon, string mapPath, string maptype, int auflösung, int BildGröße)
        {
            this.BildGröße = BildGröße;
            mappath = mapPath;
            this.auflösung = auflösung;
            this.maptype = maptype;
            this.lat = (int)lat;
            this.lon = (int)lon;
            GeoPunkt geoPunkt = new GeoPunkt(lon, lat);
            OSM_Koordinaten oSM_Koordinaten = new OSM_Koordinaten(geoPunkt,auflösung);
            oSM_Koordinaten.BerechneOSMKachel();
            mapname = maptype + "_" + oSM_Koordinaten.Dateiname;
            // TODO: Bitmapdateien identifizieren und gegebenenfalls downloaden
        }
        public void PrepRead()
        {
            if (!File.Exists(mappath + "\\" + mapname + ".png"))
            {
                OSM_Fileliste.HoleOsmDaten(auflösung,maptype,path,lat,lon);
                System.Threading.Thread.Sleep(1000);
            }
            path = mappath + "\\" + mapname + ".png";
            tempbuffer = new Bitmap(mappath + "\\" + mapname + ".png");
            buffer = new Bitmap(tempbuffer, BildGröße, BildGröße);
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


  
                return buffer.GetPixel(x,y);
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
