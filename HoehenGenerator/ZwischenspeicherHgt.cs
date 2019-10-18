using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public ZwischenspeicherHgt(int auflösung, GeoPunkt linksunten, int anzahlLat, int anzahlLon)
        {
            this.auflösung = auflösung;
            this.linksunten = linksunten;
           
            
            this.anzahlLat = anzahlLat;
            this.anzahlLon = anzahlLon;
            this.höhen = new short[AnzahlLat,AnzahlLon];
            this.linksoben = new GeoPunkt(Linksunten.Lon, Linksunten.Lat + anzahlLat / 1200 * auflösung);
            this.rechtsunten = new GeoPunkt(Linksunten.Lon + AnzahlLon / 1200 * auflösung, Linksunten.Lat);
            this.rechtsoben = new GeoPunkt(Linksunten.Lon + AnzahlLon / 1200 * auflösung, Linksunten.Lat + anzahlLat / 1200 * auflösung);
        }

        public ZwischenspeicherHgt(int auflösung, GeoPunkt linksunten, GeoPunkt rechtsoben)
        {
            this.auflösung = auflösung;
            this.linksunten = linksunten;
           
            
            this.rechtsoben = rechtsoben;
            this.linksoben = new GeoPunkt(linksunten.Lon, rechtsoben.Lat);
            this.rechtsunten = new GeoPunkt(rechtsoben.Lon, linksunten.Lat);
            this.anzahlLat = (int)((rechtsoben.Lat - linksunten.Lat) / auflösung * 1200);
            this.anzahlLon = (int)((rechtsoben.Lon - linksunten.Lon) / auflösung * 1200);
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
            return höhen[wertLat,wertLon];
        }

        public short[,] Höhen { get => höhen; set => höhen = value; }
        public int Auflösung { get => auflösung; set => auflösung = value; }
       
       
        public int AnzahlLat { get => anzahlLat; set => anzahlLat = value; }
        public int AnzahlLon { get => anzahlLon; set => anzahlLon = value; }
        internal GeoPunkt Linksunten { get => linksunten; set => linksunten = value; }
    }
}
