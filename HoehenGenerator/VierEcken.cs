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

        public VierEcken(GeoPunkt linksunten, GeoPunkt rechtsoben)
        {
            this.linksunten = linksunten;
            this.rechtsoben = rechtsoben;
            BestimmePunkte();
        }

        private void BestimmePunkte()
        {
            hgtlinksoben = BestimmeHgtFile( linksunten.Lon, rechtsoben.Lat,"lo");
            hgtlinksunten = BestimmeHgtFile(linksunten.Lon, linksunten.Lat, "lu");
            hgtrechtsoben = BestimmeHgtFile(rechtsoben.Lon, rechtsoben.Lat, "ro");
            hgtrechtsunten = BestimmeHgtFile(rechtsoben.Lon, linksunten.Lat, "ru");
        }

        private HgtmitKoordinaten BestimmeHgtFile(double lon, double lat, string v)
        {
            throw new NotImplementedException();
        }

        internal HgtmitKoordinaten Hgtlinksunten { get => hgtlinksunten; set => hgtlinksunten = value; }
        internal HgtmitKoordinaten Hgtlinksoben { get => hgtlinksoben; set => hgtlinksoben = value; }
        internal HgtmitKoordinaten Hgtrechtsunten { get => hgtrechtsunten; set => hgtrechtsunten = value; }
        internal HgtmitKoordinaten Hgtrechtsoben { get => hgtrechtsoben; set => hgtrechtsoben = value; }
    }
}
