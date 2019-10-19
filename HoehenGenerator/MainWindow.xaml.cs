using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;



namespace HoehenGenerator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly XmlDocument ge = new XmlDocument();
        String coordinaten;
        String[] sepcoordinaten;
        GeoPunkt[] geoPunkts;
        GeoPunkt mittelpunkt;
        GeoPunkt linksoben;

        GeoPunkt rechtsoben;
        GeoPunkt linksunten;
        GeoPunkt rechtsunten;
        GeoPunkt hgtlinksunten;
        GeoPunkt hgtrechtsoben;
        PointCollection orgpunkte = new PointCollection();
        PointCollection punkte = new PointCollection();
        Canvas Zeichenfläche = new Canvas();

        ListBox lbHgtFiles = new ListBox();


        List<HGTFile> listHGTFiles = new List<HGTFile>();
        string anlagenname = "neueAnlage";
        string anlagenpfad;
        bool usesrtm = false;

        bool useview = true;

        bool use1zoll = true;

        string hgtPfad;
        double maximaleHöhe = -10000.0;
        double minimaleHöhe = 10000.0;
        bool datumgrenze = false;
        int winkel = 0;
        string[] directorys = { "VIEW1", "VIEW3", "SRTM1", "SRTM3", "noHgt" };
        ConcurrentQueue<AufgabeIndices> aufgabeIndices = new ConcurrentQueue<AufgabeIndices>();
        ConcurrentQueue<LadeDateien> ladeDateiens = new ConcurrentQueue<LadeDateien>();
        ZwischenspeicherHgt ZwspeicherHgt;


        public bool Datumgrenze { get => datumgrenze; set => datumgrenze = value; }

        public MainWindow()
        {
            InitializeComponent();
            Title = "Höhengenerator für EEP";




            Thread thrHoleIndices = new Thread(HoleIndices);
            thrHoleIndices.IsBackground = true;
            thrHoleIndices.Priority = ThreadPriority.Lowest;
            thrHoleIndices.Start();


            Thread thrHoleDateien = new Thread(HoleDateien);
            thrHoleDateien.IsBackground = true;
            thrHoleDateien.Priority = ThreadPriority.Lowest;
            thrHoleDateien.Start();
        }

        private void HoleDateien()
        {
            while (true)
            {
                while (ladeDateiens.Count > 0)
                {
                    bool istArbeitDa = ladeDateiens.TryDequeue(out LadeDateien datei);
                    if (istArbeitDa)
                    {

                        LadeHGTDateien(datei.Url, datei.Zieldatei);
                        unZipHgtFiles(datei.Zieldatei);
                        Dispatcher.BeginInvoke(new Action(() => FärbeHgtLabel(datei.Zieldatei)));
                        //FärbeHgtLabel(datei.Zieldatei);
                        Dispatcher.BeginInvoke(new Action(() => ZeichneAlles(punkte)));
                        //ZeichneAlles(punkte);
                    }
                    if (ladeDateiens.Count == 0)
                    {


                    }

                }
                Thread.Sleep(100);
            }
        }

        private void HoleIndices()
        {
            while (true)
            {

                while (aufgabeIndices.Count > 0)
                {

                    bool istArbeitDa = aufgabeIndices.TryDequeue(out AufgabeIndices aufgabe);
                    if (istArbeitDa)
                    {
                        System.Diagnostics.Debug.Print(aufgabe.Hgtart + " " + aufgabe.Auflösung + " " + aufgabe.Pfad);
                        GeneriereIndices(aufgabe.Hgtart, aufgabe.Auflösung, aufgabe.Pfad);

                    }
                    if (aufgabeIndices.Count == 0)
                    {
                        if (ÜberprüfeIndices())
                            Dispatcher.BeginInvoke(new Action(() => LadeHGTFiles.IsEnabled = true));

                        else
                            Dispatcher.BeginInvoke(new Action(() => LadeHGTFiles.IsEnabled = false));
                    }
                }
                Thread.Sleep(100);

            }
        }

        private void FärbeHgtLabel(string zieldatei)
        {
            SolidColorBrush solidColor = new SolidColorBrush();
            string directory = System.IO.Path.GetDirectoryName(zieldatei);
            for (int i = 0; i < lbHgtFiles.Items.Count; i++)
            {
                if (File.Exists(directory + "\\" + lbHgtFiles.Items[i].ToString() + ".hgt"))

                    if (directory.EndsWith("1"))
                        solidColor = Brushes.LightBlue;
                    else
                        solidColor = Brushes.LightGreen;
                else
                    solidColor = Brushes.Red;
                switch (i)
                {
                    case 0:

                        lbFile1.Background = solidColor;
                        break;
                    case 1:

                        lbFile2.Background = solidColor;
                        break;
                    case 2:

                        lbFile3.Background = solidColor;
                        break;
                    case 3:

                        lbFile4.Background = solidColor;
                        break;

                }


            }

        }

        private void LadeDatei_Click(object sender, RoutedEventArgs e)
        {
            lbHgtFiles = lbHgtFiles1;
            Optimieren.IsEnabled = false;
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Bitte GoogleEarth Datei auswählen",
                Filter = "GoogleEarth Dateien|*.kml;*.kmz;"
            };
            if (ofd.ShowDialog() == true)
            {
                coordinaten = "";
                string vName = ofd.FileName;
                string pfad = System.IO.Path.GetDirectoryName(vName);
                if (!Directory.Exists(pfad + "\\HGT"))
                    try
                    {

                        Directory.CreateDirectory(pfad + "\\HGT");
                    }
                    catch (Exception)
                    {

                        MessageBox.Show("Kann Directory für Hgt-Dateien nicht erstellen!\n"
                            + "Überprüfen Sie die Schreibberechtigung im Verzeichnis:\n"
                            + "\"" + pfad + "\"");
                        throw;
                    }

                hgtPfad = pfad + "\\HGT";
                try
                {
                    FileStream fs = File.Create(hgtPfad + "\\test.txt");
                    fs.Close();
                    
                }
                catch (Exception)
                {
                    MessageBox.Show("Kann im Directory für Hgt-Dateien nicht schreiben!\n"
                            + "Überprüfen Sie die Schreibberechtigung im Verzeichnis:\n"
                            + "\"" + pfad + "\"");
                    throw;
                }
                File.Delete(hgtPfad + "\\test.txt");
                
                if (vName.EndsWith(".kmz", StringComparison.OrdinalIgnoreCase))
                {
                    ZipArchive archive = ZipFile.OpenRead(vName);
                    StreamReader p = new StreamReader(archive.Entries[0].Open());
                    ge.Load(p);
                    p.Close();


                    archive.Dispose();

                }
                else
                {

                    ge.Load(vName);
                }
            }
            else
            {
                return;
            }
            SuchenNode(ge);
            if (coordinaten.Length > 0)
            {
                //MessageBox.Show(coordinaten);
                SepariereKoordinaten(coordinaten);
                BildeSchattenpunkte(orgpunkte);
                punkte = orgpunkte;
                Zeichenfläche = Zeichenfläche1;

                //HGTFiles = HGTFiles1;
                // Optimiere(orgpunkte);
                ZeichneAlles(punkte);
                hgtlinksunten = linksunten;
                hgtrechtsoben = rechtsoben;
                //ZeichneRechteck(punkte);
                //ZeichnePolygon(punkte);
                //ZeichnePunkte(punkte);
                if (lbHgtFiles.Items.Count <= 4)
                {
                    Optimieren.IsEnabled = true;
                    Weiter.IsEnabled = true;
                    Drehen.IsEnabled = true;
                    generiereDirString();
                }
                else
                {
                    Optimieren.IsEnabled = false;
                    Weiter.IsEnabled = false;
                    Drehen.IsEnabled = false;

                    MessageBox.Show("Die Fläche ist zu groß! Es wurden "
                                           + lbHgtFiles.Items.Count
                                           + " Hgt-Files ermittelt!\nMaximal möglich sind 4 Hgt-Files!"
                                           + " Bitte eine kleinere Fläche auswählen!");

                }
                hgtlinksunten = linksunten;
                hgtrechtsoben = rechtsoben;


            }

        }

        private PointCollection BildeSchattenpunkte(PointCollection orgpunkte)
        {

            return orgpunkte;

        }

        private void Optimiere(PointCollection orgpunkte)
        {
            NeuPunkte neuPunkte;
            int anzahl = 180;
            double fläche = 0;
            // int winkel = 0;


            for (int i = 0; i < anzahl; i++)
            {
                neuPunkte = DrehePolygon(orgpunkte, i - (anzahl / 2));
                //ZeichneAlles(neuPunkte.Punkte);
                if (fläche == 0 || fläche > neuPunkte.Fläche)
                {
                    winkel = i - (anzahl / 2);
                    fläche = neuPunkte.Fläche;
                }


            }

            neuPunkte = DrehePolygon(orgpunkte, winkel);
            punkte = neuPunkte.Punkte;
            Drehen.IsEnabled = true;
            Weiter.IsEnabled = true;

        }

        private NeuPunkte DrehePolygon(PointCollection orgpunkte, double v)
        {
            PointCollection points = new PointCollection();
            Point point = new Point();

            for (int i = 0; i < orgpunkte.Count; i++)
            {

                Matrix drehung = BildeDrehungsMatrix(mittelpunkt.Lon, mittelpunkt.Lat, v);
                point = DrehePunkt(orgpunkte[i], drehung);
                if (Datumgrenze && point.X < 0)
                    point.X = point.X + 360;
                points.Add(point);
            }
            double minLänge = points.Min(x => x.X);
            double minBreite = points.Min(x => x.Y);
            double maxLänge = points.Max(x => x.X);
            double maxBreite = points.Max(x => x.Y);
            GeoPunkt linksoben = new GeoPunkt(minLänge, maxBreite);
            GeoPunkt rechtsoben = new GeoPunkt(maxLänge, maxBreite);
            GeoPunkt linksunten = new GeoPunkt(minLänge, minBreite);
            GeoPunkt rechtsunten = new GeoPunkt(maxLänge, minBreite);
            double hoehe2 = GeoPunkt.BestimmeAbstand(linksoben, linksunten);
            double breite2 = GeoPunkt.BestimmeAbstand(linksunten, rechtsunten);

            double fläche = hoehe2 * breite2;
            NeuPunkte neuPunkte = new NeuPunkte(points, fläche);
            return neuPunkte;
        }

        private static Matrix BildeDrehungsMatrix(double alpha, double beta, double phi)
        {
            // alph = Lat
            // beta = Lon
            //double[] Ri1 = new double[4] { 1, 0, 0, 0 };
            //double[] Ri2 = new double[4] { 0, 1, 0, 0 };
            //double[] Ri3 = new double[4] { 0, 0, 1, 0 };
            //double[] Ri4 = new double[4] { 0, 0, 0, 1 };

            double cosalpha = Math.Cos(GeoPunkt.bogen(alpha));
            double sinalpha = Math.Sin(GeoPunkt.bogen(alpha));
            double cosbeta = Math.Cos(GeoPunkt.bogen(beta));
            double sinbeta = Math.Sin(GeoPunkt.bogen(beta));
            double cosphi = Math.Cos(GeoPunkt.bogen(phi));
            double sinphi = Math.Sin(GeoPunkt.bogen(phi));

            Matrix R1 = new Matrix(4, 4);
            Matrix R2 = new Matrix(4, 4);
            Matrix R3 = new Matrix(4, 4);
            Matrix R4 = new Matrix(4, 4);
            Matrix R5 = new Matrix(4, 4);
            R1.SetColumn(0, new double[4] { cosalpha, 0, -sinalpha, 0 });
            R1.SetColumn(1, new double[4] { 0, 1, 0, 0 });
            R1.SetColumn(2, new double[4] { sinalpha, 0, cosalpha, 0 });
            R1.SetColumn(3, new double[4] { 0, 0, 0, 1 });

            R2.SetColumn(0, new double[4] { 1, 0, 0, 0 });
            R2.SetColumn(1, new double[4] { 0, cosbeta, -sinbeta, 0 });
            R2.SetColumn(2, new double[4] { 0, sinbeta, cosbeta, 0 });
            R2.SetColumn(3, new double[4] { 0, 0, 0, 1 });

            R3.SetColumn(0, new double[4] { cosphi, -sinphi, 0, 0 });
            R3.SetColumn(1, new double[4] { sinphi, cosphi, 0, 0 });
            R3.SetColumn(2, new double[4] { 0, 0, 1, 0 });
            R3.SetColumn(3, new double[4] { 0, 0, 0, 1 });

            R4.SetColumn(0, new double[4] { 1, 0, 0, 0 });
            R4.SetColumn(1, new double[4] { 0, cosbeta, sinbeta, 0 });
            R4.SetColumn(2, new double[4] { 0, -sinbeta, cosbeta, 0 });
            R4.SetColumn(3, new double[4] { 0, 0, 0, 1 });

            R5.SetColumn(0, new double[4] { cosalpha, 0, sinalpha, 0 });
            R5.SetColumn(1, new double[4] { 0, 1, 0, 0 });
            R5.SetColumn(2, new double[4] { -sinalpha, 0, cosalpha, 0 });
            R5.SetColumn(3, new double[4] { 0, 0, 0, 1 });

            //Matrix E = R5 * R4 * R3 * R2 * R1;
            return R1 * R2 * R3 * R4 * R5;


            //throw new NotImplementedException();
        }

        private static Point DrehePunkt(Point point, Matrix drehung)
        {

            GeoPunkt geoPunkt = new GeoPunkt(point.X, point.Y);
            Matrix P1 = new Matrix(4, 1);
            GeoPunkt geoPunkt1 = new GeoPunkt();


            P1.SetColumn(0, new double[4] { geoPunkt.Ygeo, geoPunkt.Zgeo, geoPunkt.Xgeo, 1.0 });
            //R1.SetColumn(0,[Math.Cos(gradrad),0, Math.Sin(gradrad), 0]);
            Matrix E = drehung * P1;
            double[] point2 = E.GetColumn(0);
            geoPunkt1.FügeGeopunktEin(point2[2], point2[0], point2[1]);
            Point point1 = new Point(geoPunkt1.Lon, geoPunkt1.Lat);

            //point1.X = (Math.Cos(vrad) * (point.X - mittelpunkt.Lon)) - (Math.Sin(vrad) * (point.Y - mittelpunkt.Lat)) + mittelpunkt.Lon;
            //point1.Y = (Math.Sin(vrad) * (point.X - mittelpunkt.Lon)) + (Math.Cos(vrad) * (point.Y - mittelpunkt.Lat)) + mittelpunkt.Lat;

            return point1;
        }
        private static GeoPunkt DrehePunkt(GeoPunkt geoPunkt, Matrix drehung)
        {

            //GeoPunkt geoPunkt = new GeoPunkt(point.X, point.Y);
            Matrix P1 = new Matrix(4, 1);
            GeoPunkt geoPunkt1 = new GeoPunkt();


            P1.SetColumn(0, new double[4] { geoPunkt.Ygeo, geoPunkt.Zgeo, geoPunkt.Xgeo, 1.0 });
            //R1.SetColumn(0,[Math.Cos(gradrad),0, Math.Sin(gradrad), 0]);
            Matrix E = drehung * P1;
            double[] point2 = E.GetColumn(0);
            geoPunkt1.FügeGeopunktEin(point2[2], point2[0], point2[1]);
            //Point point1 = new Point(geoPunkt1.Lon, geoPunkt1.Lat);

            //point1.X = (Math.Cos(vrad) * (point.X - mittelpunkt.Lon)) - (Math.Sin(vrad) * (point.Y - mittelpunkt.Lat)) + mittelpunkt.Lon;
            //point1.Y = (Math.Sin(vrad) * (point.X - mittelpunkt.Lon)) + (Math.Cos(vrad) * (point.Y - mittelpunkt.Lat)) + mittelpunkt.Lat;

            return geoPunkt1;
        }

        private void ZeichneRechteck(PointCollection punkte)
        {
            Polyline rechteckpunkte = new Polyline();
            double Größe, GrößeH, GrößeB, hoehe2, breite2, minLänge, maxLänge, minBreite, maxBreite;
            AnzeigeFlächeBerechnen(punkte, out GrößeH, out GrößeB, out hoehe2, out breite2, out minLänge, out maxLänge, out minBreite, out maxBreite, out Größe);
            double flaeche2 = hoehe2 * breite2;
            fläche.Text = Math.Round(flaeche2).ToString() + " km²";
            höhe.Text = Math.Round(hoehe2).ToString() + " km";
            breite.Text = Math.Round(breite2).ToString() + " km";
            Zeichenfläche.Children.Clear();
            PointCollection canvasrechteckpunkte = new PointCollection();
            canvasrechteckpunkte.Add(new Point(GrößeB, -1 * GrößeH));
            canvasrechteckpunkte.Add(new Point(0, -1 * GrößeH));
            canvasrechteckpunkte.Add(new Point(0, -1 * 0));
            canvasrechteckpunkte.Add(new Point(GrößeB, -1 * 0));
            canvasrechteckpunkte.Add(new Point(GrößeB, -1 * GrößeH));
            rechteckpunkte.Points = canvasrechteckpunkte;
            rechteckpunkte.Fill = Brushes.Blue;
            Zeichenfläche.Children.Add(rechteckpunkte);
            Canvas.SetLeft(rechteckpunkte, 0);
            Canvas.SetBottom(rechteckpunkte, 0);


        }

        private void AnzeigeFlächeBerechnen(PointCollection punkte, out double GrößeH, out double GrößeB, out double hoehe2, out double breite2, out double minLänge,
            out double minBreite, out double maxLänge, out double maxBreite, out double Größe)
        {
            Größe = Zeichenfläche.ActualHeight;
            if (Zeichenfläche.ActualWidth < Zeichenfläche.ActualHeight)
            {
                Größe = Zeichenfläche.ActualWidth;
            }
            GrößeH = Zeichenfläche.ActualHeight;
            GrößeB = Zeichenfläche.ActualWidth;
            minLänge = punkte.Min(x => x.X);
            minBreite = punkte.Min(x => x.Y);
            maxLänge = punkte.Max(x => x.X);
            maxBreite = punkte.Max(x => x.Y);
            linksoben = new GeoPunkt(minLänge, maxBreite);
            rechtsoben = new GeoPunkt(maxLänge, maxBreite);
            linksunten = new GeoPunkt(minLänge, minBreite);
            rechtsunten = new GeoPunkt(maxLänge, minBreite);

            hoehe2 = GeoPunkt.BestimmeAbstand(linksoben, linksunten);
            breite2 = GeoPunkt.BestimmeAbstand(linksunten, rechtsunten);
            if (hoehe2 / breite2 > GrößeH / GrößeB)
            {

                GrößeB = GrößeB * GrößeH / GrößeB * breite2 / hoehe2;
            }
            else
            {

                GrößeH = GrößeH * GrößeB / GrößeH * hoehe2 / breite2;
            }
            //if (Optimieren.IsEnabled == false)
            GibHGTFileaus(linksoben, rechtsoben, linksunten, rechtsunten);
        }

        private void GibHGTFileaus(GeoPunkt linksoben, GeoPunkt rechtsoben, GeoPunkt linksunten, GeoPunkt rechtsunten)
        {
            double winkel2 = -winkel;
            PointCollection points = new PointCollection();
            PointCollection points1 = new PointCollection();
            points.Clear();
            points1.Clear();
            points.Add(new Point(linksoben.Lon, linksoben.Lat));
            points.Add(new Point(rechtsoben.Lon, rechtsoben.Lat));
            points.Add(new Point(linksunten.Lon, linksunten.Lat));
            points.Add(new Point(rechtsunten.Lon, rechtsunten.Lat));
            Matrix drehung = BildeDrehungsMatrix(mittelpunkt.Lon, mittelpunkt.Lat, winkel2);
            for (int i = 0; i < points.Count; i++)
            {
                points1.Add(DrehePunkt(points[i], drehung));
            }

            double maxlat = Math.Round(points1.Max(x => x.Y) - 0.5);
            double minlat = Math.Round(points1.Min(x => x.Y) - 0.5);
            double maxlon = Math.Round(points1.Max(x => x.X) - 0.5);
            double minlon = Math.Round(points1.Min(x => x.X) - 0.5);
            lbHgtFiles.Items.Clear();
            // HGTFiles.Text = "";
            if (maxlon - minlon > 180)
            {
                BildeHGTString(maxlat, minlat, 180, maxlon);
                BildeHGTString(maxlat, minlat, minlon, -180);
            }
            else
            {
                BildeHGTString(maxlat, minlat, maxlon, minlon);

            }
            //HGTFiles.Background = Brushes.Red;
            //string[] vs = HGTFiles.Text.Split('\n');
            string[] vs1 = new string[lbHgtFiles.Items.Count];
            bool[] vs2 = new bool[lbHgtFiles.Items.Count];
            for (int i = 0; i < lbHgtFiles.Items.Count; i++)
            {
                vs1[i] = lbHgtFiles.Items[i].ToString();
                vs2[i] = false;
            }


            for (int i = 0; i < vs1.Length; i++)
            {

                if (hgtPfad != null)
                {
                    bool DateiVorhanden = false;
                    foreach (var directory in directorys)
                    {
                        if (directory.Length > 0)
                            if (!DateiVorhanden)
                            {
                                if (File.Exists(hgtPfad + "\\" + directory + "\\" + vs1[i] + ".hgt"))
                                {
                                    DateiVorhanden = true;
                                    vs2[i] = true;
                                    FärbeHgtLabel(hgtPfad + "\\" + directory + "\\" + vs1[i] + ".hgt");
                                }

                            }
                        if (!DateiVorhanden)
                            FärbeHgtLabel(hgtPfad + "\\" + directory + "\\" + vs1[i] + ".hgt");
                    }
                }

            }
            bool janein = true;
            for (int i = 0; i < vs2.Length; i++)
            {
                if (vs2[i] == false)
                    janein = false;
            }
            if (janein)
            {
                //HGTFiles.Background = Brushes.LightGreen;
                Weiter2.IsEnabled = true;
            }


        }

        private void BildeHGTString(double maxlat, double minlat, double maxlon, double minlon)
        {
            string hgt;
            for (int i = (int)minlat; i < (int)maxlat + 1; i++)
            {
                for (int j = (int)minlon; j < (int)maxlon + 1; j++)
                {

                    if (i >= 0)
                    {
                        hgt = "N" + i.ToString("D2");
                    }
                    else
                    {
                        hgt = "S" + (-i).ToString("D2");
                    }
                    if (j >= 0)
                    {
                        hgt = hgt + "E" + j.ToString("D3");
                    }
                    else
                    {
                        hgt = hgt + "W" + (-j).ToString("D3");
                    }
                    lbHgtFiles.Items.Add(hgt);
                    // hgt = hgt + "\n";
                    // HGTFiles.Text = HGTFiles.Text + hgt;

                }
            }

            return;
        }


        private void ZeichnePolygon(PointCollection punkte, bool ishgtwert = false)
        {


            Polyline polypunkte = new Polyline();
            double Größe, GrößeH, GrößeB, hoehe2, breite2, minLänge, maxLänge, minBreite, maxBreite;
            AnzeigeFlächeBerechnen(punkte, out GrößeH, out GrößeB, out hoehe2, out breite2, out minLänge, out minBreite, out maxLänge, out maxBreite, out Größe);
            double flaeche2 = hoehe2 * breite2;
            PointCollection canvaspunkte = new PointCollection();
            //Zeichenfläche.Children.Clear();
            for (int i = 0; i < punkte.Count; i++)
            {
                canvaspunkte.Add(new Point(GrößeB / (maxLänge - minLänge) * (punkte[i].X - minLänge), -1 * GrößeH / (maxBreite - minBreite) * (punkte[i].Y - minBreite)));
            }
            polypunkte.Points = canvaspunkte;
            polypunkte.Fill = Brushes.Green;
            if (ishgtwert)
                polypunkte.Fill = Brushes.Yellow;
            Zeichenfläche.Children.Add(polypunkte);
            Canvas.SetLeft(polypunkte, 0);
            Canvas.SetBottom(polypunkte, 0);

        }

        private void ZeichnePunkte(PointCollection punkte)
        {

            double Größe, GrößeH, GrößeB, hoehe2, breite2, minLänge, maxLänge, minBreite, maxBreite;
            AnzeigeFlächeBerechnen(punkte, out GrößeH, out GrößeB, out hoehe2, out breite2, out minLänge, out minBreite, out maxLänge, out maxBreite, out Größe);
            for (int i = 0; i < punkte.Count; i++)
            {
                Ellipse elli = new Ellipse();
                elli.Width = 5.0;
                elli.Height = 5.0;
                elli.Fill = Brushes.Red;
                Zeichenfläche.Children.Add(elli);

                Canvas.SetLeft(elli, GrößeB / (maxLänge - minLänge) * (punkte[i].X - minLänge) - 2.5);
                Canvas.SetBottom(elli, GrößeH / (maxBreite - minBreite) * (punkte[i].Y - minBreite) - 2.5);
            }
            Ellipse elli2 = new Ellipse();
            elli2.Width = 5.0;
            elli2.Height = 5.0;
            elli2.Fill = Brushes.Red;
            Zeichenfläche.Children.Add(elli2);

            Canvas.SetLeft(elli2, GrößeB / (maxLänge - minLänge) * ((maxLänge - minLänge) / 2) - 2.5);
            Canvas.SetBottom(elli2, GrößeH / (maxBreite - minBreite) * ((maxBreite - minBreite) / 2) - 2.5);

        }
        private void ZeichnePunkte(List<GeoPunkt> punkte)
        {

            double Größe, GrößeH, GrößeB, hoehe2, breite2, minLänge, maxLänge, minBreite, maxBreite;
            AnzeigeFlächeBerechnen(out GrößeH, out GrößeB, out hoehe2, out breite2, out minLänge, out minBreite, out maxLänge, out maxBreite, out Größe);
            //double punktgröße = 4 * GrößeH * GrößeB / punkte.Count;
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();

            //minimaleHöhe = punkte.Min(x => x.Höhe);
            //maximaleHöhe = punkte.Max(x => x.Höhe);
            double punktgröße = 1;
            for (int i = 0; i < punkte.Count; i += 10)
            {
                int Lon = (int)(GrößeB / (maxLänge - minLänge) * (punkte[i].Lon - minLänge));
                int Lat = (int)(GrößeH / (maxBreite - minBreite) * (punkte[i].Lat - minBreite));


                if (Lat > 0 && Lat < GrößeH && Lon > 0 && Lon < GrößeB)
                {
                    //Ellipse elli = new Ellipse();

                    //int höhe = punkte[i].Höhe * 100 + 1000;

                    //int r1 = höhe % 256;
                    //int g1 = (höhe / 256) % 256;
                    //int b1 = (höhe / 256 / 256) % 256;
                    //byte r = (byte)r1;
                    //byte g = (byte)g1; ;
                    //byte b = (byte)b1; ;
                    //mySolidColorBrush.Color = Color.FromRgb(r, g, b);
                    //elli.Width = punktgröße;
                    //elli.Height = punktgröße;
                    //elli.Fill = mySolidColorBrush;
                    //Zeichenfläche.Children.Add(elli);

                    //Canvas.SetLeft(elli, Lon - punktgröße / 2);
                    //Canvas.SetTop(elli, Lat - punktgröße / 2);

                }
            }




        }

        private void AnzeigeFlächeBerechnen(out double GrößeH, out double GrößeB, out double hoehe2, out double breite2, out double minLänge, out double minBreite, out double maxLänge, out double maxBreite, out double Größe)
        {
            Größe = Zeichenfläche.ActualHeight;
            if (Zeichenfläche.ActualWidth < Zeichenfläche.ActualHeight)
            {
                Größe = Zeichenfläche.ActualWidth;
            }
            GrößeH = Zeichenfläche.ActualHeight;
            GrößeB = Zeichenfläche.ActualWidth;
            minLänge = Math.Min(linksoben.Lon, linksunten.Lon);
            minBreite = Math.Min(linksunten.Lat, rechtsunten.Lat);
            maxLänge = Math.Max(rechtsoben.Lon, rechtsunten.Lon);
            maxBreite = Math.Max(linksoben.Lat, rechtsoben.Lat);

            hoehe2 = GeoPunkt.BestimmeAbstand(linksoben, linksunten);
            breite2 = GeoPunkt.BestimmeAbstand(linksunten, rechtsunten);
            if (hoehe2 / breite2 > GrößeH / GrößeB)
            {

                GrößeB = GrößeB * GrößeH / GrößeB * breite2 / hoehe2;
            }
            else
            {

                GrößeH = GrößeH * GrößeB / GrößeH * hoehe2 / breite2;
            }

        }

        private void SepariereKoordinaten(string coordinaten)
        {
            //GeoPunkt[] geoPunkts = new GeoPunkt();

            sepcoordinaten = coordinaten.Split(' ');
            geoPunkts = new GeoPunkt[sepcoordinaten.Length];
            orgpunkte.Clear();
            Datumgrenze = false;
            for (int i = 0; i < sepcoordinaten.Length; i++)
            {
                string[] einekoordinate = sepcoordinaten[i].Split(',');
                //CultureInfo culture = new CultureInfo("en-US");

                //Point einpunkt = new Point(double.Parse(einekoordinate[0], culture), double.Parse(einekoordinate[1], culture));
                orgpunkte.Add(new Point(double.Parse(einekoordinate[0], CultureInfo.InvariantCulture), double.Parse(einekoordinate[1], CultureInfo.InvariantCulture)));
                geoPunkts[i] = new GeoPunkt(double.Parse(einekoordinate[0], CultureInfo.InvariantCulture), double.Parse(einekoordinate[1], CultureInfo.InvariantCulture));
                if (geoPunkts[i].Lon > 180 || geoPunkts[i].Lon < -180)
                    Datumgrenze = true;

                if (i > 0)
                {
                    geoPunkts[i].Entfernung = GeoPunkt.BestimmeAbstand(geoPunkts[i], geoPunkts[i - 1]);
                }
            }
            mittelpunkt = new GeoPunkt(((orgpunkte.Max(x => x.X) - orgpunkte.Min(x => x.X)) / 2.0 + orgpunkte.Min(x => x.X)), ((orgpunkte.Max(x => x.Y) - orgpunkte.Min(x => x.Y)) / 2.0 + orgpunkte.Min(x => x.Y)));
        }

        private void SuchenNode(XmlNode ge)
        {
            // TODO verfeinern, dass wirklich nur die Koordinaten Linie oder Fläche
            if (!ge.HasChildNodes)
            {
                return;
            }
            for (int i = 0; i < ge.ChildNodes.Count; i++)
            {
                if (ge.ChildNodes[i].Name == "coordinates")
                {
                    coordinaten = ge.ChildNodes[i].InnerText;
                }
                SuchenNode(ge.ChildNodes[i]);
                char[] charsToTrim = { '\n', ' ', '\t' };
                coordinaten = coordinaten.Trim(charsToTrim);
            }
        }


        private void Zeichenfläche_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (punkte.Count > 0)
            {
                //ZeichneRechteck(punkte);
                //ZeichnePolygon(punkte);
                //ZeichnePunkte(punkte); 
                ZeichneAlles(punkte);
            }


        }

        private void ZeichneAlles(PointCollection punkte)
        {
            ZeichneRechteck(punkte);
            ZeichnePolygon(punkte);
            ZeichnePunkte(punkte);
        }

        private void Optimieren_Click(object sender, RoutedEventArgs e)
        {
            Optimiere(orgpunkte);
            ZeichneAlles(punkte);


        }

        private void Drehen_Click(object sender, RoutedEventArgs e)
        {
            winkel = winkel + 90;
            NeuPunkte neuPunkte = DrehePolygon(orgpunkte, winkel);
            punkte = neuPunkte.Punkte;
            ZeichneAlles(punkte);
        }

        private void Weiter_Click(object sender, RoutedEventArgs e)
        {
            ladeHGTFiles.IsEnabled = true;
            ladeHGTFiles.IsSelected = true;
            Zeichenfläche = Zeichenfläche2;
            //   HGTFiles = HGTFiles2;
            if (usesrtm == true)
                SRTM.IsChecked = true;
            else
                SRTM.IsChecked = false;
            if (useview == true)
                VIEW.IsChecked = true;
            else
                VIEW.IsChecked = false;
            if (use1zoll == true)
                einZoll.IsChecked = true;
            else
                einZoll.IsChecked = false;



            ZeichneAlles(punkte);
        }

        private void ladenTab_GotFocus(object sender, RoutedEventArgs e)
        {
            Zeichenfläche = Zeichenfläche1;

            Hauptfenster.ResizeMode = ResizeMode.CanResize;
        }

        private void ladeHGTFiles_GotFocus(object sender, RoutedEventArgs e)
        {

            btnIndex.IsEnabled = true;
            ZeichneAlles(punkte);
            if (ÜberprüfeIndices())
                LadeHGTFiles.IsEnabled = true;
            else
                LadeHGTFiles.IsEnabled = false;
            Zeichenfläche = Zeichenfläche2;

            Hauptfenster.ResizeMode = ResizeMode.CanResize;
            int anzahl = lbHgtFiles.Items.Count;
            if (anzahl > 0)
                lbFile1.Content = lbHgtFiles.Items[0].ToString();
            else
                lbFile1.Content = "";
            if (anzahl > 1)
                lbFile2.Content = lbHgtFiles.Items[1].ToString();
            else
                lbFile2.Content = "";
            if (anzahl > 2)
                lbFile3.Content = lbHgtFiles.Items[2].ToString();
            else
                lbFile3.Content = "";
            if (anzahl > 3)
                lbFile4.Content = lbHgtFiles.Items[3].ToString();
            else
                lbFile4.Content = "";
            lb3Zoll.Content = "3\"";
            if (use1zoll)
                lb1Zoll.Content = "1\"";
            else
                lb1Zoll.Content = "";


        }



        private bool ÜberprüfeIndices()
        {
            bool vorhanden = true;
            if (use1zoll)
            {
                if (useview)
                    if (!ÜberprüfeViewIndex(1, hgtPfad))
                        vorhanden = false;
                if (usesrtm)
                    if (!ÜberprüfeSRTMIndex(1, hgtPfad))
                        vorhanden = false;

            }

            if (useview)
                if (!ÜberprüfeViewIndex(3, hgtPfad))
                    vorhanden = false;
            if (usesrtm)
                if (!ÜberprüfeSRTMIndex(3, hgtPfad))
                    vorhanden = false;
            return vorhanden;
        }

        private bool ÜberprüfeSRTMIndex(int v, string hgtPfad)
        {
            if (File.Exists(hgtPfad + @"\srtmindex" + v + ".xml"))
                return true;
            else
                return false;
        }

        private bool ÜberprüfeViewIndex(int v, string hgtPfad)
        {
            if (File.Exists(hgtPfad + @"\viewindex" + v + ".xml"))
                return true;
            else
                return false;


        }

        private void LadeHGTFiles_Click(object sender, RoutedEventArgs e)
        {



            downloadeHgtFiles();



        }
        private void unZipHgtFiles(string zieldatei)
        {
            string pfad = System.IO.Path.GetDirectoryName(zieldatei);
            ZipArchive zipfile = ZipFile.OpenRead(zieldatei);
            foreach (ZipArchiveEntry entry in zipfile.Entries)
            {
                if (entry.Name.Length > 0)
                    entry.ExtractToFile(pfad + "\\" + entry.Name, overwrite: true);
                //MessageBox.Show("ZipFile:" + file + " gefunden!\n" + "Datei: " + entry.Name);


            }
            zipfile.Dispose();
            File.Delete(zieldatei);


        }

        private void unZipHgtFiles()
        {


            foreach (var item in directorys)
            {
                if (item.Length > 0)
                    if (Directory.Exists(hgtPfad + "\\" + item))
                    {
                        string[] zipfiles = Directory.GetFiles(hgtPfad + "\\" + item, "*.zip");
                        foreach (var file in zipfiles)
                        {
                            ZipArchive zipfile = ZipFile.OpenRead(file);
                            foreach (ZipArchiveEntry entry in zipfile.Entries)
                            {
                                if (entry.Name.Length > 0)
                                    entry.ExtractToFile(hgtPfad + "\\" + item + "\\" + entry.Name, overwrite: true);
                                //MessageBox.Show("ZipFile:" + file + " gefunden!\n" + "Datei: " + entry.Name);


                            }
                            zipfile.Dispose();

                            File.Delete(file);


                        }


                    }



            }


        }

        private void downloadeHgtFiles()
        {
            // List<string> vs1 = new List<string>();
            List<string> srtm1 = new List<string>();
            List<string> srtm3 = new List<string>();
            List<string> view1 = new List<string>();
            List<string> view3 = new List<string>();
            //string[] vs = HGTFiles.Text.Split('\n');
            if (Directory.Exists(hgtPfad + "\\noHgt"))
            {
                string[] zulöschen = Directory.GetFiles(hgtPfad + "\\noHgt");
                foreach (var item in zulöschen)
                {
                    File.Delete(item);
                }
            }
            for (int i = 0; i < lbHgtFiles.Items.Count; i++)
            {
                string file = lbHgtFiles.Items[i].ToString();
                if (file.Length > 0)
                {
                    string[] url = findeUrl(file);
                    //   vs1.Clear();
                    if (url.Length == 0)
                    {
                        //MessageBox.Show("Datei: " + vs[i] + "nicht existent!");
                        if (!Directory.Exists(hgtPfad + "\\noHgt"))
                            Directory.CreateDirectory(hgtPfad + "\\noHgt");
                        StreamWriter sw = File.CreateText(hgtPfad + "\\noHgt\\" + file + ".hgt");
                        sw.Close();



                    }
                    else
                    {
                        for (int j = 0; j < url.Length; j++)
                        {
                            //  vs1.Add(url[j]);
                            if (url[j].Contains("SRTM1") && !File.Exists(hgtPfad + "\\SRTM1\\" + file + ".hgt"))
                                srtm1.Add(url[j]);
                            if (url[j].Contains("SRTM3") && !File.Exists(hgtPfad + "\\SRTM3\\" + file + ".hgt"))
                                srtm3.Add(url[j]);
                            if (url[j].Contains("dem1") && !File.Exists(hgtPfad + "\\VIEW1\\" + file + ".hgt"))
                                view1.Add(url[j]);
                            if (url[j].Contains("dem3") && !File.Exists(hgtPfad + "\\VIEW3\\" + file + ".hgt"))
                                view3.Add(url[j]);
                        }

                    }

                }

            }

            for (int i = 0; i < srtm1.Count; i++)
            {
                string dateiname = System.IO.Path.GetFileName(srtm1[i]);
                String Zielname = hgtPfad + "\\SRTM1\\" + dateiname;
                if (!File.Exists(Zielname))
                    ladeDateiens.Enqueue(new LadeDateien(srtm1[i], Zielname));
                // LadeHGTFiles(srtm1[i], Zielname);
                //  webClient.DownloadFile(srtm1[i], Zielname);
            }
            for (int i = 0; i < srtm3.Count; i++)
            {
                string dateiname = System.IO.Path.GetFileName(srtm3[i]);
                String Zielname = hgtPfad + "\\SRTM3\\" + dateiname;
                if (!File.Exists(Zielname))
                    ladeDateiens.Enqueue(new LadeDateien(srtm3[i], Zielname));
                // webClient.DownloadFile(srtm3[i], Zielname);
            }
            for (int i = 0; i < view1.Count; i++)
            {
                string dateiname = System.IO.Path.GetFileName(view1[i]);
                String Zielname = hgtPfad + "\\VIEW1\\" + dateiname;
                if (!File.Exists(Zielname))
                    ladeDateiens.Enqueue(new LadeDateien(view1[i], Zielname));
                // webClient.DownloadFile(view1[i], Zielname);
            }
            for (int i = 0; i < view3.Count; i++)
            {
                string dateiname = System.IO.Path.GetFileName(view3[i]);
                String Zielname = hgtPfad + "\\VIEW3\\" + dateiname;
                if (!File.Exists(Zielname))
                    ladeDateiens.Enqueue(new LadeDateien(view3[i], Zielname));
                //webClient.DownloadFile(view3[i], Zielname);
            }



        }

        private void LadeHGTDateien(string v, string zielname)
        {
            WebClient webClient = new WebClient() // TODO: Ausnahmen beim Internetzugriff "Kann Datei nicht laden o.ä.
            {
                Encoding = Encoding.UTF8
            };
            try
            {
                webClient.DownloadFile(v, zielname);
            }
            catch (Exception)
            {

                MessageBox.Show("Fehler! Kann Datei: " + v +
                   " nicht downloaden!\nBitte überprüfen Sie Ihre Internezverbindung");
            }

            webClient.Dispose();
        }

        private string[] findeUrl(string item)
        {
            List<string> vs = new List<string>();
            for (int i = 1; i < 4; i += 2)
            {
                string view = DurchsucheIndex(i, hgtPfad, item, "view");
                string srtm = DurchsucheIndex(i, hgtPfad, item, "srtm");
                for (int j = 0; j < view.Length; j++)
                {
                    vs.Add(view);
                }
                for (int j = 0; j < srtm.Length; j++)
                {
                    vs.Add(srtm);
                }
            }
            List<string> vs1 = new List<string>();
            for (int i = 0; i < vs.Count; i++)
            {
                if (!vs1.Contains<string>(vs[i]))
                    vs1.Add(vs[i]);
            }
            string[] vs2 = new string[vs1.Count];
            for (int i = 0; i < vs1.Count; i++)
            {
                vs2[i] = vs1[i];
            }
            return vs2;
        }

        private string DurchsucheIndex(int i, string hgtPfad, string item, string v)
        {
            string ergebnis = "";
            string ersterTeil = "";
            string zweiterTeil = "";
            string knoten = "";
            if (File.Exists(hgtPfad + "\\" + v + "index" + i + ".xml"))
            {
                XmlReader xmlReader = XmlReader.Create(hgtPfad + "\\" + v + "index" + i + ".xml");
                while (xmlReader.Read())
                {
                    switch (xmlReader.NodeType)
                    {

                        case XmlNodeType.Element:
                            if (!xmlReader.IsEmptyElement)
                            {

                                if (xmlReader.Name == "Abschnitt")
                                    knoten = xmlReader.Name;
                                if (xmlReader.Name == "ZipDatei")
                                    knoten = xmlReader.Name;

                                if (xmlReader.HasAttributes)
                                {
                                    // Durch die Attribute navigieren
                                    while (xmlReader.MoveToNextAttribute())
                                    {

                                        if (knoten == "Abschnitt" && xmlReader.Name == "Url")
                                            ersterTeil = xmlReader.Value;
                                        if (knoten == "ZipDatei" && (xmlReader.Name == "Dateiname" || xmlReader.Name == "Url"))
                                            zweiterTeil = xmlReader.Value;


                                    }
                                }
                            }

                            else
                            { }
                            break;

                        case XmlNodeType.Text:
                            if (xmlReader.Value == item)
                                ergebnis = ersterTeil + zweiterTeil;

                            break;
                        default:
                            break;

                    }
                }
            }
            return ergebnis;
        }

        private void GeneriereIndices()
        {

            if (use1zoll)
            {
                if (useview)
                    aufgabeIndices.Enqueue(new AufgabeIndices("view", 1, hgtPfad));
                //    GeneriereIndices("view",1, hgtPfad);
                if (usesrtm)
                    aufgabeIndices.Enqueue(new AufgabeIndices("srtm", 1, hgtPfad));
                //  GeneriereIndices("srtm", 1, hgtPfad);

            }

            if (useview)
                aufgabeIndices.Enqueue(new AufgabeIndices("view", 3, hgtPfad));
            //   GeneriereIndices("view",3, hgtPfad);
            if (usesrtm)
                aufgabeIndices.Enqueue(new AufgabeIndices("srtm", 3, hgtPfad));

            //   GeneriereIndices("srtm",3, hgtPfad);
            //while (aufgabeIndices.Count > 0)
            //{
            //    AufgabeIndices aufgabe = aufgabeIndices.Dequeue();
            //    System.Diagnostics.Debug.Print(aufgabe.Hgtart + " " + aufgabe.Auflösung+ " " + aufgabe.Pfad);
            //    GeneriereIndices(aufgabe.Hgtart, aufgabe.Auflösung, aufgabe.Pfad);
            //}


            if (ÜberprüfeIndices())
                LadeHGTFiles.IsEnabled = true;
            else
                LadeHGTFiles.IsEnabled = false;
        }

        private void GeneriereIndices(string s, int i, string hgtPfad)
        {
            if (s.ToLower() == "srtm")
                GeneriereSRTMIndex(i, hgtPfad);
            if (s.ToLower() == "view")
                GeneriereViewIndex(i, hgtPfad);
        }

        private void GeneriereSRTMIndex(int i, string hgtPfad)
        {

            if (!File.Exists(hgtPfad + @"\srtmindex" + i + ".xml"))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "  ";
                XmlWriter xmlWriter = XmlWriter.Create(hgtPfad + @"\srtmindex" + i + ".xml", settings);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("HgtDateien");
                xmlWriter.WriteAttributeString("Quelle", "SRTM" + i);

                List<string> vs1 = new List<string>();


                string baseurl = "https://dds.cr.usgs.gov/";
                string url = baseurl + "srtm/version2_1/SRTM" + i;

                string[] vs = SammleUrls(url);

                for (int j = 0; j < vs.Length; j++)
                {

                    if (!vs[j].StartsWith("/") && vs[j].EndsWith("/"))
                    {
                        xmlWriter.WriteStartElement("Abschnitt");
                        xmlWriter.WriteAttributeString("Url", url + "/" + vs[j]);
                        string[] vs2 = SammleUrls(url + "/" + vs[j]);
                        for (int k = 0; k < vs2.Length; k++)
                        {
                            if (!vs2[k].StartsWith("/"))
                            {
                                xmlWriter.WriteStartElement("ZipDatei");
                                xmlWriter.WriteAttributeString("Dateiname", vs2[k]);
                                xmlWriter.WriteElementString("Datei", vs2[k].Substring(0, 7));
                                xmlWriter.WriteEndElement();
                                vs1.Add(vs2[k]);
                            }

                        }


                        xmlWriter.WriteEndElement();
                    }


                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
            }
            if (!Directory.Exists(hgtPfad + @"\SRTM" + i))
                Directory.CreateDirectory(hgtPfad + @"\SRTM" + i);

        }

        private static string[] SammleUrls(string url)
        {
            string s = "";
            WebClient w = new WebClient
            {
                Encoding = Encoding.UTF8
            };
            try
            {
                s = w.DownloadString(url);
            }
            catch (Exception)
            {

                MessageBox.Show("Fehler! Kann Datei: " + url +
                    " nicht downloaden!\nBitte überprüfen Sie Ihre Internezverbindung");
            }

            w.Dispose();
            MatchCollection m = Regex.Matches(s, "<a href=\"([^\"]*)\">");
            string[] vs = new string[m.Count];
            for (int i2 = 0; i2 < m.Count; i2++)
            {
                vs[i2] = m[i2].Groups[1].Value;
            }

            return vs;
        }

        private void GeneriereViewIndex(int i, string hgtPfad)
        {

            if (!File.Exists(hgtPfad + @"\viewindex" + i + ".xml"))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "  ";
                XmlWriter xmlWriter = XmlWriter.Create(hgtPfad + @"\viewindex" + i + ".xml", settings);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("HgtDateien");
                xmlWriter.WriteAttributeString("Quelle", "VIEW" + i);
                string url = "http://www.viewfinderpanoramas.org/Coverage%20map%20viewfinderpanoramas_org" + i + ".htm";

                string[] vs = SammleAreas(url);
                for (int j = 0; j < vs.Length; j++)
                {
                    MatchCollection m1 = Regex.Matches(vs[j], "coords=\"([^\"]*)\"");
                    MatchCollection m2 = Regex.Matches(vs[j], "href=\"([^\"]*)\"");
                    xmlWriter.WriteStartElement("ZipDatei");
                    xmlWriter.WriteAttributeString("Url", m2[0].Groups[1].Value);

                    xmlWriter.WriteElementString("Koordinaten", m1[0].Groups[1].Value);
                    string[] zf = findeZipFiles(m1[0].Groups[1].Value);
                    for (int k = 0; k < zf.Length; k++)
                    {
                        xmlWriter.WriteElementString("Datei", zf[k]);
                    }
                    xmlWriter.WriteEndElement();
                }
                //File.WriteAllLines(hgtPfad + @"\viewfinder" + i + ".txt", vs);

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();


            }
            if (!Directory.Exists(hgtPfad + @"\VIEW" + i))
                Directory.CreateDirectory(hgtPfad + @"\VIEW" + i);
        }

        private string[] findeZipFiles(string value)
        {
            double viewDimension = 1800.0 / 360.0;
            string lonName = "";
            string latName = "";
            string[] vs = value.Split(',');

            int l = int.Parse(vs[0]);
            int t = int.Parse(vs[1]);
            int r = int.Parse(vs[2]);
            int b = int.Parse(vs[3]);
            int w = (int)(l / viewDimension + 0.5) - 180;
            int e = (int)(r / viewDimension + 0.5) - 180;
            int s = 90 - (int)(b / viewDimension + 0.5);
            int n = 90 - (int)(t / viewDimension + 0.5);
            List<string> vs1 = new List<string>();
            for (int lon = w; lon < e; lon++)
            {
                for (int lat = s; lat < n; lat++)
                {
                    if (lon < 0)
                    {
                        lonName = "W" + (-lon).ToString("D3");
                    }
                    else
                    {
                        lonName = "E" + (lon).ToString("D3");
                    }
                    if (lat < 0)
                    {
                        latName = "S" + (-lat).ToString("D2");
                    }
                    else
                    {
                        latName = "N" + (lat).ToString("D2");
                    }
                    string name = latName + lonName;
                    vs1.Add(name);
                }

            }
            string[] ergebnis = new string[vs1.Count];

            for (int i = 0; i < vs1.Count; i++)
            {
                ergebnis[i] = vs1[i];
            }
            return ergebnis;
        }

        private static string[] SammleAreas(string url)
        {
            WebClient w = new WebClient
            {
                Encoding = Encoding.UTF8
            };
            string s = "";
            try
            {
                s = w.DownloadString(url);
            }
            catch (Exception e)
            {
                MessageBox.Show("Fehler" + e.Message + "\nURL: " + url);
            }

            w.Dispose();
            MatchCollection m = Regex.Matches(s, "<area .*>");
            string[] vs = new string[m.Count];
            for (int i2 = 0; i2 < m.Count; i2++)
            {
                vs[i2] = m[i2].Value;
            }

            return vs;
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void SRTM_Checked(object sender, RoutedEventArgs e)
        {
            if (SRTM.IsChecked == true)
                usesrtm = true;
            else
            {
                usesrtm = false;

                if (VIEW.IsChecked == false)
                {
                    useview = true;
                    VIEW.IsChecked = true;
                }


            }
            if (ÜberprüfeIndices())
                LadeHGTFiles.IsEnabled = true;
            else
                LadeHGTFiles.IsEnabled = false;
            generiereDirString();
            ZeichneAlles(punkte);
        }

        private void VIEW_Checked(object sender, RoutedEventArgs e)
        {
            if (VIEW.IsChecked == true)
                useview = true;
            else
            {
                useview = false;

                if (SRTM.IsChecked == false)
                {
                    usesrtm = true;
                    SRTM.IsChecked = true;
                }


            }
            if (ÜberprüfeIndices())
                LadeHGTFiles.IsEnabled = true;
            else
                LadeHGTFiles.IsEnabled = false;
            generiereDirString();
            ZeichneAlles(punkte);
        }

        private void generiereDirString()
        {
            if (!useview)
            {
                directorys[0] = "";
                directorys[1] = "";
            }
            else
            {
                directorys[0] = "VIEW1";
                directorys[1] = "VIEW3";

            }


            if (!usesrtm)
            {
                directorys[2] = "";
                directorys[3] = "";
            }
            else
            {
                directorys[2] = "SRTM1";
                directorys[3] = "SRTM3";

            }

            if (!use1zoll)
            {
                directorys[0] = "";
                directorys[2] = "";
            }
            else
            {
                if (useview)
                    directorys[0] = "VIEW1";
                if (usesrtm)
                    directorys[2] = "SRTM1";

            }

        }

        private void einZoll_Checked(object sender, RoutedEventArgs e)
        {
            if (einZoll.IsChecked == true)
            {
                use1zoll = true;
                lb1Zoll.Content = "1\"";

            }

            else
            {
                use1zoll = false;
                lb1Zoll.Content = "";



            }
            if (ÜberprüfeIndices())
                LadeHGTFiles.IsEnabled = true;
            else
                LadeHGTFiles.IsEnabled = false;
            generiereDirString();
            ZeichneAlles(punkte);
        }



        private void Weiter2_Click(object sender, RoutedEventArgs e)
        {
            Verarbeitung.IsEnabled = true;
            Verarbeitung.IsSelected = true;
        }

        private void Verarbeitung_GotFocus(object sender, RoutedEventArgs e)
        {
            Zeichenfläche = Zeichenfläche3;
            Hauptfenster.ResizeMode = ResizeMode.NoResize;
            ZeichneAlles(punkte);

        }

        private void ZeichneMatrix(List<Filemitauflösung> lfma)
        {
            VierEcken vierEcken = new VierEcken(hgtlinksunten, hgtrechtsoben, lfma[0].Auflösung);
            ZwischenspeicherHgt zwischenspeicherHgt = new ZwischenspeicherHgt(hgtlinksunten, hgtrechtsoben, lfma[0].Auflösung);
            //if (vierEcken.Hgtlinksoben.Name != vierEcken.Hgtrechtsunten.Name) MessageBox.Show("Mehr als eine Hgt-Datei");
            //else MessageBox.Show("Nur eine Hgt-Datei");

            List<FileMitEckKoordinaten> fileMitEcks = new List<FileMitEckKoordinaten>();
            vierEcken.Verzeichnispfad = System.IO.Path.GetDirectoryName(lfma[0].Dateiname);
            foreach (Filemitauflösung item in lfma)
            {
                fileMitEcks.Add(new FileMitEckKoordinaten(System.IO.Path.GetFileNameWithoutExtension(item.Dateiname), item.Auflösung));
            }
            foreach (FileMitEckKoordinaten item in fileMitEcks)
            {
                if (item.Name == vierEcken.Hgtlinksoben.Name)
                {
                    item.Linksoben[0] = vierEcken.Hgtlinksoben.DezLon;
                    item.Linksoben[1] = vierEcken.Hgtlinksoben.DezLat;
                    item.Linksunten[0] = vierEcken.Hgtlinksoben.DezLon;
                    item.Rechtsoben[1] = vierEcken.Hgtlinksoben.DezLat;
                    // Lat = RO
                    // Lon = LU
                }
                if (item.Name == vierEcken.Hgtrechtsoben.Name)
                {
                    item.Rechtsoben[0] = vierEcken.Hgtrechtsoben.DezLon;
                    item.Rechtsoben[1] = vierEcken.Hgtrechtsoben.DezLat;
                    item.Rechtsunten[0] = vierEcken.Hgtrechtsoben.DezLon;
                    item.Linksoben[1] = vierEcken.Hgtrechtsoben.DezLat;
                    // Lat = LO
                    // Lon = RU

                }
                if (item.Name == vierEcken.Hgtlinksunten.Name)
                {
                    item.Linksunten[0] = vierEcken.Hgtlinksunten.DezLon;
                    item.Linksunten[1] = vierEcken.Hgtlinksunten.DezLat;
                    item.Linksoben[0] = vierEcken.Hgtlinksunten.DezLon;
                    item.Rechtsunten[1] = vierEcken.Hgtlinksunten.DezLat;
                    // Lat = RU
                    // Lon = LO
                }
                if (item.Name == vierEcken.Hgtrechtsunten.Name)
                {
                    item.Rechtsunten[0] = vierEcken.Hgtrechtsunten.DezLon;
                    item.Rechtsunten[1] = vierEcken.Hgtrechtsunten.DezLat;
                    item.Rechtsoben[0] = vierEcken.Hgtrechtsunten.DezLon;
                    item.Linksunten[1] = vierEcken.Hgtrechtsunten.DezLat;
                    // Lat = LU
                    // Lon = RO
                }

            }
            int intanzahlLat = (3600 / vierEcken.Auflösung + vierEcken.Hgtlinksoben.DezLat - vierEcken.Hgtlinksunten.DezLat) % (3600 / vierEcken.Auflösung);
            int intanzahlLon = (3600 / vierEcken.Auflösung + vierEcken.Hgtrechtsunten.DezLon - vierEcken.Hgtlinksunten.DezLon) % (3600 / vierEcken.Auflösung);

            int[] zwausmasse = { intanzahlLat, intanzahlLon };

            ZwspeicherHgt = new ZwischenspeicherHgt(hgttolatlon(vierEcken.Hgtlinksunten.Name, vierEcken.Auflösung, vierEcken.Hgtlinksunten.DezLat, vierEcken.Hgtlinksunten.DezLon),
                intanzahlLat, intanzahlLon, vierEcken.Auflösung);
            ZwspeicherHgt.LeseSpeicherEin(vierEcken, fileMitEcks);


            List<GeoPunkt> geoPunkts = new List<GeoPunkt>();
            maximaleHöhe = -10000.0;
            minimaleHöhe = 10000.0;
            GeoPunkt geoPunkt = new GeoPunkt();
            GeoPunkt geoPunkt1 = new GeoPunkt();
            List<GeoPunkt> geos = new List<GeoPunkt>();
            Matrix drehung = BildeDrehungsMatrix(mittelpunkt.Lon, mittelpunkt.Lat, (-1 * winkel));
            foreach (Filemitauflösung item in lfma)
            {
                int auflösung = item.Auflösung;
                HGTFile hGTFile = new HGTFile(auflösung, item.Dateiname);
                short[,] daten = hGTFile.LeseDaten();
                string dateiname = System.IO.Path.GetFileName(item.Dateiname);

                int anzahl = (int)Math.Sqrt(daten.Length);

                for (int i = 0; i < anzahl; i++)
                {
                    for (int j = 0; j < anzahl; j++)
                    {
                        geoPunkt = hgttolatlon(dateiname, auflösung, i, j); // TODO: Berechnung überprüfen
                        geoPunkt.Höhe = daten[i, j];

                        if (IstPunktImRechteck(ref geoPunkt, 0.2))
                        {
                            if (winkel == 0)
                                geoPunkt1 = geoPunkt;
                            else
                            {
                                geoPunkt1 = DrehePunkt(geoPunkt, drehung);
                                //Point point = new Point();
                                geoPunkt1.Höhe = daten[i, j];

                            }

                            //point.X = geoPunkt1.Lat;
                            //point.Y = geoPunkt1.Lon;
                            //TODO: Anzahl der zu lesender Punkte vorher einschränken
                            //TODO: Andere Globusteile einbeziehen Fläche überprüfen Methode Punkt innerhalb Polygon hier und oben
                            if (IstPunktImRechteck(ref geoPunkt1))
                            {
                                if (maximaleHöhe < geoPunkt1.Höhe)
                                    maximaleHöhe = geoPunkt1.Höhe;
                                if (minimaleHöhe > geoPunkt1.Höhe && geoPunkt1.Höhe != -32768)
                                    minimaleHöhe = geoPunkt1.Höhe;

                                if (geoPunkt1.Höhe != -32768)
                                    geoPunkts.Add(geoPunkt1);

                            }
                        }
                    }
                }
                //MessageBox.Show("Anzahl der Punkte: " + geoPunkts.Count);
                daten = null;
                hGTFile = null;
            }

            // ZeichnePunkte(geoPunkts); //TODO: Zeichnen in Image und nicht Canvas als Datei und dann darstellen ??
            tbMaxhöhe.Text = maximaleHöhe.ToString("N0") + "m";
            tbMinHöhe.Text = minimaleHöhe.ToString("N0") + "m";
            geoPunkts.Clear();
        }




        private bool IstPunktImRechteck(ref GeoPunkt geoPunkt1, double diff = 0.0)
        { // TODO: In allen Erdteilen? Datumgrenze?
            return geoPunkt1.Lat <= Math.Max(linksoben.Lat, rechtsoben.Lat) + diff
                && geoPunkt1.Lat >= Math.Min(linksunten.Lat, rechtsunten.Lat) - diff
                && geoPunkt1.Lon <= Math.Max(rechtsoben.Lon, rechtsunten.Lon) + diff
                && geoPunkt1.Lon >= Math.Min(linksunten.Lon, linksoben.Lon) - diff;
        }

        private void Einlesen_Click(object sender, RoutedEventArgs e)
        {
            Filemitauflösung fma = new Filemitauflösung("", 0);
            List<Filemitauflösung> lfma = new List<Filemitauflösung>();

            lfma.Clear();
            //string[] vs = HGTFiles.Text.Split('\n');
            List<int> aufl = new List<int>();
            bool nurdreiZoll = false;
            foreach (string item in lbHgtFiles.Items)
            {

                if (item.Length > 0)
                {
                    fma = findeErsteDatei(item);
                    aufl.Add(fma.Auflösung);

                }
            }
            int aufl1 = aufl.Max();
            if (aufl1 == 3)
                nurdreiZoll = true;

            lfma.Clear();
            foreach (string item in lbHgtFiles.Items)
            {

                if (item.Length > 0)
                {
                    fma = findeErsteDatei(item, nurdreiZoll);
                    if (fma.Auflösung > 0)
                    {
                        fma.Auflösung = fma.Auflösung;
                        lfma.Add(fma);
                        //HGTFile hGTFile = new HGTFile(fma.Auflösung, fma.Dateiname);
                        //hGTFile.LeseDaten();
                        //hGTFile.Name = item;
                        //listHGTFiles.Add(hGTFile);
                    }

                }
                //listHGTFiles.Find(x => x.Name == item);

            }

            ZeichneMatrix(lfma);


        }

        private Filemitauflösung findeErsteDatei(string item, bool nurdreizoll = false)
        {
            int i = 0;
            Filemitauflösung fma = new Filemitauflösung("", 0);
            foreach (string verzeichnis in directorys)

            {
                if (verzeichnis.Length > 0)
                {
                    string a = verzeichnis.Substring(verzeichnis.Length - 1);
                    try
                    {
                        i = int.Parse(a);
                    }
                    catch (Exception)
                    {
                        return fma;
                    }
                    //if (!(nurdreizoll && i == 1))
                    //{

                    if (File.Exists(hgtPfad + "\\" + verzeichnis + "\\" + item + ".hgt"))
                    {
                        fma.Dateiname = hgtPfad + "\\" + verzeichnis + "\\" + item + ".hgt";
                        fma.Auflösung = i;
                        return fma;
                    }
                    //}
                }
            }
            return fma;
        }

        private GeoPunkt hgttolatlon(string filename, int auflösung, int breit, int hoch)
        {
            GeoPunkt geoPunkt = new GeoPunkt();
            string ostwest;
            string nordsüd;
            double lat;
            double lon;
            ostwest = filename.Substring(0, 1);
            nordsüd = filename.Substring(3, 1);
            lat = int.Parse(filename.Substring(1, 2));
            lon = int.Parse(filename.Substring(4, 3));
            if (ostwest == "W")
                lat = -lat;
            if (nordsüd == "S")
                lon = -lon;
            geoPunkt.Lat = lat + breit / 3600.0 * auflösung;
            geoPunkt.Lon = lon + hoch / 3600.0 * auflösung;
            return geoPunkt;
        }

        private void btnAnlagenDirectory_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Bitte Verzeichnis für Anlagen-Dateien auswählen";

            //fbd.RootFolder = Environment.SpecialFolder.Personal;
            string path = Environment.CurrentDirectory;
            fbd.SelectedPath = @"X:\Trend\HöhenGenerator\Anlagen";

            System.Windows.Forms.DialogResult result = fbd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                anlagenpfad = fbd.SelectedPath;
                btnGeneriereAnlage.IsEnabled = true;

            }


        }



        private void tbAnlagenname_TextChanged(object sender, TextChangedEventArgs e)
        {
            anlagenname = tbAnlagenname.Text;
            if (anlagenname.Length == 0)
                btnAnlagenDirectory.IsEnabled = false;
            else
                btnAnlagenDirectory.IsEnabled = true;
        }

        private void btnGeneriereAnlage_Click(object sender, RoutedEventArgs e)
        {
            string[] bitmapnamen = { anlagenname + "B.bmp", anlagenname + "F.bmp", anlagenname + "H.bmp", anlagenname + "S.bmp", anlagenname + "T.bmp" };
            int höhe = 90;
            int breite = 150;
            System.Drawing.Color[] colors = { System.Drawing.Color.FromArgb(255,0, 100, 0) ,
                System.Drawing.Color.FromArgb(255, 200, 200, 200),
                System.Drawing.Color.FromArgb(255, 16, 39, 0),
                System.Drawing.Color.FromArgb(255,0,0,1),
                System.Drawing.Color.FromArgb(255, 0, 0, 100) };

            System.Drawing.Imaging.PixelFormat pixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;




            for (int i = 0; i < bitmapnamen.Length; i++)
            {
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(breite, höhe, pixelFormat);


                ZeichneBitMap zeichneBitMap = new ZeichneBitMap(bitmap, colors[i]);

                zeichneBitMap.FülleBitmap();


                SpeicherBild speicherBild = new SpeicherBild(zeichneBitMap.Bitmap, anlagenpfad + "\\" + bitmapnamen[i]);

                speicherBild.Speichern(zeichneBitMap.Bitmap, anlagenpfad + "\\" + bitmapnamen[i]);

            }

            SchreibeAnlagenFile af = new SchreibeAnlagenFile(anlagenpfad, anlagenname, höhe, breite, 150);
            if (af.SchreibeFile())
                MessageBox.Show("Anlagendatei geschrieben");
            else
                MessageBox.Show("Fehler beim Anlagenscheiben");

        }



        private void generiereAnlage_GotFocus(object sender, RoutedEventArgs e)
        {
            tbAnlagenname.Text = anlagenname;
        }

        private void btnIndex_Click(object sender, RoutedEventArgs e)
        {
            GeneriereIndices();
        }

        private void VIEW_Checked(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}

