﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

namespace HoehenGenerator
{
    internal class MapReader : IDisposable
    {
        public bool read = false;
        private Bitmap buffer, tempbuffer, tempbuffer2;
        public string path, mappath;
        public int BildBreite, BildHöhe;
        private string mapname;
        private readonly int auflösung;
        private readonly int lat, lon;
        private readonly string[] maptype;

        public MapReader(int lat, int lon, string mapPath, string[] maptype, int auflösung, int BildBreite, int BildHöhe)
        {
            this.BildBreite = Math.Max(BildBreite, BildHöhe);
            this.BildHöhe = Math.Max(BildBreite, BildHöhe);
            mappath = mapPath;
            this.auflösung = auflösung;
            this.maptype = maptype;
            this.lat = lat;
            this.lon = lon;
            buffer = null;
            tempbuffer = null;
            tempbuffer2 = null;
            //GeoPunkt geoPunkt = new GeoPunkt(lon, lat);
            //OSM_Koordinaten oSM_Koordinaten = new OSM_Koordinaten(geoPunkt,auflösung);
            //oSM_Koordinaten.BerechneOSMKachel();
            //mapname = maptype + "_" + oSM_Koordinaten.Dateiname;
            // TODO: Bitmapdateien identifizieren und gegebenenfalls downloaden
        }
        public void PrepRead()
        {
            if (read == false)
            {
                buffer = new Bitmap(BildHöhe, BildBreite, PixelFormat.Format32bppArgb);
                Graphics graphics = Graphics.FromImage(buffer);
                graphics.CompositingMode = CompositingMode.SourceOver;


                foreach (string maptyp in maptype)
                {
                    if (maptyp == "OSM" || maptyp == "ORM")
                    {
                        mapname = maptyp + "_" + auflösung.ToString(CultureInfo.CurrentCulture) + "_" + lat.ToString(CultureInfo.CurrentCulture) + "_" + lon.ToString(CultureInfo.CurrentCulture);

                        if (!File.Exists(mappath + "\\" + mapname + ".png"))
                        {
                            OSM_Fileliste.HoleOsmDaten(auflösung, maptyp, mappath, lat, lon);
                            System.Threading.Thread.Sleep(1000);
                        }
                        path = mappath + "\\" + mapname + ".png";
                        tempbuffer = new Bitmap(mappath + "\\" + mapname + ".png");
                        tempbuffer2 = new Bitmap(tempbuffer, BildHöhe, BildBreite);
                        graphics.DrawImage(tempbuffer2, 0, 0);
                        tempbuffer.Dispose();
                        tempbuffer2.Dispose();
                    }
                    else if (maptyp == "GMS" || maptyp == "GMM" || maptyp == "GMH")
                    {
                        mapname = maptyp + "_" + auflösung.ToString(CultureInfo.CurrentCulture) + "_" + lat.ToString(CultureInfo.CurrentCulture) + "_" + lon.ToString(CultureInfo.CurrentCulture);

                        if (!File.Exists(mappath + "\\" + mapname + ".jpg"))
                        {
                            OSM_Fileliste.HoleOsmDaten(auflösung, maptyp, mappath, lat, lon);
                            System.Threading.Thread.Sleep(1000);
                        }
                        path = mappath + "\\" + mapname + ".jpg";
                        tempbuffer = new Bitmap(mappath + "\\" + mapname + ".jpg");
                        tempbuffer2 = new Bitmap(tempbuffer, BildHöhe, BildBreite);
                        graphics.DrawImage(tempbuffer2, 0, 0);
                        tempbuffer.Dispose();
                        tempbuffer2.Dispose();

                    }
                    else
                    {
                        tempbuffer2 = new Bitmap(BildHöhe, BildBreite, PixelFormat.Format32bppArgb);
                        for (int i = 0; i < BildHöhe; i++)
                        {
                            for (int j = 0; j < BildBreite; j++)
                            {
                                tempbuffer2.SetPixel(i, j, Color.LightGray);
                            }
                        }
                        graphics.DrawImage(tempbuffer2, 0, 0);
                        tempbuffer2.Dispose();
                    }
                }
                //buffer.Save(mappath + "\\" + maptype[0] + "_" + auflösung.ToString(CultureInfo.CurrentCulture) + "_" + lat.ToString(CultureInfo.CurrentCulture) + "_" + lon.ToString(CultureInfo.CurrentCulture) + ".bmp");
                // buffer = new Bitmap(tempbuffer2); 

                read = true;
            }

            // TODO: Bitmap(s) einlesen
        }
        public Color Farbe(int x, int y)
        {
            Color color = Color.FromArgb(255, 200, 200, 200);
            if (!read && path != null)
            {
                PrepRead();
            }
            if (buffer == null)
            {
                return color;
            }

            if (x >= 0 && x < BildHöhe && y >= 0 && y < BildBreite)
            {



                return buffer.GetPixel(x, y);
            }
            else
            {
                return color;
            }
        }
        public bool FreeBuf()
        {
            if (buffer == null)
            {
                return false;
            }

            if (buffer != null)
            {
                buffer.Dispose();
            }

            if (tempbuffer != null)
            {
                tempbuffer.Dispose();
            }

            if (tempbuffer2 != null)
            {
                tempbuffer2.Dispose();
            }

            read = false;
            return true;
        }

        public void Dispose()
        {
            FreeBuf();
            if (buffer != null)
            {
                buffer.Dispose();
            }

            if (tempbuffer != null)
            {
                tempbuffer.Dispose();
            }

            if (tempbuffer2 != null)
            {
                tempbuffer2.Dispose();
            }

            read = false;
        }
    }
}
