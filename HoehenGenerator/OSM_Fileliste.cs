using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace HoehenGenerator
{
    internal static class OSM_Fileliste
    {




        public static void HoleOsmDaten(int osmauflösung, string osmtyp, string osmpfad, int osmbreite, int osmlänge)
        {
            string downloadname = "";
            bool osm = true;
            string dateinamekomplett;
            string dateiname = osmauflösung.ToString(CultureInfo.CurrentCulture) + "_" + osmbreite.ToString(CultureInfo.CurrentCulture) + "_" + osmlänge.ToString(CultureInfo.CurrentCulture);
            if (osmtyp == "OSM")
            {
                dateinamekomplett = osmpfad + "\\" + osmtyp + "_" + dateiname + ".png";
                Random random = new Random();
                char ServerZufall = (char)random.Next(97, 99);
                downloadname = "https://" + ServerZufall + ".tile.openstreetmap.de/" + osmauflösung.ToString(CultureInfo.CurrentCulture) + "/" + osmlänge.ToString(CultureInfo.CurrentCulture) + "/" + osmbreite.ToString(CultureInfo.CurrentCulture) + ".png";

            }
            else if (osmtyp == "ORM")
            {
                dateinamekomplett = osmpfad + "\\" + osmtyp + "_" + dateiname + ".png";
                Random random = new Random();
                char ServerZufall = (char)random.Next(97, 99);
                downloadname = "https://" + ServerZufall + ".tiles.openrailwaymap.org/standard/" + osmauflösung.ToString(CultureInfo.CurrentCulture) + "/" + osmlänge.ToString(CultureInfo.CurrentCulture) + "/" + osmbreite.ToString(CultureInfo.CurrentCulture) + ".png";

            }

            else if (osmtyp == "GoM")
            {
                dateinamekomplett = osmpfad + "\\" + osmtyp + "_" + dateiname + ".jpg";
                osm = false;
                Random random = new Random();
                int ServerZufall = random.Next(0, 3); //TODO: als Stream downloaden
                downloadname = "https://" + "mt" + ServerZufall + ".google.com/vt/lyrs=s&x=" + osmlänge.ToString(CultureInfo.CurrentCulture) + "&y=" + osmbreite.ToString(CultureInfo.CurrentCulture) + "&z=" + osmauflösung.ToString(CultureInfo.CurrentCulture);
                //downloadname = "https://" + ServerZufall + ".tiles.openrailwaymap.org/standard/" + osmauflösung.ToString(CultureInfo.CurrentCulture) + "/" + osmlänge.ToString(CultureInfo.CurrentCulture) + "/" + osmbreite.ToString(CultureInfo.CurrentCulture) + ".png";

            }
            else
            {
                dateinamekomplett = osmpfad + "\\" + osmtyp + "_" + dateiname + ".png";
                Color color = Color.FromArgb(255, 200, 200, 200);
                Bitmap bitmap = new Bitmap(256, 256);
                for (int i = 0; i < bitmap.Height; i++)
                {
                    for (int j = 0; j < bitmap.Width; j++)
                    {
                        bitmap.SetPixel(i, j, color);
                    }
                }

                bitmap.Save(dateinamekomplett);
                bitmap.Dispose();
            }
            if (!File.Exists(dateinamekomplett))
            {
                if (!LadeOSMDateien(downloadname, dateinamekomplett, osm))
                {
                    Color color = Color.FromArgb(255, 16, 39, 0);
                    Bitmap dummy = new Bitmap(256, 256);
                    for (int i = 0; i < dummy.Height; i++)
                    {
                        for (int j = 0; j < dummy.Width; j++)
                        {
                            dummy.SetPixel(i, j, color);
                        }
                    }

                    {

                    }
                    dummy.Save(dateinamekomplett);
                    dummy.Dispose();
                }



            }
            //WandleBildInBmp(dateinamekomplett,osmauflösung);
        }



        public static bool LadeOSMDateien(string v, string zielname, bool osm = true)
        {
            bool ergebnis = false;
            WebClient webClient = new WebClient()
            {
                Encoding = Encoding.UTF8
            };
            // webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            webClient.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0)");
            //Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0
            if (osm)
            {
                try
                {
                    webClient.DownloadFile(v, zielname);
                    ergebnis = true;
                }
                catch (Exception)
                {

                    //MessageBox.Show("Fehler! Kann Datei: " + v +
                    //   " nicht downloaden!\nBitte überprüfen Sie Ihre Internetverbindung");
                    ergebnis = false;
                }
            }
            else
            {
                try
                {
                    Stream dat = webClient.OpenRead(v);
                    FileStream file;
                    try
                    {
                        file = File.Create(zielname);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    dat.CopyTo(file);
                    dat.Close();
                    file.Close();

                    ergebnis = true;
                }
                catch (Exception)
                {

                    ergebnis = false;
                }
            }
            webClient.Dispose();
            return ergebnis;
        }

    }
}
