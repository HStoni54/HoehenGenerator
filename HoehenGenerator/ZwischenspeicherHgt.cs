using System;
using System.Collections.Generic;

namespace HoehenGenerator
{
    internal class ZwischenspeicherHgt
    {
        private short[,] höhen;
        private int auflösung;
        private GeoPunkt linksunten;
        private GeoPunkt linksoben;
        private GeoPunkt rechtsunten;
        private GeoPunkt rechtsoben;
        private int anzahlLat, anzahlLon;
        

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
            double zwischenwert;

            this.rechtsoben = rechtsoben;
            this.linksoben = new GeoPunkt(linksunten.Lon, rechtsoben.Lat);
            this.rechtsunten = new GeoPunkt(rechtsoben.Lon, linksunten.Lat);
            this.anzahlLat = (int)((rechtsoben.Lat - linksunten.Lat) / auflösung * 3600) + 1;
            zwischenwert = rechtsoben.Lon - linksunten.Lon;
            if (zwischenwert < 0)
                //zwischenwert = 360.0 + zwischenwert;



                this.anzahlLon = (int)((360.0 + zwischenwert) / auflösung * 3600);
            else this.anzahlLon = (int)((zwischenwert) / auflösung * 3600 + 1);
            this.höhen = new short[AnzahlLon, AnzahlLat];
        }

        public double HöheVonPunkt(GeoPunkt geoPunkt)
        {
            //double interpolierteHöhe = BerechneHöhe(geoPunkt);
            double interpolierteHöhe = InterpoliereHöhe(geoPunkt);
            return interpolierteHöhe;
        }

        private double InterpoliereHöhe(GeoPunkt geoPunkt)
        {
            double zwischenwert = geoPunkt.Lon - linksunten.Lon;
            if (zwischenwert < 0) zwischenwert += 360;
            double doLat = (geoPunkt.Lat - linksunten.Lat) / auflösung * 3600.0;
            double doLon = (zwischenwert) / auflösung * 3600.0;
            int wertLat = (int)doLat;
            int wertLon = (int)doLon;
            //double restLat = wertLat - doLat;
            //double restLon = wertLon - doLon;
            double restLat = doLat - wertLat;
            double restLon = doLon - wertLon;

            double[,] ndata = new double[4, 4];
            for (int X = 0; X < 4; X++)
                for (int Y = 0; Y < 4; Y++)
                {


                    //Smoothing done by averaging the general area around the coords.
                    int istLat = wertLat + (Y - 1);
                    int istLon = wertLon + (X - 1);
                    if (istLat < 0)
                        istLat = 0;
                    if (istLon < 0)
                        istLon = 0;
                    if (istLon > anzahlLon - 1)
                        istLon = anzahlLon - 1;
                    if (istLat > anzahlLat - 1)
                        istLat = anzahlLat - 1;

                    ndata[Y, X] = höhen[istLon, istLat];
                    if (ndata[Y, X] <= 0)
                    {
                        //
                    }
                }
            double x1 = CubicPolate(ndata[0, 0], ndata[1, 0], ndata[2, 0], ndata[3, 0], restLat);
            double x2 = CubicPolate(ndata[0, 1], ndata[1, 1], ndata[2, 1], ndata[3, 1], restLat);
            double x3 = CubicPolate(ndata[0, 2], ndata[1, 2], ndata[2, 2], ndata[3, 2], restLat);
            double x4 = CubicPolate(ndata[0, 3], ndata[1, 3], ndata[2, 3], ndata[3, 3], restLat);

            double y1 = CubicPolate(x1, x2, x3, x4, restLon);
            //if (y1 <= 0 )
            //    {
            //    bool kleinernull = true;
            //}
            return y1;
        }
        private static double CubicPolate(double v0, double v1, double v2, double v3, double fracy)
        {
            double A = (v3 - v2) - (v0 - v1);
            double B = (v0 - v1) - A;
            double C = v2 - v0;
            double D = v1;

            return A * Math.Pow(fracy, 3) + B * Math.Pow(fracy, 2) + C * fracy + D;

        }
        //private double BerechneHöhe(GeoPunkt geoPunkt)
        //{
        //    // TODO: richtige Interpolation einführen Überprüfung Reihenfolge der Rückgabewerte


