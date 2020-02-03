using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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
                    string downloadname = "https://" + "a" + ".tile.openstreetmap.de/" + osmauflösung.ToString(CultureInfo.CurrentCulture) + "/" + j.ToString(CultureInfo.CurrentCulture) + "/" + i.ToString(CultureInfo.CurrentCulture) + ".png";
                    //string downloadname = "https://" + "a" + ".tile.openstreetmap.org/" + osmauflösung.ToString(CultureInfo.CurrentCulture) + "/" + j.ToString(CultureInfo.CurrentCulture) + "/" + i.ToString(CultureInfo.CurrentCulture) + ".png";
                    if (!File.Exists(dateinamekomplett))
                    {
                        LadeOSMDateien(downloadname, dateinamekomplett);
                       
                        
                    }

                }
            }
        }

        private static bool LadeOSMDateien(string v, string zielname)
        {
            bool ergebnis = true;
            WebClient webClient = new WebClient()
            {
                Encoding = Encoding.UTF8
         };
            webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            try
            {
                webClient.DownloadFile(v, zielname);
            }
            catch (Exception)
            {

                MessageBox.Show("Fehler! Kann Datei: " + v +
                   " nicht downloaden!\nBitte überprüfen Sie Ihre Internetverbindung");
                ergebnis = false;
            }

            webClient.Dispose();
            return ergebnis;
        }
    }
}
