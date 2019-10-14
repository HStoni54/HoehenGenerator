using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Compression;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

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
        GeoPunkt verschiebung;
        PointCollection orgpunkte = new PointCollection();
        PointCollection punkte = new PointCollection();
        Canvas Zeichenfläche = new Canvas();
        TextBox HGTFiles = new TextBox();
        bool usesrtm = false;

        bool useview = true;
        bool use3zoll = true;
        bool use1zoll = true;
        
        string hgtPfad;
        bool datumgrenze = false;
        int winkel = 0;

        public bool Datumgrenze { get => datumgrenze; set => datumgrenze = value; }

        public MainWindow()
        {
            InitializeComponent();
            Title = "Höhengenerator für EEP";


        }

        private void LadeDatei_Click(object sender, RoutedEventArgs e)
        {
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
                HGTFiles = HGTFiles1;
                // Optimiere(orgpunkte);
                ZeichneAlles(punkte);
                //ZeichneRechteck(punkte);
                //ZeichnePolygon(punkte);
                //ZeichnePunkte(punkte);
                Optimieren.IsEnabled = true;
                Weiter.IsEnabled = true;
                Drehen.IsEnabled = true;

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
            GeoPunkt linksoben = new GeoPunkt(minLänge, maxBreite);
            GeoPunkt rechtsoben = new GeoPunkt(maxLänge, maxBreite);
            GeoPunkt linksunten = new GeoPunkt(minLänge, minBreite);
            GeoPunkt rechtsunten = new GeoPunkt(maxLänge, minBreite);

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
            HGTFiles.Text = "";
            if (maxlon - minlon > 180)
            {
                BildeHGTString(maxlat, minlat, 180, maxlon);
                BildeHGTString(maxlat, minlat, minlon, -180);
            }
            else
            {
                BildeHGTString(maxlat, minlat, maxlon, minlon);

            }
            HGTFiles.Background = Brushes.Red;
            string[] vs = HGTFiles.Text.Split('\n');
            string[] vs1 = new string[vs.Length - 1];
            bool[] vs2 = new bool[vs.Length - 1];
            for (int i = 0; i < vs.Length - 1; i++)
            {
                vs1[i] = vs[i];
                vs2[i] = false;
            }

            string[] directorys = { "VIEW1", "VIEW3", "SRTM1", "SRTM3" ,"noHgt"};
            for (int i = 0; i < vs1.Length; i++)
            {

                foreach (var directory in directorys)
                {

                    if (File.Exists(hgtPfad + "\\" + directory + "\\" + vs1[i] + ".hgt"))
                        vs2[i] = true;
                }
            }
            bool janein = true;
            for (int i = 0; i < vs2.Length; i++)
            {
                if (vs2[i] == false)
                    janein = false;
            }
            if (janein) HGTFiles.Background = Brushes.LightGreen;
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
                    hgt = hgt + "\n";
                    HGTFiles.Text = HGTFiles.Text + hgt;
                }
            }

            return;
        }


        private void ZeichnePolygon(PointCollection punkte)
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
            HGTFiles = HGTFiles2;
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
            if (use3zoll == true)
                dreiZoll.IsChecked = true;
            else
                dreiZoll.IsChecked = false;
           
               

            ZeichneAlles(punkte);
        }

        private void ladenTab_GotFocus(object sender, RoutedEventArgs e)
        {
            Zeichenfläche = Zeichenfläche1;
        }

        private void ladeHGTFiles_GotFocus(object sender, RoutedEventArgs e)
        {
            Zeichenfläche = Zeichenfläche2;
        }

        private void buttonDirectory_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Bitte Verzeichnis für Hgt-Dateien auswählen";

            //fbd.RootFolder = Environment.SpecialFolder.Personal;
            string path = Environment.CurrentDirectory;
            fbd.SelectedPath = @"X:\Trend\HöhenGenerator\HGT";

            System.Windows.Forms.DialogResult result = fbd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                hgtPfad = fbd.SelectedPath;
                LadeHGTFiles.IsEnabled = true;
                ZeichneAlles(punkte);
            }
            else
            {
                LadeHGTFiles.IsEnabled = false;

            }
        }

        private void LadeHGTFiles_Click(object sender, RoutedEventArgs e)
        {


            GeneriereIndices();
            downloadeHgtFiles();
            unZipHgtFiles();
            ZeichneAlles(punkte);


        }

        private void unZipHgtFiles()
        {
            string[] directorys = { "VIEW1", "VIEW3", "SRTM1", "SRTM3" };
            foreach (var item in directorys)
            {
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
            List<string> vs1 = new List<string>();
            List<string> srtm1 = new List<string>();
            List<string> srtm3 = new List<string>();
            List<string> view1 = new List<string>();
            List<string> view3 = new List<string>();
            string[] vs = HGTFiles.Text.Split('\n');
            for (int i = 0; i < vs.Length; i++)
            {
                if (vs[i] != "")
                {
                    string[] url = findeUrl(vs[i]);
                    if (url.Length == 0)
                    {
                        //MessageBox.Show("Datei: " + vs[i] + "nicht existent!");
                        if (!Directory.Exists(hgtPfad + "\\noHgt"))
                            Directory.CreateDirectory(hgtPfad + "\\noHgt");
                        StreamWriter sw = File.CreateText(hgtPfad + "\\noHgt\\" + vs[i] + ".hgt");
                        sw.Close();
                      
                        
                        
                    }
                    else
                    {
                        for (int j = 0; j < url.Length; j++)
                        {
                            vs1.Add(url[j]);
                            if (url[j].Contains("SRTM1") && !File.Exists(hgtPfad + "\\SRTM1\\" + vs[i] + ".hgt"))
                                srtm1.Add(vs1[j]);
                            if (url[j].Contains("SRTM3") && !File.Exists(hgtPfad + "\\SRTM3\\" + vs[i] + ".hgt"))
                                srtm3.Add(vs1[j]);
                            if (url[j].Contains("dem1") && !File.Exists(hgtPfad + "\\VIEW1\\" + vs[i] + ".hgt"))
                                view1.Add(vs1[j]);
                            if (url[j].Contains("dem3") && !File.Exists(hgtPfad + "\\VIEW3\\" + vs[i] + ".hgt"))
                                view3.Add(vs1[j]);
                        }

                    }

                }

            }
            //for (int i = 0; i < vs1.Count; i++)
            //{
            //    if (vs1[i].Contains("SRTM1"))
            //        srtm1.Add(vs1[i]);
            //    if (vs1[i].Contains("SRTM3"))
            //        srtm3.Add(vs1[i]);
            //    if (vs1[i].Contains("dem1"))
            //        view1.Add(vs1[i]);
            //    if (vs1[i].Contains("dem3"))
            //        view3.Add(vs1[i]);

            //}
            WebClient webClient = new WebClient()
            {
                Encoding = Encoding.UTF8
            };
            for (int i = 0; i < srtm1.Count; i++)
            {
                string dateiname = System.IO.Path.GetFileName(srtm1[i]);
                String Zielname = hgtPfad + "\\SRTM1\\" + dateiname;
                if (!File.Exists(Zielname))
                    webClient.DownloadFile(srtm1[i], Zielname);
            }
            for (int i = 0; i < srtm3.Count; i++)
            {
                string dateiname = System.IO.Path.GetFileName(srtm3[i]);
                String Zielname = hgtPfad + "\\SRTM3\\" + dateiname;
                if (!File.Exists(Zielname))
                    webClient.DownloadFile(srtm3[i], Zielname);
            }
            for (int i = 0; i < view1.Count; i++)
            {
                string dateiname = System.IO.Path.GetFileName(view1[i]);
                String Zielname = hgtPfad + "\\VIEW1\\" + dateiname;
                if (!File.Exists(Zielname))
                    webClient.DownloadFile(view1[i], Zielname);
            }
            for (int i = 0; i < view3.Count; i++)
            {
                string dateiname = System.IO.Path.GetFileName(view3[i]);
                String Zielname = hgtPfad + "\\VIEW3\\" + dateiname;
                if (!File.Exists(Zielname))
                    webClient.DownloadFile(view3[i], Zielname);
            }



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
                        //case XmlNodeType.XmlDeclaration:
                        //    Console.WriteLine("{0,-20}<{1}>", "DEKLARATION", xmlReader.Value);
                        //break;
                        //case XmlNodeType.CDATA:
                        //    Console.WriteLine("{0,-20}{1}", "CDATA", xmlReader.Value);
                        //break;
                        //case XmlNodeType.Whitespace:
                        //Console.WriteLine("{0,-20}", "WHITESPACE");
                        //break;
                        //case XmlNodeType.Comment:
                        //    Console.WriteLine("{0,-20}<!--{1}-->", "COMMENT", xmlReader.Value);
                        //break;
                        case XmlNodeType.Element:
                            if (xmlReader.IsEmptyElement) { }
                            //Console.WriteLine("{0,-20}<{1} />", "EMPTY_ELEMENT", xmlReader.Name);
                            else
                            {
                                //Console.WriteLine("{0,-20}<{1}>", "ELEMENT", xmlReader.Name);
                                // prüfen, ob der Knoten Attribute hat
                                if (xmlReader.Name == "Abschnitt")
                                    knoten = xmlReader.Name;
                                if (xmlReader.Name == "ZipDatei")
                                    knoten = xmlReader.Name;

                                if (xmlReader.HasAttributes)
                                {
                                    // Durch die Attribute navigieren
                                    while (xmlReader.MoveToNextAttribute())
                                    {
                                        //Console.WriteLine("{0,-20}{1}",
                                        //       "ATTRIBUT", xmlReader.Name + "=" + xmlReader.Value);
                                        if (knoten == "Abschnitt" && xmlReader.Name == "Url")
                                            ersterTeil = xmlReader.Value;
                                        if (knoten == "ZipDatei" && (xmlReader.Name == "Dateiname" || xmlReader.Name == "Url"))
                                            zweiterTeil = xmlReader.Value;


                                    }
                                }
                            }
                            break;
                        //case XmlNodeType.EndElement:
                        //    Console.WriteLine("{0,-20}</{1}>", "END_ELEMENT", xmlReader.Name);
                        //break;
                        case XmlNodeType.Text:
                            if (xmlReader.Value == item)
                                ergebnis = ersterTeil + zweiterTeil;
                            //Console.WriteLine("{0,-20}{1}", "TEXT", xmlReader.Value);
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
            for (int i = 1; i < 4; i += 2)
            {
                GeneriereViewIndex(i, hgtPfad);
                GeneriereSRTMIndex(i, hgtPfad);

            }
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
            WebClient w = new WebClient
            {
                Encoding = Encoding.UTF8
            };
            string s = w.DownloadString(url);

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

        }

        private void einZoll_Checked(object sender, RoutedEventArgs e)
        {
            if (einZoll.IsChecked == true)
                use1zoll = true;
            else
            {
                use1zoll = false;

                if (dreiZoll.IsChecked == false)
                {
                    use3zoll = true;
                    dreiZoll.IsChecked = true;
                }


            }

        }

        private void dreiZoll_Checked(object sender, RoutedEventArgs e)
        {
            if (dreiZoll.IsChecked == true)
                use3zoll = true;
            else
            {
                use3zoll = false;

                if (einZoll.IsChecked == false)
                {
                    use1zoll = true;
                    einZoll.IsChecked = true;
                }


            }

        }
    }
}

