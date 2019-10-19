using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HoehenGenerator
{
    class ZwischenspeicherHgt
    {
        short[,] höhen;
        int auflösung;
        GeoPunkt linksunten;
        GeoPunkt linksoben;
        GeoPunkt rechtsunten;
        GeoPunkt rechtsoben;



        int anzahlLat, anzahlLon;

        public ZwischenspeicherHgt(GeoPunkt linksunten, int anzahlLat, int anzahlLon, int auflösung)
        {
            this.auflösung = auflösung;
            this.linksunten = linksunten;


            this.anzahlLat = anzahlLat;
            this.anzahlLon = anzahlLon;
            this.höhen = new short[AnzahlLat, AnzahlLon];
            this.linksoben = new GeoPunkt(Linksunten.Lon, Linksunten.Lat + anzahlLat / 3600 * auflösung);
            this.rechtsunten = new GeoPunkt(Linksunten.Lon + AnzahlLon / 3600 * auflösung, Linksunten.Lat);
            this.rechtsoben = new GeoPunkt(Linksunten.Lon + AnzahlLon / 3600 * auflösung, Linksunten.Lat + anzahlLat / 3600 * auflösung);
        }

        public ZwischenspeicherHgt(GeoPunkt linksunten, GeoPunkt rechtsoben, int auflösung)
        {
            this.auflösung = auflösung;
            this.linksunten = linksunten;


            this.rechtsoben = rechtsoben;
            this.linksoben = new GeoPunkt(linksunten.Lon, rechtsoben.Lat);
            this.rechtsunten = new GeoPunkt(rechtsoben.Lon, linksunten.Lat);
            this.anzahlLat = (int)((rechtsoben.Lat - linksunten.Lat) / auflösung * 3600);
            this.anzahlLon = (int)((rechtsoben.Lon - linksunten.Lon) / auflösung * 3600);
            this.höhen = new short[AnzahlLat, AnzahlLon];
        }

        public short HöheVonPunkt(GeoPunkt geoPunkt)
        {
            short interpolierteHöhe = BerechneHöhe(geoPunkt);
            return interpolierteHöhe;
        }

        private short BerechneHöhe(GeoPunkt geoPunkt)
        {
            // TODO: richtige Interpolation einführen
            int wertLon = (int)((geoPunkt.Lon - linksunten.Lon) / auflösung * 1200);
            int wertLat = (int)((geoPunkt.Lat - linksunten.Lat) / auflösung * 1200);

            // throw new NotImplementedException();
            return höhen[wertLat, wertLon];
        }
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
            short[,] daten;


            switch (v)
            {


                case "lu":
                    hGTFile = new HGTFile(auflösung, pfad + "\\" + hgtname.Hgtlinksunten.Name + ".hgt");
                    daten = hGTFile.LeseDaten();
                    // TODO: Werte für einlesen definieren

                    //     MessageBox.Show("Zweig lu");
                    break;
                case "lo":
                    hGTFile = new HGTFile(auflösung, pfad + "\\" + hgtname.Hgtlinksoben.Name + ".hgt");
                    daten = hGTFile.LeseDaten();
                    // TODO: Werte für einlesen definieren
                    //  MessageBox.Show("Zweig lo");
                    break;
                case "ru":
                    hGTFile = new HGTFile(auflösung, pfad + "\\" + hgtname.Hgtrechtsunten.Name + ".hgt");
                    daten = hGTFile.LeseDaten();
                    //MessageBox.Show("Zweig ru");
                    // TODO: Werte für einlesen definieren
                    break;
                case "ro":
                    //MessageBox.Show("Zweig ro");
                    // TODO: Werte für einlesen definieren
                    hGTFile = new HGTFile(auflösung, pfad + "\\" + hgtname.Hgtrechtsoben.Name + ".hgt");
                    daten = hGTFile.LeseDaten();
                    break;

                default:
                    break;

            }
            //MessageBox.Show("Einlesen von " + hgtname.Name + "Lage: " + v + " Verzeichnis: " + pfad);
            daten = null;
        }

        public short[,] Höhen { get => höhen; set => höhen = value; }
        public int Auflösung { get => auflösung; set => auflösung = value; }


        public int AnzahlLat { get => anzahlLat; set => anzahlLat = value; }
        public int AnzahlLon { get => anzahlLon; set => anzahlLon = value; }
        internal GeoPunkt Linksunten { get => linksunten; set => linksunten = value; }
    }
}
