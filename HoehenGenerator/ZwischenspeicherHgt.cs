using System;
using System.Collections.Generic;

namespace HoehenGenerator
{
    internal class ZwischenspeicherHgt
    {
        private short[,] höhen;
        private int auflösung;
        private GeoPunkt linksunten;
        private GeoPunkt linksoben;
        private GeoPunkt rechtsunten;
        private GeoPunkt rechtsoben;
        private int anzahlLat, anzahlLon;

        public ZwischenspeicherHgt(GeoPunkt linksunten, int anzahlLon, int anzahlLat, int auflösung)
        {
            this.auflösung = auflösung;
            this.linksunten = linksunten;


            this.anzahlLat = anzahlLat;
            this.anzahlLon = anzahlLon;
            this.höhen = new short[AnzahlLon, AnzahlLat];
            this.linksoben = new GeoPunkt(Linksunten.Lon, Linksunten.Lat + anzahlLat / 3600.0 * auflösung);
            this.rechtsunten = new GeoPunkt(Linksunten.Lon + AnzahlLon / 3600.0 * auflösung, Linksunten.Lat);
            this.rechtsoben = new GeoPunkt(Linksunten.Lon + AnzahlLon / 3600.0 * auflösung, Linksunten.Lat + anzahlLat / 3600.0 * auflösung);
        }

        public ZwischenspeicherHgt(GeoPunkt linksunten, GeoPunkt rechtsoben, int auflösung)
        {
            this.auflösung = auflösung;
            this.linksunten = linksunten;
            double zwischenwert;

            this.rechtsoben = rechtsoben;
            this.linksoben = new GeoPunkt(linksunten.Lon, rechtsoben.Lat);
            this.rechtsunten = new GeoPunkt(rechtsoben.Lon, linksunten.Lat);
            this.anzahlLat = (int)((rechtsoben.Lat - linksunten.Lat) / auflösung * 3600) + 1;
            zwischenwert = rechtsoben.Lon - linksunten.Lon;
            if (zwischenwert < 0)
                //zwischenwert = 360.0 + zwischenwert;



                this.anzahlLon = (int)((360.0 + zwischenwert) / auflösung * 3600);
            else this.anzahlLon = (int)((zwischenwert) / auflösung * 3600 + 1);
            this.höhen = new short[AnzahlLon, AnzahlLat];
        }

        public double HöheVonPunkt(GeoPunkt geoPunkt)
        {
            //double interpolierteHöhe = BerechneHöhe(geoPunkt);
            double interpolierteHöhe = InterpoliereHöhe(geoPunkt);
            return interpolierteHöhe;
        }

        private double InterpoliereHöhe(GeoPunkt geoPunkt)
        {
            double zwischenwert = geoPunkt.Lon - linksunten.Lon;
            if (zwischenwert < 0) zwischenwert += 360;
            double doLat = (geoPunkt.Lat - linksunten.Lat) / auflösung * 3600.0;
            double doLon = (zwischenwert) / auflösung * 3600.0;
            int wertLat = (int)doLat;
            int wertLon = (int)doLon;
            double restLat = doLat - wertLat;
            double restLon = doLon - wertLon;

            double[,] ndata = new double[4, 4];
            for (int X = 0; X < 4; X++)
                for (int Y = 0; Y < 4; Y++)
                {


                    //Smoothing done by averaging the general area around the coords.
                    int istLat = wertLat + (Y - 1);
                    int istLon = wertLon + (X - 1);
                    if (istLat < 0)
                        istLat = 0;
                    if (istLon < 0)
                        istLon = 0;
                    if (istLon > anzahlLon - 1)
                        istLon = anzahlLon - 1;
                    if (istLat > anzahlLat - 1)
                        istLat = anzahlLat - 1;

                    ndata[Y, X] = höhen[istLon, istLat];
                    if (ndata[Y, X] == 0)
                    {

                    }
                }
            double x1 = CubicPolate(ndata[0, 0], ndata[1, 0], ndata[2, 0], ndata[3, 0], restLat);
            double x2 = CubicPolate(ndata[0, 1], ndata[1, 1], ndata[2, 1], ndata[3, 1], restLat);
            double x3 = CubicPolate(ndata[0, 2], ndata[1, 2], ndata[2, 2], ndata[3, 2], restLat);
            double x4 = CubicPolate(ndata[0, 3], ndata[1, 3], ndata[2, 3], ndata[3, 3], restLat);

            double y1 = CubicPolate(x1, x2, x3, x4, restLon);
            return y1;
        }
        private static double CubicPolate(double v0, double v1, double v2, double v3, double fracy)
        {
            double A = (v3 - v2) - (v0 - v1);
            double B = (v0 - v1) - A;
            double C = v2 - v0;
            double D = v1;

            return A * Math.Pow(fracy, 3) + B * Math.Pow(fracy, 2) + C * fracy + D;

        }
        //private double BerechneHöhe(GeoPunkt geoPunkt)
        //{
        //    // TODO: richtige Interpolation einführen Überprüfung Reihenfolge der Rückgabewerte


        //    int wertLat = (int)((geoPunkt.Lat - linksunten.Lat) / auflösung * 3600.0);
        //    int wertLon = (int)((geoPunkt.Lon - linksunten.Lon) / auflösung * 3600.0);
        //    if (wertLat < 0)
        //        wertLat = 0;
        //    if (wertLon < 0)
        //        wertLon = 0;
        //    if (wertLon > anzahlLat - 1)
        //        wertLon = anzahlLat - 1;
        //    if (wertLat > anzahlLon - 1)
        //        wertLat = anzahlLon - 1;

