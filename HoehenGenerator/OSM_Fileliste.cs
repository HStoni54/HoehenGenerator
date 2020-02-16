using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
        //private int anzahlhoch;
        //private int anzahlbreit;

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
            if (!Directory.Exists(pfad + "\\" + osmtyp))

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
            string osmpfad = pfad + "\\" + osmtyp + "\\";

            for (int i = osmlinksunten.Osmbreite; i >= osmrechtsoben.Osmbreite; i--)
            {
                if (osmlinksunten.Osmlänge <= osmrechtsoben.Osmlänge)
                {
                    for (int j = osmlinksunten.Osmlänge; j <= osmrechtsoben.Osmlänge; j++)
                    {
                        //TODO: hier die Ladeprozedur einschieben
                        HoleOsmDaten(osmauflösung, osmtyp, osmpfad, i, j);

                    }
                }
                else
                {
                    int kachelanzahl = (int)Math.Pow(2, osmauflösung);

                    for (int j = osmlinksunten.Osmlänge; j < kachelanzahl; j++)
                    {
                        //TODO: hier die Ladeprozedur einschieben
                        HoleOsmDaten(osmauflösung, osmtyp, osmpfad, i, j);

                    }
                    for (int j = 0; j <= osmrechtsoben.Osmlänge; j++)
                    {
                        //TODO: hier die Ladeprozedur einschieben
                        HoleOsmDaten(osmauflösung, osmtyp, osmpfad, i, j);

                    }
                }
            }
        }

        public static void HoleOsmDaten(int osmauflösung, string osmtyp, string osmpfad, int osmbreite, int osmlänge)
        {
            string dateiname = osmtyp + "_" + osmauflösung.ToString(CultureInfo.CurrentCulture) + "_" + osmbreite.ToString(CultureInfo.CurrentCulture) + "_" + osmlänge.ToString(CultureInfo.CurrentCulture) + ".png"; // TODO IFormatprovider einsetzen
            string dateinamekomplett = osmpfad + "\\" + dateiname;
            string downloadname = "https://" + "a" + ".tile.openstreetmap.de/" + osmauflösung.ToString(CultureInfo.CurrentCulture) + "/" + osmlänge.ToString(CultureInfo.CurrentCulture) + "/" + osmbreite.ToString(CultureInfo.CurrentCulture) + ".png";
            //string downloadname = "https://" + "a" + ".tile.openstreetmap.org/" + osmauflösung.ToString(CultureInfo.CurrentCulture) + "/" + osmlänge.ToString(CultureInfo.CurrentCulture) + "/" + osmbreite.ToString(CultureInfo.CurrentCulture) + ".png";
            if (!File.Exists(dateinamekomplett))
            {
                if (!LadeOSMDateien(downloadname, dateinamekomplett))
                {
                    Color color = Color.FromArgb(255, 16, 39, 0);
                    Bitmap dummy = new Bitmap(256, 256);
                    for (int i = 0; i < dummy.Height; i++)
                        for (int j = 0; j < dummy.Width; j++)
                        {
                            dummy.SetPixel(i, j, color);
                        }
                    {

                    }
                    dummy.Save(dateinamekomplett);
                    dummy.Dispose();
                }


            }
            WandleBildInBmp(dateinamekomplett);
        }

        private static void WandleBildInBmp(string dateinamekomplett)
        {
            string dateinameneu = dateinamekomplett.Substring(0, dateinamekomplett.Length - 4) + ".bmp";
            if (!File.Exists(dateinameneu))
            {
                Bitmap ausgangsbild = new Bitmap(dateinamekomplett);
                int bildbreite = ausgangsbild.Width;
                int bildhöhe = ausgangsbild.Height;
                Rectangle rechteck = new Rectangle(0, 0, bildbreite, bildhöhe);
                Bitmap bearbeitungsbild = ausgangsbild.Clone(rechteck, PixelFormat.Format24bppRgb);

                bearbeitungsbild.Save(dateinameneu, ImageFormat.Bmp);
                bearbeitungsbild.Dispose();
                ausgangsbild.Dispose();
            }

        }

        public static bool LadeOSMDateien(string v, string zielname)
        {
            bool ergebnis = true;
            WebClient webClient = new WebClient()
            {
                Encoding = Encoding.UTF8
            };
            // webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            webClient.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0)");
            //Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0

            try
            {
                webClient.DownloadFile(v, zielname);
            }
            catch (Exception)
            {

                //MessageBox.Show("Fehler! Kann Datei: " + v +
                //   " nicht downloaden!\nBitte überprüfen Sie Ihre Internetverbindung");
                ergebnis = false;
            }

            webClient.Dispose();
            return ergebnis;
        }
    }
}
