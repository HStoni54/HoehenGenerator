using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoehenGenerator
{
    class OSM_Koordinaten
    {
        private GeoPunkt geoPunkt;
        private int osmauflösung = 0;
        private int kachelanzahl;
        private int osmbreite;
        private int osmlänge;
        private int kachelbreite;
        private int kachellänge;
        private double kachell;
        private double kachelb;
        private string dateiname;

        public OSM_Koordinaten(GeoPunkt geoPunkt)
        {
            this.geoPunkt = geoPunkt;
        }

        public OSM_Koordinaten(GeoPunkt geoPunkt, int osmauflösung) : this(geoPunkt)
        {
            this.Osmauflösung = osmauflösung;
            kachelanzahl = (int)Math.Pow(2, this.Osmauflösung);
        }

        public int Osmbreite { get => osmbreite; set => osmbreite = value; }
        public int Osmlänge { get => osmlänge; set => osmlänge = value; }
        public int Osmauflösung { get => osmauflösung; set => osmauflösung = value; }
        public int Kachelbreite { get => kachelbreite; set => kachelbreite = value; }
        public int Kachellänge { get => kachellänge; set => kachellänge = value; }
        public double Kachell { get => kachell; set => kachell = value; }
        public double Kachelb { get => kachelb; set => kachelb = value; }
        public string Dateiname { get => dateiname; set => dateiname = value; }
       

        public void BerechneOSMKachel(GeoPunkt geoPunkt, int osmauflösung)
        {
            this.geoPunkt = geoPunkt;
            BerechneOSMKachel(osmauflösung);
        }
        public void BerechneOSMKachel(int osmauflösung)
        {
            this.Osmauflösung = osmauflösung;
            kachelanzahl = (int)Math.Pow(2, this.Osmauflösung);
            BerechneOSMKachel();
        }

        public void BerechneOSMKachel()
        {
            //osmbreite = (int)((((90 - geoPunkt.Lat ) )/ 180 * kachelanzahl) ); // TODO Breite-Werte stimmen nicht 85.0511 arctan(sinh(pi))
            //    Reproject the coordinates to the Mercator projection (from EPSG:4326 to EPSG:3857):
            //        x = lon
            //    y = arsinh(tan(lat)) = log[tan(lat) + sec(lat)]

            //    (lat and lon are in radians)

            //Transform range of x and y to 0 – 1 and shift origin to top left corner:
            //        x = [1 + (x / π)] / 2
            //    y = [1 − (y / π)] / 2
            //Calculate the number of tiles across the map, n, using 2zoom
            //Multiply x and y by n.Round results down to give tilex and tiley.
            //double y = Math.Log(Math.Tan(geoPunkt.Lat * Math.PI / 180) + 1 / Math.Cos(geoPunkt.Lat * Math.PI / 180));
            //y = (1 - (y / Math.PI)) / 2;
            double osmb = (1 - (Math.Log(Math.Tan(geoPunkt.Lat * Math.PI / 180) + 1/Math.Cos(geoPunkt.Lat * Math.PI / 180)))/Math.PI)/2 * kachelanzahl;
            double osml = (180 + geoPunkt.Lon) % 360 / 360 * kachelanzahl;
            osmlänge = (int)osml;
            osmbreite = (int)osmb;
            kachelb = osmb - osmbreite;
            kachell = osml - osmlänge;
            //kachelbreite = (int)(((90 - geoPunkt.Lat) / 180 * kachelanzahl * 512) % 512);
            //kachelhöhe = (int)(((180 + geoPunkt.Lon) / 360 * kachelanzahl * 512) % 512);
            kachellänge = (int)(kachell * 512);
            kachelbreite = (int)(kachelb * 512);
            dateiname = "OSM_" + Osmauflösung.ToString(CultureInfo.CurrentCulture) + "_" + osmbreite.ToString(CultureInfo.CurrentCulture) + "_" + osmlänge.ToString(CultureInfo.CurrentCulture); 
        }
    }
}