        //    // throw new NotImplementedException();
        //    return höhen[anzahlLon - wertLat, wertLon];
        //}
        public void LeseSpeicherEin(VierEcken vierEcken, List<FileMitEckKoordinaten> fileMitEcks)
        {
            bool zweiReihen = true;
            bool zweiSpalten = true;
            FileMitEckKoordinaten file;
            if (vierEcken.Hgtlinksoben.Name == vierEcken.Hgtlinksunten.Name)
                zweiReihen = false;

            if (vierEcken.Hgtlinksoben.Name == vierEcken.Hgtrechtsoben.Name)
                zweiSpalten = false;
            file = fileMitEcks.Find(x => x.Name == vierEcken.Hgtlinksunten.Name);

            LeseEin(vierEcken, "lu", vierEcken.Verzeichnispfad, file);
            if (zweiReihen)
            {
                file = fileMitEcks.Find(x => x.Name == vierEcken.Hgtlinksoben.Name);
                LeseEin(vierEcken, "lo", vierEcken.Verzeichnispfad, file);

            }
            if (zweiSpalten)
            {
                file = fileMitEcks.Find(x => x.Name == vierEcken.Hgtrechtsunten.Name);
                LeseEin(vierEcken, "ru", vierEcken.Verzeichnispfad, file);

            }
            if (zweiReihen && zweiSpalten)
            {
                file = fileMitEcks.Find(x => x.Name == vierEcken.Hgtrechtsoben.Name);
                LeseEin(vierEcken, "ro", vierEcken.Verzeichnispfad, file);

            }
        }

        private void LeseEin(VierEcken hgtname, string v, string pfad, FileMitEckKoordinaten fileMitEcks)
        {
            HGTFile hGTFile;
            //short[,] daten;


            switch (v)
            {


                case "lu":
                    hGTFile = new HGTFile(auflösung, pfad + "\\" + hgtname.Hgtlinksunten.Name + ".hgt");
        
                    for (int i = fileMitEcks.Linksunten[1]; i < fileMitEcks.Rechtsoben[1]; i++)
                    {
                        for (int j = fileMitEcks.Linksunten[0]; j < fileMitEcks.Rechtsoben[0]; j++)
                        {
                            höhen[j - fileMitEcks.Linksunten[0], i - fileMitEcks.Linksunten[1]] = hGTFile.HgtDaten[i, j];

                        }
                    }
                    //short test = hGTFile.HgtDaten[1200, 1200];
                     break;
                case "lo":
                    hGTFile = new HGTFile(auflösung, pfad + "\\" + hgtname.Hgtlinksoben.Name + ".hgt");
                    for (int i = fileMitEcks.Linksunten[1]; i < fileMitEcks.Rechtsoben[1] + 1; i++)
                    {
                        for (int j = fileMitEcks.Linksunten[0]; j < fileMitEcks.Rechtsoben[0]; j++)
                        {
                            höhen[j - fileMitEcks.Linksunten[0], AnzahlLat - 1 + i - fileMitEcks.Rechtsoben[1]] = hGTFile.HgtDaten[i, j];
                        }
                    }
                     break;
                case "ru":
                    hGTFile = new HGTFile(auflösung, pfad + "\\" + hgtname.Hgtrechtsunten.Name + ".hgt");
                    //daten = hGTFile.HgtDaten;
                    // 
                    for (int i = fileMitEcks.Linksunten[1]; i < fileMitEcks.Rechtsoben[1]; i++)
                    {
                        for (int j = fileMitEcks.Linksunten[0]; j < fileMitEcks.Rechtsoben[0] + 1; j++)
                        {
                            //höhen[j - fileMitEcks.Linksunten[0], AnzahlLat + i - fileMitEcks.Rechtsoben[1]] = hGTFile.HgtDaten[i, j];





                            höhen[AnzahlLon - 1 + j - fileMitEcks.Rechtsoben[0], i - fileMitEcks.Linksunten[1]] = hGTFile.HgtDaten[i, j];
                        }
                    }                  //MessageBox.Show("Zweig ru");
                    // 
                    break;
                case "ro":
                    // 
                    hGTFile = new HGTFile(auflösung, pfad + "\\" + hgtname.Hgtrechtsoben.Name + ".hgt");
                    //daten = hGTFile.HgtDaten;
                    // 
                    // 
                    for (int i = fileMitEcks.Linksunten[1]; i < fileMitEcks.Rechtsoben[1] + 1; i++)
                    {
                        for (int j = fileMitEcks.Linksunten[0]; j < fileMitEcks.Rechtsoben[0] + 1; j++)
                        {
                            höhen[AnzahlLon - 1 + j - fileMitEcks.Rechtsoben[0], AnzahlLat - 1 + i - fileMitEcks.Rechtsoben[1]] = hGTFile.HgtDaten[i, j];
                        }
                    }                  //MessageBox.Show("Zweig ro");
                    break;

                default:
                    break;

            }
            //MessageBox.Show("Einlesen von " + hgtname.Name + "Lage: " + v + " Verzeichnis: " + pfad);
            //daten = null;
        }


        public short[,] Höhen { get => höhen; set => höhen = value; }
        public int Auflösung { get => auflösung; set => auflösung = value; }


        public int AnzahlLat { get => anzahlLat; set => anzahlLat = value; }
        public int AnzahlLon { get => anzahlLon; set => anzahlLon = value; }
        internal GeoPunkt Linksunten { get => linksunten; set => linksunten = value; }
    }
}
