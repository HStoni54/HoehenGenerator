using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HoehenGenerator
{
    class VierEcken
    {
        GeoPunkt linksunten;
        GeoPunkt rechtsoben;
        HgtmitKoordinaten hgtlinksunten;
        HgtmitKoordinaten hgtlinksoben;
        HgtmitKoordinaten hgtrechtsunten;
        HgtmitKoordinaten hgtrechtsoben;
        int auflösung;

        public VierEcken(GeoPunkt linksunten, GeoPunkt rechtsoben, int auflösung)
        {
            this.linksunten = linksunten;
            this.rechtsoben = rechtsoben;
            this.auflösung = auflösung;
            BestimmePunkte();
        }

        private void BestimmePunkte()
        {
            hgtlinksoben = BestimmeHgtFile(linksunten.Lon, rechtsoben.Lat, "lo");
            hgtlinksunten = BestimmeHgtFile(linksunten.Lon, linksunten.Lat, "lu");
            hgtrechtsoben = BestimmeHgtFile(rechtsoben.Lon, rechtsoben.Lat, "ro");
            hgtrechtsunten = BestimmeHgtFile(rechtsoben.Lon, linksunten.Lat, "ru");
        }

        private HgtmitKoordinaten BestimmeHgtFile(double lon, double lat, string v)
        {
            string hgt;
            int lat1=0, lon1=0;
            int lat2 = (int)lat;
            int lon2 = (int)lon;
            lat1 = (int)((lat - (int)lat) * 3600 / auflösung);
            lon1 = (int)((lon - (int)lon) * 3600 / auflösung);
            switch (v)
            {
                case "lo":
                    lat1 += 3;
                    lon1 -= 1;
                    break;
                case "lu":
                    lat1 -= 1;
                    lon1 -= 1;
                    break;
                case "ro":
                    lat1 += 3;
                    lon1 += 3;
                    break;
                case "ru":
                    lat1 -= 1;
                    lon1 += 3;
                    break;
            }
            if (lat2 >= 0)
            {
                hgt = "N" + lat2.ToString("D2");
            }
            else
            {
                hgt = "S" + (-lat2).ToString("D2");
            }
            if (lon2 >= 0)
            {
                hgt = hgt + "E" + lon2.ToString("D3");
            }
            else
            {
                hgt = hgt + "W" + (-lon2).ToString("D3");
            }
            return  new HgtmitKoordinaten(hgt,lon1,lat1);;
        }

        internal HgtmitKoordinaten Hgtlinksunten { get => hgtlinksunten; set => hgtlinksunten = value; }
        internal HgtmitKoordinaten Hgtlinksoben { get => hgtlinksoben; set => hgtlinksoben = value; }
        internal HgtmitKoordinaten Hgtrechtsunten { get => hgtrechtsunten; set => hgtrechtsunten = value; }
        internal HgtmitKoordinaten Hgtrechtsoben { get => hgtrechtsoben; set => hgtrechtsoben = value; }
    }
}
