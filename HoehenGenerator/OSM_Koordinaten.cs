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
        private int kachelhöhe;
        private int kachelbreite;
        private string dateiname;

        public OSM_Koordinaten(GeoPunkt geoPunkt)
        {
            this.geoPunkt = geoPunkt;
        }

        public OSM_Koordinaten(GeoPunkt geoPunkt, int osmauflösung) : this(geoPunkt)
        {
            this.osmauflösung = osmauflösung;
            kachelanzahl = (int)Math.Pow(2, this.osmauflösung);
        }

        public int Osmbreite { get => osmbreite; set => osmbreite = value; }
        public int Osmlänge { get => osmlänge; set => osmlänge = value; }
        public int Osmauflösung { get => osmauflösung; set => osmauflösung = value; }
        public int Kachelhöhe { get => kachelhöhe; set => kachelhöhe = value; }
        public int Kachelbreite { get => kachelbreite; set => kachelbreite = value; }
        public void BerechneOSMKachel(GeoPunkt geoPunkt, int osmauflösung)
        {
            this.geoPunkt = geoPunkt;
            BerechneOSMKachel(osmauflösung);
        }
        public void BerechneOSMKachel(int osmauflösung)
        {
            this.osmauflösung = osmauflösung;
            kachelanzahl = (int)Math.Pow(2, this.osmauflösung);
            BerechneOSMKachel();
        }

        public void BerechneOSMKachel()
        {
            osmbreite = (int)((((90 - geoPunkt.Lat ) )/ 180 * kachelanzahl) ); // TODO Breite-Werte stimmen nicht
            osmlänge = (int)((((180 + geoPunkt.Lon ) ) / 360 * kachelanzahl) );
            kachelbreite = (int)(((90 - geoPunkt.Lat) / 180 * kachelanzahl * 512) % 512);
            kachelhöhe = (int)(((180 + geoPunkt.Lon) / 360 * kachelanzahl * 512) % 512);
            dateiname = osmauflösung.ToString(CultureInfo.CurrentCulture) + "_" + osmbreite.ToString(CultureInfo.CurrentCulture) + "_" + osmlänge.ToString(CultureInfo.CurrentCulture); // TODO IFormatprovider einsetzen
        }
    }
}
