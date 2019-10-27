using System.Globalization;

namespace HoehenGenerator
{
    internal class VierEcken
    {
        private GeoPunkt linksunten;
        private GeoPunkt rechtsoben;
        private GeoPunkt linksoben;
        private GeoPunkt rechtsunten;
        private HgtmitKoordinaten hgtlinksunten;
        private HgtmitKoordinaten hgtlinksoben;
        private HgtmitKoordinaten hgtrechtsunten;
        private HgtmitKoordinaten hgtrechtsoben;
        private string verzeichnispfad;
        private int auflösung;

        public VierEcken(GeoPunkt linksunten, GeoPunkt rechtsoben, int auflösung)
        {
            this.linksunten = linksunten;
            this.rechtsoben = rechtsoben;
            this.auflösung = auflösung;
            this.linksoben = new GeoPunkt(linksunten.Lon, rechtsoben.Lat);
            this.rechtsunten = new GeoPunkt(rechtsoben.Lon, linksunten.Lat);
            BestimmePunkte();
        }

        private void BestimmePunkte()
        {
            hgtlinksoben = BestimmeHgtFile(linksoben.Lon - 0.01, linksoben.Lat + 0.01, "lo");
            hgtlinksunten = BestimmeHgtFile(linksunten.Lon - 0.01, linksunten.Lat - 0.01, "lu");
            hgtrechtsoben = BestimmeHgtFile(rechtsoben.Lon + 0.01, rechtsoben.Lat + 0.01, "ro");
            hgtrechtsunten = BestimmeHgtFile(rechtsunten.Lon + 0.01, rechtsunten.Lat - 0.01, "ru");
        }

        private HgtmitKoordinaten BestimmeHgtFile(double lon, double lat, string v)
        {
            string hgt;
            int lat1 = 0, lon1 = 0;
            int lat2 = (int)lat;
            int lon2 = (int)lon;
            lat1 = (int)((lat - (int)lat) * 3600 / auflösung);
            lon1 = (int)((lon - (int)lon) * 3600 / auflösung);
 
            if (lat2 >= 0)
            {
                hgt = "N" + lat2.ToString("D2", CultureInfo.CurrentCulture);
            }
            else
            {
                hgt = "S" + (-lat2 + 1).ToString("D2", CultureInfo.CurrentCulture);
                lat1 = 3600 / auflösung + lat1;
            }
            if (lon2 >= 0 && lon2 < 180)
            {
                hgt = hgt + "E" + lon2.ToString("D3", CultureInfo.CurrentCulture);
            }
            else {
                if (lon2 >= 180)
                {
                    lon2 = lon2 - 360; 
                    hgt = hgt + "W" + (-lon2).ToString("D3", CultureInfo.CurrentCulture);
                }
                else
                {
                lon1 = 3600 / auflösung + lon1;
                    hgt = hgt + "W" + (-lon2 + 1).ToString("D3", CultureInfo.CurrentCulture);

                }




            }
            return new HgtmitKoordinaten(hgt, lon1, lat1); ;
        }

        internal HgtmitKoordinaten Hgtlinksunten { get => hgtlinksunten; set => hgtlinksunten = value; }
        internal HgtmitKoordinaten Hgtlinksoben { get => hgtlinksoben; set => hgtlinksoben = value; }
        internal HgtmitKoordinaten Hgtrechtsunten { get => hgtrechtsunten; set => hgtrechtsunten = value; }
        internal HgtmitKoordinaten Hgtrechtsoben { get => hgtrechtsoben; set => hgtrechtsoben = value; }
        public int Auflösung { get => auflösung; set => auflösung = value; }
        public string Verzeichnispfad { get => verzeichnispfad; set => verzeichnispfad = value; }
    }
}
