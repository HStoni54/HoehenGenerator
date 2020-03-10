﻿using System;
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
            string downloadname = ""; ;

            string dateiname = osmauflösung.ToString(CultureInfo.CurrentCulture) + "_" + osmbreite.ToString(CultureInfo.CurrentCulture) + "_" + osmlänge.ToString(CultureInfo.CurrentCulture) + ".png"; // TODO IFormatprovider einsetzen
            string dateinamekomplett = osmpfad + "\\" + osmtyp + "_" + dateiname;
            if (osmtyp == "OSM")
            {
                Random random = new Random();
                char ServerZufall = (char)random.Next(97, 99);
                downloadname = "https://" + ServerZufall + ".tile.openstreetmap.de/" + osmauflösung.ToString(CultureInfo.CurrentCulture) + "/" + osmlänge.ToString(CultureInfo.CurrentCulture) + "/" + osmbreite.ToString(CultureInfo.CurrentCulture) + ".png";

            }
            else if (osmtyp == "ORM")
            {
                Random random = new Random();
                char ServerZufall = (char)random.Next(97, 99);
                downloadname = "https://" + ServerZufall + ".tiles.openrailwaymap.org/standard/" + osmauflösung.ToString(CultureInfo.CurrentCulture) + "/" + osmlänge.ToString(CultureInfo.CurrentCulture) + "/" + osmbreite.ToString(CultureInfo.CurrentCulture) + ".png";

            }
            else
            {
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
                if (!LadeOSMDateien(downloadname, dateinamekomplett))
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
