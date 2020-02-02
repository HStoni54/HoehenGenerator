using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
        public void OSM_LadeFiles(int osmauflösung, string osmtyp, string pfad) // TODO: Datumsgrenze
        {
            if (!Directory.Exists(pfad + "\\"+ osmtyp))

                try
                {

                    Directory.CreateDirectory(pfad + "\\" + osmtyp);
                }
                catch (Exception)
                {

                    MessageBox.Show("Kann Directory für OSM-Dateien nicht erstellen!\n"
                        + "Überprüfen Sie die Schreibberechtigung im Verzeichnis:\n"
                        + "\"" + pfad + "\"");

                }

            for (int i = osmlinksunten.Osmbreite; i >= osmrechtsoben.Osmbreite; i--)
            {
                for (int j = osmlinksunten.Osmlänge; j <= osmrechtsoben.Osmlänge; j++)
                {
                    //TODO: hier die Ladeprozedur einschieben
                    string dateiname = osmtyp + "_" + osmauflösung.ToString(CultureInfo.CurrentCulture) + "_" + i.ToString(CultureInfo.CurrentCulture) + "_" + j.ToString(CultureInfo.CurrentCulture) + ".png"; // TODO IFormatprovider einsetzen
                    string dateinamekomplett = pfad + "\\" + osmtyp + "\\" + dateiname;

                }
            }
        }
    }
}
