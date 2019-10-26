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
            hgtlinksoben = BestimmeHgtFile(linksoben.Lon, linksoben.Lat, "lo");
            hgtlinksunten = BestimmeHgtFile(linksunten.Lon, linksunten.Lat, "lu");
            hgtrechtsoben = BestimmeHgtFile(rechtsoben.Lon, rechtsoben.Lat, "ro");
            hgtrechtsunten = BestimmeHgtFile(rechtsunten.Lon, rechtsunten.Lat, "ru");
        }

        private HgtmitKoordinaten BestimmeHgtFile(double lon, double lat, string v)
        {
            string hgt;
            int lat1 = 0, lon1 = 0;
            int lat2 = (int)lat;
            int lon2 = (int)lon;
            lat1 = (int)((lat - (int)lat) * 3600 / auflösung);
            lon1 = (int)((lon - (int)lon) * 3600 / auflösung);
            switch (v)
            {
                case "lo":
                    lat1 += 3;
                    if (lat1 > 3600 / auflösung)
                        lat1 = 3600 / auflösung;
                    lon1 -= 1;
                    if (lon1 < 0)
                        lon1 = 0;
                    break;
                case "lu":
                    lat1 -= 1;
                    if (lat1 < 0)
                        lat1 = 0;
                    lon1 -= 1;
                    if (lon1 < 0)
                        lon1 = 0;
                    break;
                case "ro":
                    lat1 += 3;
                    if (lat1 > 3600 / auflösung)
                        lat1 = 3600 / auflösung;
                    lon1 += 3;
                    if (lon1 > 3600 / auflösung)
                        lon1 = 3600 / auflösung;
                    break;
                case "ru":
                    lat1 -= 1;
                    if (lat1 < 0)
                        lat1 = 0;
                    lon1 += 3;
                    if (lon1 > 3600 / auflösung)
                        lon1 = 3600 / auflösung;
                    break;
            }
            if (lat2 >= 0)
            {
                hgt = "N" + lat2.ToString("D2", CultureInfo.CurrentCulture);
            }
            else
            {
                hgt = "S" + (-lat2).ToString("D2", CultureInfo.CurrentCulture);
            }
            if (lon2 >= 0 && lon2 < 180)
            {
                hgt = hgt + "E" + lon2.ToString("D3", CultureInfo.CurrentCulture);
            }
            else {
                if (lon2 >= 180) lon2 = lon2 - 360;
            
                hgt = hgt + "W" + (-lon2).ToString("D3", CultureInfo.CurrentCulture);
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