        //    int wertLat = (int)((geoPunkt.Lat - linksunten.Lat) / auflösung * 3600.0);
        //    int wertLon = (int)((geoPunkt.Lon - linksunten.Lon) / auflösung * 3600.0);
        //    if (wertLat < 0)
        //        wertLat = 0;
        //    if (wertLon < 0)
        //        wertLon = 0;
        //    if (wertLon > anzahlLat - 1)
        //        wertLon = anzahlLat - 1;
        //    if (wertLat > anzahlLon - 1)
        //        wertLat = anzahlLon - 1;

        //    // throw new NotImplementedException();
        //    return höhen[anzahlLon - wertLat, wertLon];
        //}
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
            //short[,] daten;


            switch (v)
            {


                case "lu":
                    hGTFile = new HGTFile(auflösung, pfad + "\\" + hgtname.Hgtlinksunten.Name + ".hgt");

                    for (int i = fileMitEcks.Linksunten[1]; i < fileMitEcks.Rechtsoben[1]; i++)
                    {
                        for (int j = fileMitEcks.Linksunten[0]; j < fileMitEcks.Rechtsoben[0]; j++)
                        {
                            höhen[j - fileMitEcks.Linksunten[0], i - fileMitEcks.Linksunten[1]] = hGTFile.HgtDaten[i, j];

                        }
                    }
                    //short test = hGTFile.HgtDaten[1200, 1200];
                    break;
                case "lo":
                    hGTFile = new HGTFile(auflösung, pfad + "\\" + hgtname.Hgtlinksoben.Name + ".hgt");
                    for (int i = fileMitEcks.Linksunten[1]; i < fileMitEcks.Rechtsoben[1] + 1; i++)
                    {
                        for (int j = fileMitEcks.Linksunten[0]; j < fileMitEcks.Rechtsoben[0]; j++)
                        {
                            höhen[j - fileMitEcks.Linksunten[0], AnzahlLat - 1 + i - fileMitEcks.Rechtsoben[1]] = hGTFile.HgtDaten[i, j];
                        }
                    }
                    break;
                case "ru":
                    hGTFile = new HGTFile(auflösung, pfad + "\\" + hgtname.Hgtrechtsunten.Name + ".hgt");
                    //daten = hGTFile.HgtDaten;
                    // 
                    for (int i = fileMitEcks.Linksunten[1]; i < fileMitEcks.Rechtsoben[1]; i++)
                    {
                        for (int j = fileMitEcks.Linksunten[0]; j < fileMitEcks.Rechtsoben[0] + 1; j++)
                        {
                            //höhen[j - fileMitEcks.Linksunten[0], AnzahlLat + i - fileMitEcks.Rechtsoben[1]] = hGTFile.HgtDaten[i, j];





                            höhen[AnzahlLon - 1 + j - fileMitEcks.Rechtsoben[0], i - fileMitEcks.Linksunten[1]] = hGTFile.HgtDaten[i, j];
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
                    for (int i = fileMitEcks.Linksunten[1]; i < fileMitEcks.Rechtsoben[1] + 1; i++)
                    {
                        for (int j = fileMitEcks.Linksunten[0]; j < fileMitEcks.Rechtsoben[0] + 1; j++)
                        {
                            höhen[AnzahlLon - 1 + j - fileMitEcks.Rechtsoben[0], AnzahlLat - 1 + i - fileMitEcks.Rechtsoben[1]] = hGTFile.HgtDaten[i, j];
                        }
                    }                  //MessageBox.Show("Zweig ro");
                    break;

                default:
                    break;

            }
            //MessageBox.Show("Einlesen von " + hgtname.Name + "Lage: " + v + " Verzeichnis: " + pfad);
            //daten = null;
        }


        public short[,] Höhen { get => höhen; set => höhen = value; }
        public int Auflösung { get => auflösung; set => auflösung = value; }


        public int AnzahlLat { get => anzahlLat; set => anzahlLat = value; }
        public int AnzahlLon { get => anzahlLon; set => anzahlLon = value; }
        internal GeoPunkt Linksunten { get => linksunten; set => linksunten = value; }


        private double[][] eleArray = new double[4][];
        private HGTReader[][] readers;
        private int lastRow = -1;
        private int minLat32;
        private int minLon32;
        protected  static double FACTOR = 45.0d / (1 << 29);
        private short outsidePolygonHeight = HGTReader.UNDEF;
        short h = HGTReader.UNDEF;

        private bool useComplexInterpolation = true;

        /**
         * Return elevation in meter for a point given in DEM units (32 bit res).
         * @param lat32
         * @param lon32
         * @return height in m or Short.MIN_VALUE if value is invalid 
         */
        protected short GetElevation(int lat32, int lon32)
        {
            int row = (int)((lat32 - minLat32) * FACTOR);
            int col = (int)((lon32 - minLon32) * FACTOR);

            HGTReader rdr = readers[row][col];
            if (rdr == null)
            {
                // no reader : ocean or missing file
                return outsidePolygonHeight;
            }
            int res = rdr.GetRes();
            rdr.PrepRead();
            if (res <= 0)
                return 0; // assumed to be an area in the ocean
            lastRow = row;

            double scale = res * FACTOR;

            double y1 = (lat32 - minLat32) * scale - row * res;
            double x1 = (lon32 - minLon32) * scale - col * res;
            int xLeft = (int)x1;
            int yBottom = (int)y1;
            double qx = x1 - xLeft;
            double qy = y1 - yBottom;

  
        //statPoints++;
            if (useComplexInterpolation)
            {
                // bicubic (Catmull-Rom) interpolation with 16 points
                bool filled = FillArray(rdr, row, col, xLeft, yBottom);
                if (filled)
                {
                    h = (short)Math.Round(BicubicInterpolation(eleArray, qx, qy));
                    //statBicubic++;
                }
            }

            if (h == HGTReader.UNDEF)
            {
                // use bilinear interpolation if bicubic not available
                int xRight = xLeft + 1;
                int yTop = yBottom + 1;

                int hLT = rdr.Ele(xLeft, yTop);
                int hRT = rdr.Ele(xRight, yTop);
                int hLB = rdr.Ele(xLeft, yBottom);
                int hRB = rdr.Ele(xRight, yBottom);

                h = InterpolatedHeight(qx, qy, hLT, hRT, hRB, hLB);
                //statBilinear++;
                //if (h == HGTReader.UNDEF) statVoid++;
            }

            //if (h == HGTReader.UNDEF && log.isLoggable(Level.WARNING))
            //{
            //    double lon = lon32 * FACTOR;
            //    double lat = lat32 * FACTOR;
            //    Coord c = new Coord(lat, lon);
            //    log.warn("height interpolation returns void at", c.toDegreeString());
            //}
            return h;
        }


        /**
         * Fill 16 values of HGT near required coordinates
         * can use HGTreaders near the current one
         */
        private bool FillArray(HGTReader rdr, int row, int col, int xLeft, int yBottom)
        {
            int res = rdr.GetRes();
            int minX = 0;
            int minY = 0;
            int maxX = 3;
            int maxY = 3;
            bool inside = true;

            // check borders
            if (xLeft == 0)
            {
                if (col <= 0)
                    return false;
                minX = 1;
                inside = false;
            }
            else if (xLeft == res - 1)
            {
                if (col + 1 >= readers[0].Length)
                    return false;
                maxX = 2;
                inside = false;
            }
            if (yBottom == 0)
            {
                if (row <= 0)
                    return false;
                minY = 1;
                inside = false;
            }
            else if (yBottom == res - 1)
            {
                if (row + 1 >= readers.Length)
                    return false;
                maxY = 2;
                inside = false;
            }

            // fill data from current reader
            short h;
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    h = rdr.Ele(xLeft + x - 1, yBottom + y - 1);
                    if (h == HGTReader.UNDEF)
                        return false;
                    eleArray[x][y] = h;
                }
            }

