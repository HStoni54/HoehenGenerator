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
        int winkel = 0;
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
            } else
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
                neuPunkte = DrehePolygon(orgpunkte, i - (anzahl / 2) );
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
           

            for (int i = 0; i < orgpunkte.Count; i++)
            {

                Matrix drehung = BildeDrehungsMatrix(mittelpunkt.Lon, mittelpunkt.Lat, v);
                points.Add(DrehePunkt(orgpunkte[i],drehung));
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
            NeuPunkte neuPunkte = new NeuPunkte(points,fläche);
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

            double cosalpha =  Math.Cos(GeoPunkt.bogen(alpha));
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
            return  R1 * R2 * R3 * R4 * R5;

  
            //throw new NotImplementedException();
        }

        private static Point DrehePunkt(Point point, Matrix drehung)
        {

            GeoPunkt geoPunkt = new GeoPunkt(point.X, point.Y);
            Matrix P1 = new Matrix(4, 1);
            GeoPunkt geoPunkt1 = new GeoPunkt();


            P1.SetColumn(0, new double[4] { geoPunkt.Ygeo, geoPunkt.Zgeo, geoPunkt.Xgeo, 1.0 });
            //R1.SetColumn(0,[Math.Cos(gradrad),0, Math.Sin(gradrad), 0]);
            Matrix E =  drehung * P1;
            double[] point2 = E.GetColumn(0);
            geoPunkt1.FügeGeopunktEin(point2[2], point2[0], point2[1]);
            Point point1 = new Point(geoPunkt1.Lon,geoPunkt1.Lat);

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
 
            // throw new NotImplementedException();
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

        //private Matrix Rückrechnung(double winkel2, GeoPunkt linksoben2, Matrix G)
        //{
        //    G.SetColumn(0, new double[4] { linksoben2.Ygeo, linksoben2.Zgeo, linksoben2.Xgeo, 0 });

        //    Matrix drehung = BildeDrehungsMatrix(mittelpunkt.Lon, mittelpunkt.Lat, winkel2);
        //    GeoPunkt geoPunkt = new GeoPunkt();
        //    geoPunkt.FügeGeopunktEin();
        //    return drehung * G;
        //}

        private void ZeichnePolygon(PointCollection punkte)
        {
            Polyline polypunkte = new Polyline();
            double Größe, GrößeH, GrößeB, hoehe2, breite2, minLänge, maxLänge, minBreite, maxBreite;
            AnzeigeFlächeBerechnen(punkte, out GrößeH, out GrößeB, out hoehe2, out breite2, out minLänge,  out minBreite, out maxLänge,out maxBreite,out Größe);
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
            //Zeichenfläche.ActualHeight;

            //Zeichenfläche.ActualWidth;
            double Größe, GrößeH, GrößeB, hoehe2, breite2, minLänge, maxLänge, minBreite, maxBreite;
            AnzeigeFlächeBerechnen(punkte, out GrößeH, out GrößeB, out hoehe2, out breite2, out minLänge, out minBreite, out maxLänge, out maxBreite, out Größe);
            //Zeichenfläche.Children.Clear();
            for (int i = 0; i < punkte.Count; i++)
            {
                Ellipse elli = new Ellipse();
                elli.Width = 5.0;
                elli.Height = 5.0;
                elli.Fill = Brushes.Red;
                Zeichenfläche.Children.Add(elli);
                
                Canvas.SetLeft(elli, GrößeB / (maxLänge - minLänge) *  (punkte[i].X - minLänge) - 2.5);
                Canvas.SetBottom(elli, GrößeH / (maxBreite -minBreite) *  (punkte[i].Y -minBreite) - 2.5);
            }
            Ellipse elli2 = new Ellipse();
            elli2.Width = 5.0;
            elli2.Height = 5.0;
            elli2.Fill = Brushes.Red;
            Zeichenfläche.Children.Add(elli2);

            Canvas.SetLeft(elli2, GrößeB / (maxLänge - minLänge) * ((maxLänge - minLänge) / 2) - 2.5);
            Canvas.SetBottom(elli2, GrößeH / (maxBreite - minBreite) * ((maxBreite -  minBreite) / 2) - 2.5);

        }

        private void SepariereKoordinaten(string coordinaten)
        {
            //GeoPunkt[] geoPunkts = new GeoPunkt();

            sepcoordinaten = coordinaten.Split(' ');
            geoPunkts = new GeoPunkt[sepcoordinaten.Length];
            orgpunkte.Clear();
            for (int i = 0; i < sepcoordinaten.Length; i++)
            {
                string[] einekoordinate = sepcoordinaten[i].Split(',');
                //CultureInfo culture = new CultureInfo("en-US");
                
                //Point einpunkt = new Point(double.Parse(einekoordinate[0], culture), double.Parse(einekoordinate[1], culture));
                orgpunkte.Add( new Point(double.Parse(einekoordinate[0], CultureInfo.InvariantCulture), double.Parse(einekoordinate[1], CultureInfo.InvariantCulture)));
                geoPunkts[i] = new GeoPunkt(double.Parse(einekoordinate[0], CultureInfo.InvariantCulture), double.Parse(einekoordinate[1], CultureInfo.InvariantCulture));
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
    }
}

