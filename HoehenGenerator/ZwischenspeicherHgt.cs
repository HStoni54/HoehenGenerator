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
            this.linksoben = new GeoPunkt(Linksunten.Lon, Linksunten.Lat + anzahlLat / 3600.0 * auflösung);
            this.rechtsunten = new GeoPunkt(Linksunten.Lon + AnzahlLon / 3600.0 * auflösung, Linksunten.Lat);
            this.rechtsoben = new GeoPunkt(Linksunten.Lon + AnzahlLon / 3600.0 * auflösung, Linksunten.Lat + anzahlLat / 3600.0 * auflösung);
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
            // TODO: richtige Interpolation einführen Überprüfung Reihenfolge der Rückgabewerte


            int wertLat = anzahlLon - (int)((geoPunkt.Lat - linksunten.Lat) / auflösung * 3600.0);
            int wertLon = (int)((geoPunkt.Lon - linksunten.Lon) / auflösung * 3600.0);
            if (wertLat < 0)
                wertLat = 0;
            if (wertLon < 0)
                wertLon = 0;
            if (wertLon > anzahlLat - 1)
                wertLon = anzahlLat - 1;
            if (wertLat > anzahlLon - 1)
                wertLat = anzahlLon - 1;

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
                    //daten = hGTFile.LeseDaten();
                    //daten = hGTFile.HgtDaten;
                    // 
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
                    //daten = hGTFile.HgtDaten; reihen und Spalten vertauschen
                    // 
                    for (int i = fileMitEcks.Linksunten[1]; i < fileMitEcks.Rechtsoben[1]; i++)
                    {
                        for (int j = fileMitEcks.Linksunten[0]; j < fileMitEcks.Rechtsoben[0]; j++)
                        {
                            //höhen[i - fileMitEcks.Linksunten[1], AnzahlLat - 1 + fileMitEcks.Rechtsoben[0] - j ] = hGTFile.HgtDaten[i, j];
                            höhen[AnzahlLon + i - fileMitEcks.Rechtsoben[1], j - fileMitEcks.Linksunten[0]] = hGTFile.HgtDaten[i, j];
                        }
                    }                
                    // 
                    //  MessageBox.Show("Zweig lo");
                    break;
                case "ru":
                    hGTFile = new HGTFile(auflösung, pfad + "\\" + hgtname.Hgtrechtsunten.Name + ".hgt");
                    //daten = hGTFile.HgtDaten;
                    // 
                    for (int i = fileMitEcks.Linksunten[1]; i < fileMitEcks.Rechtsoben[1]; i++)
                    {
                        for (int j = fileMitEcks.Linksunten[0]; j < fileMitEcks.Rechtsoben[0]; j++)
                        {
                            höhen[i - fileMitEcks.Linksunten[1], AnzahlLat + j - fileMitEcks.Rechtsoben[0]] = hGTFile.HgtDaten[i, j];
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
                    for (int i = fileMitEcks.Linksunten[1]; i < fileMitEcks.Rechtsoben[1]; i++)
                    {
                        for (int j = fileMitEcks.Linksunten[0]; j < fileMitEcks.Rechtsoben[0]; j++)
                        {
                            höhen[AnzahlLon + i - fileMitEcks.Rechtsoben[1], AnzahlLat + j - fileMitEcks.Rechtsoben[0]] = hGTFile.HgtDaten[i, j];
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