            if (inside) // no need to check borders again
                return true;

            // fill data from adjacent readers, down and up
            if (xLeft > 0 && xLeft < res - 1)
            {
                if (yBottom == 0)
                { // bottom edge
                    HGTReader rdrBB = PrepReader(res, row - 1, col);
                    if (rdrBB == null)
                        return false;
                    for (int x = 0; x <= 3; x++)
                    {
                        h = rdrBB.Ele(xLeft + x - 1, res - 1);
                        if (h == HGTReader.UNDEF)
                            return false;
                        eleArray[x][0] = h;
                    }
                }
                else if (yBottom == res - 1)
                { // top edge
                    HGTReader rdrTT = PrepReader(res, row + 1, col);
                    if (rdrTT == null)
                        return false;
                    for (int x = 0; x <= 3; x++)
                    {
                        h = rdrTT.Ele(xLeft + x - 1, 1);
                        if (h == HGTReader.UNDEF)
                            return false;
                        eleArray[x][3] = h;
                    }
                }
            }

            // fill data from adjacent readers, left and right
            if (yBottom > 0 && yBottom < res - 1)
            {
                if (xLeft == 0)
                { // left edgge
                    HGTReader rdrLL = PrepReader(res, row, col - 1);
                    if (rdrLL == null)
                        return false;
                    for (int y = 0; y <= 3; y++)
                    {
                        h = rdrLL.Ele(res - 1, yBottom + y - 1);
                        if (h == HGTReader.UNDEF)
                            return false;
                        eleArray[0][y] = h;
                    }
                }
                else if (xLeft == res - 1)
                { // right edge
                    HGTReader rdrRR = PrepReader(res, row, col + 1);
                    if (rdrRR == null)
                        return false;
                    for (int y = 0; y <= 3; y++)
                    {
                        h = rdrRR.Ele(1, yBottom + y - 1);
                        if (h == HGTReader.UNDEF)
                            return false;
                        eleArray[3][y] = h;
                    }
                }
            }

