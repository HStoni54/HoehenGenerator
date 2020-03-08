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
        private int lastRow = -1;
        private int auflösung;
        private int minLat, maxLat, minLon, maxLon, dimLat, dimLon;
        
        public MapConverter(string mappath, string[] maptypen, GeoPunkt gplinksunten, GeoPunkt gprechtsoben, int gpauflösung)
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
            for (int i = 0; i < readers.Length; i++)
            {
                readers[i] = new MapReader[dimLon];
                for (int j = 0; j < readers[i].Length; j++)
                {
                    readers[i][j] = new MapReader(minLat + i, minLon + j,mappath, maptypen, auflösung, 256, 256);
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
            MapReader rdr = readers[oSM_Koordinaten.Osmbreite][oSM_Koordinaten.Osmlänge];
            if (rdr.read == false)
            {
                rdr.PrepRead();
            }
            //color = rdr.farbe(oSM_Koordinaten.)
            return color;
        }
    }
}
