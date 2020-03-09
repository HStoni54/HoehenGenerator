using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoehenGenerator
{
    class MapConverter
    {
        private MapReader[][] readers;
       private int auflösung;
        private int minLat, maxLat, minLon, maxLon, dimLat, dimLon;
        private int bildbreite, bildhöhe;

        public MapConverter(string mappath, string[] maptypen, GeoPunkt gplinksunten, GeoPunkt gprechtsoben, int gpauflösung, GeoPunkt mittelpunkt, int rasterdichte)
        {
            auflösung = gpauflösung;
            OSM_Koordinaten rechtsoben = new OSM_Koordinaten(gprechtsoben, auflösung);
            rechtsoben.BerechneOSMKachel();
            OSM_Koordinaten linksunten = new OSM_Koordinaten(gplinksunten, auflösung);
            linksunten.BerechneOSMKachel();
            minLat = rechtsoben.Osmbreite;
            maxLat = linksunten.Osmbreite;
            minLon = linksunten.Osmlänge;
            maxLon = rechtsoben.Osmlänge;
            dimLat = maxLat - minLat + 1;
            dimLon = maxLon - minLon + 1;
            readers = null;
            readers = new MapReader[dimLat][];
            double osmpunkte = 256 / (40030 / Math.Pow(2, auflösung) * Math.Cos(mittelpunkt.Lat / 180 * Math.PI));
            bildbreite = 256 * rasterdichte / (int)osmpunkte;
             bildhöhe = 256 * rasterdichte / (int)osmpunkte;

            for (int i = 0; i < readers.Length; i++)
            {
                readers[i] = new MapReader[dimLon];
                for (int j = 0; j < readers[i].Length; j++)
                {
                    readers[i][j] = new MapReader(minLat + i, minLon + j, mappath, maptypen, auflösung, bildhöhe, bildbreite);
                    //readers[i][j].PrepRead();
                }
            }
        }

        public void PrepReaders()
        {
            for (int i = 0; i < dimLat; i++)
            {
                for (int j = 0; j < dimLon; j++)
                {
                    readers[i][j].PrepRead();
                }
            }
        }

        public Color GibFarbe(GeoPunkt geoPunkt)
        {
            Color color = Color.White;
            OSM_Koordinaten oSM_Koordinaten = new OSM_Koordinaten(geoPunkt, auflösung);
            oSM_Koordinaten.BerechneOSMKachel();
            if (oSM_Koordinaten.Osmbreite >= minLat && oSM_Koordinaten.Osmbreite <= maxLat && oSM_Koordinaten.Osmlänge >= minLon && oSM_Koordinaten.Osmlänge <= maxLon)
            {

           
            MapReader rdr = readers[oSM_Koordinaten.Osmbreite - minLat][oSM_Koordinaten.Osmlänge - minLon];
            if (rdr.read == false)
            {
                rdr.PrepRead();
            }

            color = rdr.Farbe((int)(rdr.BildHöhe * oSM_Koordinaten.Kachell), (int)(rdr.BildBreite * oSM_Koordinaten.Kachelb));
            }
            return color;
        }

        public void FreeBuff()
        {
            for (int i = 0; i < dimLat; i++)
            {
                for (int j = 0; j < dimLon; j++)
                {
                    readers[i][j].FreeBuf();
                }
            }
        }
    }
}