            // fill data from adjacent readers, corners
            if (xLeft == 0)
            {
                if (yBottom == 0)
                { // left bottom corner
                    HGTReader rdrLL = PrepReader(res, row, col - 1);
                    if (rdrLL == null)
                        return false;
                    for (int y = 1; y <= 3; y++)
                    {
                        h = rdrLL.Ele(res - 1, yBottom + y - 1);
                        if (h == HGTReader.UNDEF)
                            return false;
                        eleArray[0][y] = h;
                    }

                    HGTReader rdrBB = PrepReader(res, row - 1, col);
                    if (rdrBB == null)
                        return false;
                    for (int x = 1; x <= 3; x++)
                    {
                        h = rdrBB.Ele(xLeft + x - 1, res - 1);
                        if (h == HGTReader.UNDEF)
                            return false;
                        eleArray[x][0] = h;
                    }

                    HGTReader rdrLB = PrepReader(res, row - 1, col - 1);
                    if (rdrLB == null)
                        return false;
                    h = rdrLB.Ele(res - 1, res - 1);
                    if (h == HGTReader.UNDEF)
                        return false;
                    eleArray[0][0] = h;
                }
                else if (yBottom == res - 1)
                { // left top corner
                    HGTReader rdrLL = PrepReader(res, row, col - 1);
                    if (rdrLL == null)
                        return false;
                    for (int y = 0; y <= 2; y++)
                    {
                        h = rdrLL.Ele(res - 1, yBottom + y - 1);
                        if (h == HGTReader.UNDEF)
                            return false;
                        eleArray[0][y] = h;
                    }

                    HGTReader rdrTT = PrepReader(res, row + 1, col);
                    if (rdrTT == null)
                        return false;
                    for (int x = 1; x <= 3; x++)
                    {
                        h = rdrTT.Ele(xLeft + x - 1, 1);
                        if (h == HGTReader.UNDEF)
                            return false;
                        eleArray[x][3] = h;
                    }

                    HGTReader rdrLT = PrepReader(res, row + 1, col - 1);
                    if (rdrLT == null)
                        return false;
                    h = rdrLT.Ele(res - 1, 1);
                    if (h == HGTReader.UNDEF)
                        return false;
                    eleArray[0][3] = h;
                }
            }
            else if (xLeft == res - 1)
            {
                if (yBottom == 0)
                { // right bottom corner
                    HGTReader rdrRR = PrepReader(res, row, col + 1);
                    if (rdrRR == null)
                        return false;
                    for (int y = 1; y <= 3; y++)
                    {
                        h = rdrRR.Ele(1, yBottom + y - 1);
                        if (h == HGTReader.UNDEF)
                            return false;
                        eleArray[3][y] = h;
                    }

                    HGTReader rdrBB = PrepReader(res, row - 1, col);
                    if (rdrBB == null)
                        return false;
                    for (int x = 0; x <= 2; x++)
                    {
                        h = rdrBB.Ele(xLeft + x - 1, res - 1);
                        if (h == HGTReader.UNDEF)
                            return false;
                        eleArray[x][0] = h;
                    }

                    HGTReader rdrRB = PrepReader(res, row - 1, col + 1);
                    if (rdrRB == null)
                        return false;
                    h = rdrRB.Ele(1, res - 1);
                    if (h == HGTReader.UNDEF)
                        return false;
                    eleArray[3][0] = h;
                }
                else if (yBottom == res - 1)
                { // right top corner
                    HGTReader rdrRR = PrepReader(res, row, col + 1);
                    if (rdrRR == null)
                        return false;
                    for (int y = 0; y <= 2; y++)
                    {
                        h = rdrRR.Ele(1, yBottom + y - 1);
                        if (h == HGTReader.UNDEF)
                            return false;
                        eleArray[3][y] = h;
                    }

                    HGTReader rdrTT = PrepReader(res, row + 1, col);
                    if (rdrTT == null)
                        return false;
                    for (int x = 0; x <= 2; x++)
                    {
                        h = rdrTT.Ele(xLeft + x - 1, 1);
                        if (h == HGTReader.UNDEF)
                            return false;
                        eleArray[x][3] = h;
                    }

                    HGTReader rdrRT = PrepReader(res, row + 1, col + 1);
                    if (rdrRT == null)
                        return false;
                    h = rdrRT.Ele(1, 1);
                    if (h == HGTReader.UNDEF)
                        return false;
                    eleArray[3][3] = h;
                }
            }

