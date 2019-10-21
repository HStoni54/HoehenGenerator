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

        public ZwischenspeicherHgt(GeoPunkt linksunten, int anzahlLon, int anzahlLat, int auflösung)
        {
            this.auflösung = auflösung;
            this.linksunten = linksunten;


            this.anzahlLat = anzahlLat;
            this.anzahlLon = anzahlLon;
            this.höhen = new short[AnzahlLon, AnzahlLat];
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
            this.höhen = new short[AnzahlLon, AnzahlLat];
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
                // TODO einlesen "2Hoch un 4Dateien" stimmt nicht  

                case "lu":
                    hGTFile = new HGTFile(auflösung, pfad + "\\" + hgtname.Hgtlinksunten.Name + ".hgt");
                    //daten = hGTFile.LeseDaten();
                    //daten = hGTFile.HgtDaten;
                    // TODO: Werte für einlesen definieren
                    /* beginn und ende im HgtFile -> steht im File also vorher auslesen
                     * Anzahl ergibt sich daraus
                     * Beginn oder Ende  im Datenfile das einzige, was sich ändert
                     * 
                     * Ich habe mal wieder Lat und Lon vertauscht
                     * 
                     * Hier stimmt etwas noch nicht,  es werden falsche Zahlen ausgelesen 
                     */
                    for (int i = fileMitEcks.Linksunten[1];  i < fileMitEcks.Rechtsoben[1]; i++)
                    {
                        for (int j = fileMitEcks.Linksunten[0] ; j < fileMitEcks.Rechtsoben[0]; j++)
                        {
                            höhen[i - fileMitEcks.Linksunten[1], j - fileMitEcks.Linksunten[0]] = hGTFile.HgtDaten[i, j];
                        }
                    }

                    //     MessageBox.Show("Zweig lu");
                    break;
                case "lo":
                    hGTFile = new HGTFile(auflösung, pfad + "\\" + hgtname.Hgtlinksoben.Name + ".hgt");
                    //daten = hGTFile.HgtDaten;
                    // TODO: für höhen[,] j korrigierern (AnzahlLat)
                    for (int i = fileMitEcks.Linksunten[0]; i < fileMitEcks.Rechtsoben[0]; i++)
                    {
                        for (int j = fileMitEcks.Linksunten[1]; j < fileMitEcks.Rechtsoben[1]; j++)
                        {
                            höhen[i - fileMitEcks.Linksunten[0], j - fileMitEcks.Linksunten[1] + AnzahlLat - fileMitEcks.Rechtsoben[1]] = hGTFile.HgtDaten[i, j];
                        }
                    }                
                    // TODO: Werte für einlesen definieren s.o.
                    //  MessageBox.Show("Zweig lo");
                    break;
                case "ru":
                    hGTFile = new HGTFile(auflösung, pfad + "\\" + hgtname.Hgtrechtsunten.Name + ".hgt");
                    //daten = hGTFile.HgtDaten;
                    // TODO: für höhen[,] i korrigierern (AnzahlLon)
                    for (int i = fileMitEcks.Linksunten[0]; i < fileMitEcks.Rechtsoben[0]; i++)
                    {
                        for (int j = fileMitEcks.Linksunten[1]; j < fileMitEcks.Rechtsoben[1]; j++)
                        {
                            höhen[i - fileMitEcks.Linksunten[0] + AnzahlLon - fileMitEcks.Rechtsoben[0], j - fileMitEcks.Linksunten[1]] = hGTFile.HgtDaten[i, j];
                        }
                    }                  //MessageBox.Show("Zweig ru");
                    // TODO: Werte für einlesen definieren s.o.
                    break;
                case "ro":
                   // TODO: Werte für einlesen definieren s.o.
                    hGTFile = new HGTFile(auflösung, pfad + "\\" + hgtname.Hgtrechtsoben.Name + ".hgt");
                    //daten = hGTFile.HgtDaten;
                    // TODO: für höhen[,] j korrigierern (AnzahlLat)
                    // TODO: für höhen[,] i korrigierern (AnzahlLon)
                    for (int i = fileMitEcks.Linksunten[0]; i < fileMitEcks.Rechtsoben[0]; i++)
                    {
                        for (int j = fileMitEcks.Linksunten[1]; j < fileMitEcks.Rechtsoben[1]; j++)
                        {
                            höhen[i - fileMitEcks.Linksunten[0] + AnzahlLon - fileMitEcks.Rechtsoben[0], j - fileMitEcks.Linksunten[1] + AnzahlLat - fileMitEcks.Rechtsoben[1]] = hGTFile.HgtDaten[i, j];
                        }
                    }                  //MessageBox.Show("Zweig ro");
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
