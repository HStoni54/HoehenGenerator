using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoehenGenerator
{
    class OSM_Koordinaten
    {
        private GeoPunkt geoPunkt;
        private int osmauflösung = 0;
        private int osmbreite;
        private int osmhöhe;
        private int kachelhöhe;
        private int kachelbreite;

        public OSM_Koordinaten(GeoPunkt geoPunkt)
        {
            this.geoPunkt = geoPunkt;
        }

        public OSM_Koordinaten(GeoPunkt geoPunkt, int osmauflösung) : this(geoPunkt)
        {
            this.Osmauflösung = osmauflösung;
        }

        public int Osmbreite { get => osmbreite; set => osmbreite = value; }
        public int Osmhöhe { get => osmhöhe; set => osmhöhe = value; }
        public int Osmauflösung { get => osmauflösung; set => osmauflösung = value; }
        public int Kachelhöhe { get => kachelhöhe; set => kachelhöhe = value; }
        public int Kachelbreite { get => kachelbreite; set => kachelbreite = value; }

        public void BerechneOSMKachel(int osmauflösung)
        {
            this.Osmauflösung = osmauflösung;
            BerechneOSMKachel();
        }

        public void BerechneOSMKachel()
        {

        }
    }
}