            // all 16 values present
            return true;
        }


        /**
 * 
 */
        private HGTReader PrepReader(int res, int row, int col)
        {
            if (row >= readers.Length)
            {
                //log.error("invalid array index for row", row);
                return null;
            }
            if (col >= readers[row].Length)
            {
                //log.error("invalid array index for col", row);
                return null;
            }
            HGTReader rdr = readers[row][col];

            if (rdr == null)
            {
                //statRdrNull++;
                return null;
            }

            // do not use if different resolution
            if (res != rdr.GetRes())
            {
                //statRdrRes++;
                return null;
            }

            rdr.PrepRead();
            if (row > lastRow)
                lastRow = row;

            return rdr;
        }


        /**
         * Interpolate the height of point p from the 4 closest values in the hgt matrix.
         * Bilinear interpolation with single node restore
         * @param qx value from 0 .. 1 gives relative x position in matrix 
         * @param qy value from 0 .. 1 gives relative y position in matrix
         * @param hlt height left top
         * @param hrt height right top
         * @param hrb height right bottom
         * @param hlb height left bottom
         * @return the interpolated height
         */

        private short InterpolatedHeight(double qx, double qy, int hlt, int hrt, int hrb, int hlb)
        {
            // extrapolate single node height if requested point is not near
            // for multiple missing nodes, return the height of the neares node
            if (hlb == HGTReader.UNDEF)
            {
                if (hrb == HGTReader.UNDEF || hlt == HGTReader.UNDEF || hrt == HGTReader.UNDEF)
                {
                    if (hrt != HGTReader.UNDEF && hlt != HGTReader.UNDEF && qy > 0.5D)  //top edge
                        return (short)Math.Round((1.0D - qx) * hlt + qx * hrt);
                    if (hrt != HGTReader.UNDEF && hrb != HGTReader.UNDEF && qx > 0.5D)  //right edge
                        return (short)Math.Round((1.0D - qy) * hrb + qy * hrt);
                    //if (hlt != HGTReader.UNDEF && hrb != HGTReader.UNDEF && qx + qy > 0.5D && gx + qy < 1.5D)	//diagonal
                    // nearest value
                    return (short)((qx < 0.5D) ? ((qy < 0.5D) ? hlb : hlt) : ((qy < 0.5D) ? hrb : hrt));
                }
                if (qx + qy < 0.4D) // point is near missing value
                    return HGTReader.UNDEF;
                hlb = hlt + hrb - hrt;
            }
            else if (hrt == HGTReader.UNDEF)
            {
                if (hlb == HGTReader.UNDEF || hrb == HGTReader.UNDEF || hlt == HGTReader.UNDEF)
                {
                    if (hlb != HGTReader.UNDEF && hrb != HGTReader.UNDEF && qy < 0.5D)  //lower edge
                        return (short)Math.Round((1.0D - qx) * hlb + qx * hrb);
                    if (hlb != HGTReader.UNDEF && hlt != HGTReader.UNDEF && qx < 0.5D)  //left edge
                        return (short)Math.Round((1.0D - qy) * hlb + qy * hlt);
                    //if (hlt != HGTReader.UNDEF && hrb != HGTReader.UNDEF && qx + qy > 0.5D && gx + qy < 1.5D)	//diagonal
                    // nearest value
                    return (short)((qx < 0.5D) ? ((qy < 0.5D) ? hlb : hlt) : ((qy < 0.5D) ? hrb : hrt));
                }
                if (qx + qy > 1.6D) // point is near missing value
                    return HGTReader.UNDEF;
                hrt = hlt + hrb - hlb;
            }
            else if (hrb == HGTReader.UNDEF)
            {
                if (hlb == HGTReader.UNDEF || hlt == HGTReader.UNDEF || hrt == HGTReader.UNDEF)
                {
                    if (hlt != HGTReader.UNDEF && hrt != HGTReader.UNDEF && qy > 0.5D)  //top edge
                        return (short)Math.Round((1.0D - qx) * hlt + qx * hrt);
                    if (hlt != HGTReader.UNDEF && hlb != HGTReader.UNDEF && qx < 0.5D)  //left edge
                        return (short)Math.Round((1.0D - qy) * hlb + qy * hlt);
                    //if (hlb != HGTReader.UNDEF && hrt != HGTReader.UNDEF && qy > qx - 0.5D && qy < qx + 0.5D)	//diagonal
                    // nearest value
                    return (short)((qx < 0.5D) ? ((qy < 0.5D) ? hlb : hlt) : ((qy < 0.5D) ? hrb : hrt));
                }
                if (qy < qx - 0.4D) // point is near missing value 
                    return HGTReader.UNDEF;
                hrb = hlb + hrt - hlt;
            }
            else if (hlt == HGTReader.UNDEF)
            {
                if (hlb == HGTReader.UNDEF || hrb == HGTReader.UNDEF || hrt == HGTReader.UNDEF)
                {
                    if (hrb != HGTReader.UNDEF && hlb != HGTReader.UNDEF && qy < 0.5D)  //lower edge
                        return (short)Math.Round((1.0D - qx) * hlb + qx * hrb);
                    if (hrb != HGTReader.UNDEF && hrt != HGTReader.UNDEF && qx > 0.5D)  //right edge
                        return (short)Math.Round((1.0D - qy) * hrb + qy * hrt);
                    //if (hlb != HGTReader.UNDEF && hrt != HGTReader.UNDEF && qy > qx - 0.5D && qy < qx + 0.5D)	//diagonal
                    // nearest value
                    return (short)((qx < 0.5D) ? ((qy < 0.5D) ? hlb : hlt) : ((qy < 0.5D) ? hrb : hrt));
                }
                if (qy > qx + 0.6D) // point is near missing value
                    return HGTReader.UNDEF;
                hlt = hlb + hrt - hrb;
                // bilinera interpolation
            }
            double hxt = (1.0D - qx) * hlt + qx * hrt;
            double hxb = (1.0D - qx) * hlb + qx * hrb;
            return (short)Math.Round((1.0D - qy) * hxb + qy * hxt);

        }

        /**
        * Cubic interpolation for 4 points, taken from http://www.paulinternet.nl/?page=bicubic
        * Uses Catmull–Rom spline.
        * @author Paul Breeuwsma
        */
        private static double CubicInterpolation(double[] p, double qx)
        {
            return p[1] + 0.5 * qx * (p[2] - p[0] + qx * (2.0 * p[0] - 5.0 * p[1] + 4.0 * p[2] - p[3] + qx * (3.0 * (p[1] - p[2]) + p[3] - p[0])));
        }

        /**
         * Bicubic interpolation for 4x4 points, taken from http://www.paulinternet.nl/?page=bicubic
         * @author Paul Breeuwsma
         * @param p 4x4 matrix -1,0,1,2 * -1,0,1,2 with given values  
         * @param qx value from 0 .. 1 gives relative x position in matrix 
         * @param qy value from 0 .. 1 gives relative y position in matrix
         */
        private static double BicubicInterpolation(double[][] p, double qx, double qy)
        {
            double[] arr = new double[4];

            arr[0] = CubicInterpolation(p[0], qy);
            arr[1] = CubicInterpolation(p[1], qy);
            arr[2] = CubicInterpolation(p[2], qy);
            arr[3] = CubicInterpolation(p[3], qy);
            return CubicInterpolation(arr, qx);
        }

    }
}
