using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoehenGenerator
{
    class OSM_Fileliste
    {
        private GeoPunkt georechtsoben;
        private GeoPunkt geolinksunten;
        private int auflösung;

        private OSM_Koordinaten osmrechtsoben;
        private OSM_Koordinaten osmlinksunten;
        private int anzahlhoch;
        private int anzahlbreit;

        public OSM_Fileliste(GeoPunkt georechtsoben, GeoPunkt geolinksunten, int auflösung)
        {
            this.georechtsoben = georechtsoben;
            this.geolinksunten = geolinksunten;
            this.auflösung = auflösung;
            osmrechtsoben = new OSM_Koordinaten(this.georechtsoben, this.auflösung);
            osmrechtsoben.BerechneOSMKachel();
            osmlinksunten = new OSM_Koordinaten(this.geolinksunten, this.auflösung);
            osmlinksunten.BerechneOSMKachel();
        }
        public void OSM_LadeFiles()
        {
            for (int i = osmlinksunten.Osmbreite; i >= osmrechtsoben.Osmbreite; i--)
            {
                for (int j = osmlinksunten.Osmlänge; j <= osmrechtsoben.Osmlänge; j++)
                {
                    //TODO: hier die Ladeprozedur einschieben
                }
            }
        }
    }
}
