using System;
using System.Globalization;

namespace HoehenGenerator
{
    internal class OSM_Koordinaten
    {
        private GeoPunkt geoPunkt;
        private int kachelanzahl;

        public OSM_Koordinaten(GeoPunkt geoPunkt)
        {
            this.geoPunkt = geoPunkt;
        }

        public OSM_Koordinaten(GeoPunkt geoPunkt, int osmauflösung) : this(geoPunkt)
        {
            Osmauflösung = osmauflösung;
            kachelanzahl = (int)Math.Pow(2, Osmauflösung);
        }

        public int Osmbreite { get; set; }
        public int Osmlänge { get; set; }
        public int Osmauflösung { get; set; } = 0;
        public int Kachelbreite { get; set; }
        public int Kachellänge { get; set; }
        public double Kachell { get; set; }
        public double Kachelb { get; set; }
        public string Dateiname { get; set; }


        public void BerechneOSMKachel(GeoPunkt geoPunkt, int osmauflösung)
        {
            this.geoPunkt = geoPunkt;
            BerechneOSMKachel(osmauflösung);
        }
        public void BerechneOSMKachel(int osmauflösung)
        {
            Osmauflösung = osmauflösung;
            kachelanzahl = (int)Math.Pow(2, Osmauflösung);
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
            double osmb = (1 - (Math.Log(Math.Tan(geoPunkt.Lat * Math.PI / 180) + 1 / Math.Cos(geoPunkt.Lat * Math.PI / 180))) / Math.PI) / 2 * kachelanzahl;
            double osml = (180 + geoPunkt.Lon) % 360 / 360 * kachelanzahl;
            Osmlänge = (int)osml;
            Osmbreite = (int)osmb;
            Kachelb = osmb - Osmbreite;
            Kachell = osml - Osmlänge;
            //kachelbreite = (int)(((90 - geoPunkt.Lat) / 180 * kachelanzahl * 512) % 512);
            //kachelhöhe = (int)(((180 + geoPunkt.Lon) / 360 * kachelanzahl * 512) % 512);
            Kachellänge = (int)(Kachell * 512);
            Kachelbreite = (int)(Kachelb * 512);
            Dateiname = Osmauflösung.ToString(CultureInfo.CurrentCulture) + "_" + Osmbreite.ToString(CultureInfo.CurrentCulture) + "_" + Osmlänge.ToString(CultureInfo.CurrentCulture);
        }
    }
}
