using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
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
        private string coordinaten;
        private string[] sepcoordinaten;
        private GeoPunkt[] geoPunkts;
        private GeoPunkt mittelpunkt;
        private GeoPunkt linksoben;
        private GeoPunkt rechtsoben;
        private GeoPunkt linksunten;
        private GeoPunkt rechtsunten;
        private GeoPunkt hgtlinksunten;
        private GeoPunkt hgtrechtsoben;
        private readonly PointCollection orgpunkte = new PointCollection();
        private PointCollection punkte = new PointCollection();
        private Canvas Zeichenfläche = new Canvas();
        private ListBox lbHgtFiles = new ListBox();
        private string anlagenname = "Neue Anlage";
        private string anlagenpfad;
        private bool usesrtm = false;
        private bool useview = true;
        private bool use1zoll = true;
        private string hgtPfad;
        private double maximaleHöhe = -10000.0;
        private double minimaleHöhe = 10000.0;
        private double zahlbreiteDerAnlage = 1.5;
        private double zahltbHöheDerAnlage = 1.5;
        private double hoehe2 = 0.6;
        private double breite2 = 1.0;
        private int zahltbRasterdichte = 150;
        private double minLänge, maxLänge, minBreite, maxBreite;
        private int winkel = 0;
        private readonly string[] directorys = { "VIEW1", "SRTM1", "VIEW3", "SRTM3", "noHGT" };
        private readonly ConcurrentQueue<AufgabeIndices> aufgabeIndices = new ConcurrentQueue<AufgabeIndices>();
        private readonly ConcurrentQueue<LadeDateien> ladeDateiens = new ConcurrentQueue<LadeDateien>();
        private readonly ConcurrentQueue<UnzippeDateien> unzippeDateiens = new ConcurrentQueue<UnzippeDateien>();
        private readonly ConcurrentQueue<ZeichePunkteAufCanvas> punkteAufCanvas = new ConcurrentQueue<ZeichePunkteAufCanvas>();
        private readonly ConcurrentQueue<ClGeneriereLeerHGTs> ClGeneriereLeerHGTs = new ConcurrentQueue<ClGeneriereLeerHGTs>();
        private double maximaleEEPHöhe;
        private double minimaleEEPHöhe;
        private double höhenausgleich = 0.0;
        private double ausgleichfaktor = 1.0;
        private double zahlScalierungEEPBreite = 1.0;
        private double zahlScalierungEEPHöhe = 1.0;
        private int[,] baeume;
        private int downloadcount = 0;
        private bool pfahl = false;
        private bool baum = false;
        private int zoom = 20;
        private string pfad;
        private HGTConverter hGTConverter;
        private int auflösung;
        private MapConverter mapConverter;

        public bool Datumgrenze { get; set; } = false;

        private string[] maptype;

        public string[] GetMaptype()
        {
            return maptype;
        }

        public void SetMaptype(string[] value)
        {
            maptype = value;
        }

        public MainWindow()
        {
            InitializeComponent();
 
                SetMaptype(new string[1]);
            GetMaptype()[0] = "OSM";
            //maptype[1] = "ORM";
            Title = "Höhengenerator für EEP " + VersionNr();

  


            Thread thrHoleDateien = new Thread(HoleDateien)
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };
            thrHoleDateien.Start();

            Thread thrHoleIndices = new Thread(HoleIndices)
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };
            thrHoleIndices.Start();

            Thread thrGeneriereLeerHGTs = new Thread(GeneriereLeerHGTs)
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };
            thrGeneriereLeerHGTs.Start();

            Thread thrUnzpFiles = new Thread(UnzipDateien)
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };
            thrUnzpFiles.Start();


            int prozessoranzahl = Environment.ProcessorCount;
            Thread[] thrZeichneCanvas = new Thread[prozessoranzahl];
            for (int i = 0; i < thrZeichneCanvas.Length; i++)
            {
                thrZeichneCanvas[i] = new Thread(ZeichneCanvas)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Lowest
                };
                thrZeichneCanvas[i].Start();
            }

        }

        private static string VersionNr()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            AssemblyName asmName = asm.GetName();
            string Fullname = asm.FullName;
            object[] attribs = asm.GetCustomAttributes(typeof(AssemblyProductAttribute), true);

            if (attribs.Length > 0)
            {
                AssemblyProductAttribute asmProduct = attribs[0] as AssemblyProductAttribute;
            }

            string vers = string.Format(CultureInfo.CurrentCulture, " - Version: {0}.{1}.{2} Build: {3}",
                //productName,
                asmName.Version.Major.ToString(CultureInfo.CurrentCulture),
                asmName.Version.Minor.ToString(CultureInfo.CurrentCulture),
                asmName.Version.Build.ToString(CultureInfo.CurrentCulture),
                asmName.Version.Revision.ToString(CultureInfo.CurrentCulture));

            return vers;
            //return Fullname;
        }

        private void GeneriereLeerHGTs()
        {
            while (true)
            {
                while (ClGeneriereLeerHGTs.Count > 0)

                {
                    bool istArbeitDa = ClGeneriereLeerHGTs.TryDequeue(out ClGeneriereLeerHGTs datei);
                    if (istArbeitDa)
                    {

                        if (!File.Exists(datei.HgtDateiname1))
                        {

                            FileStream stream;
                            try
                            {
                                stream = File.Create(datei.HgtDateiname1);
                            }
                            catch (Exception)
                            {

                                throw;
                            }
                            for (int i = 0; i < 1201; i++)
                            {
                                for (int j = 0; j < 1201; j++)
                                {
                                    //for (int k = 0; k < 2; k++)
                                    //{
                                    stream.WriteByte(128);
                                    stream.WriteByte(0);


                                    //}
                                }
                            }
                            stream.Close();
                            Dispatcher.BeginInvoke(new Action(() => FärbeHgtLabel(datei.HgtDateiname1)));
                        }
                    }


                }
                Thread.Sleep(100);
            }
        }


        private void ZeichneCanvas()
        {

            while (true)
            {
                while (punkteAufCanvas.Count > 0)
                {
                    bool istArbeitDa = punkteAufCanvas.TryDequeue(out ZeichePunkteAufCanvas datei);
                    if (istArbeitDa)
                    {

                        Dispatcher.BeginInvoke(new Action(() => ZeichneCanvasPunkte(datei.MySolidColorBrush, datei.Punktgröße, datei.Lon1, datei.Lat1)));
                    }


                }
                Thread.Sleep(100);
            }
        }

        private void UnzipDateien()
        {
            while (true)
            {
                while (unzippeDateiens.Count > 0)
                {
                    bool istArbeitDa = unzippeDateiens.TryDequeue(out UnzippeDateien datei);
                    if (istArbeitDa)
                    {
                        UnZipHgtFiles(datei.Zieldatei);
                        downloadcount -= 1;
                        Dispatcher.BeginInvoke(new Action(() => FärbeHgtLabel(datei.Zieldatei)));
                        //FärbeHgtLabel(datei.Zieldatei);
                        Dispatcher.BeginInvoke(new Action(() => ZeichneAlles(punkte)));
                        //ZeichneAlles(punkte);
                        if (downloadcount == 0)
                        {
                            Dispatcher.BeginInvoke(new Action(() => Weiter2.IsEnabled = true));
                        }
                    }
                }
                Thread.Sleep(100);
            }

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

                        if (LadeHGTDateien(datei.Url, datei.Zieldatei))
                        {
                            unzippeDateiens.Enqueue(new UnzippeDateien(datei.Zieldatei));
                        }
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
                        {
                            Dispatcher.BeginInvoke(new Action(() => LadeHGTFiles.IsEnabled = true));
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() => LadeHGTFiles.IsEnabled = false));
                        }
                    }
                }
                Thread.Sleep(100);

            }
        }

        private void FärbeHgtLabel(string zieldatei)
        {
            SolidColorBrush solidColor;
            string directory = System.IO.Path.GetDirectoryName(zieldatei);
            for (int i = 0; i < lbHgtFiles.Items.Count; i++)
            {
                if (File.Exists(directory + "\\" + lbHgtFiles.Items[i].ToString() + ".hgt"))
                {
                    if (directory.EndsWith("1", StringComparison.CurrentCulture))
                    {
                        solidColor = Brushes.LightBlue;
                    }
                    else
                        if (directory.EndsWith("3", StringComparison.CurrentCulture))
                    {
                        solidColor = Brushes.LightGreen;
                    }
                    else
                    {
                        solidColor = Brushes.Yellow;
                    }
                }
                else
                {
                    solidColor = Brushes.Red;
                }

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
                pfad = System.IO.Path.GetDirectoryName(vName);
                if (!Directory.Exists(pfad + "\\OSM"))
                {
                    try
                    {

                        Directory.CreateDirectory(pfad + "\\OSM");
                    }
                    catch (Exception)
                    {

                        MessageBox.Show("Kann Directory für OSM-Dateien nicht erstellen!\n"
                            + "Überprüfen Sie die Schreibberechtigung im Verzeichnis:\n"
                            + "\"" + pfad + "\"");

                    }
                }

                foreach (string item in Directory.GetFiles(pfad + "\\OSM", "*.bmp"))
                {
                    File.Delete(item);
                }

                if (!Directory.Exists(pfad + "\\HGT"))
                {
                    try
                    {

                        Directory.CreateDirectory(pfad + "\\HGT");
                    }
                    catch (Exception)
                    {

                        MessageBox.Show("Kann Directory für Hgt-Dateien nicht erstellen!\n"
                            + "Überprüfen Sie die Schreibberechtigung im Verzeichnis:\n"
                            + "\"" + pfad + "\"");

                    }
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

                }
                File.Delete(hgtPfad + "\\test.txt");
                if (Directory.Exists(hgtPfad + @"\noHGT"))
                {
                    string[] file = Directory.GetFiles(hgtPfad + @"\noHGT");
                    foreach (string item in file)
                    {

                        File.Delete(item);
                    }
                }



                if (!Directory.Exists(pfad + "\\Anlagen"))
                {
                    try
                    {

                        Directory.CreateDirectory(pfad + "\\Anlagen");
                    }
                    catch (Exception)
                    {

                        MessageBox.Show("Kann Directory für EEP-Anlagen nicht erstellen!\n"
                            + "Überprüfen Sie die Schreibberechtigung im Verzeichnis:\n"
                            + "\"" + pfad + "\"");

                    }
                }

                anlagenpfad = pfad + "\\Anlagen";
                try
                {
                    FileStream fs = File.Create(anlagenpfad + "\\test.txt");
                    fs.Close();

                }
                catch (Exception)
                {
                    MessageBox.Show("Kann im Directory für EEP-Anlagen nicht schreiben!\n"
                            + "Überprüfen Sie die Schreibberechtigung im Verzeichnis:\n"
                            + "\"" + pfad + "\"");

                }
                File.Delete(anlagenpfad + "\\test.txt");
                if (!Directory.Exists(hgtPfad + @"\noHGT"))
                {
                    Directory.CreateDirectory(hgtPfad + @"\noHGT");
                }

                btnAnlagenDirectory.IsEnabled = false;
                btnGeneriereAnlage.IsEnabled = true;
                generiereAnlage.IsEnabled = false;
                Weiter2.IsEnabled = false;
                anlagenname = System.IO.Path.GetFileNameWithoutExtension(vName);
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
            hGTConverter = null;
            tbMaxhöhe.Text = "";
            tbMinHöhe.Text = "";
            btWeiter3.IsEnabled = false;
            SuchenNode(ge);
            if (coordinaten.Length > 0)
            {
                SepariereKoordinaten(coordinaten);
                punkte = orgpunkte;
                Zeichenfläche = Zeichenfläche1;

                ZeichneAlles(punkte);
                hgtlinksunten = linksunten;
                hgtrechtsoben = rechtsoben;
                if (lbHgtFiles.Items.Count <= 4)
                {
                    Optimieren.IsEnabled = true;
                    Weiter.IsEnabled = true;
                    Drehen.IsEnabled = true;
                    GeneriereDirString();
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


        private void Optimiere(PointCollection orgpunkte)
        {
            NeuPunkte neuPunkte;
            int anzahl = 180;
            double fläche = 0;
            // int winkel = 0;


            for (int i = 0; i < anzahl; i++)
            {
                neuPunkte = DrehePolygon(orgpunkte, i - (anzahl / 2));
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
                {
                    point.X += 360;
                }

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

            double cosalpha = Math.Cos(GeoPunkt.Bogen(alpha));
            double sinalpha = Math.Sin(GeoPunkt.Bogen(alpha));
            double cosbeta = Math.Cos(GeoPunkt.Bogen(beta));
            double sinbeta = Math.Sin(GeoPunkt.Bogen(beta));
            double cosphi = Math.Cos(GeoPunkt.Bogen(phi));
            double sinphi = Math.Sin(GeoPunkt.Bogen(phi));

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

            return R1 * R2 * R3 * R4 * R5;


        }

        private static Point DrehePunkt(Point point, Matrix drehung)
        {

            GeoPunkt geoPunkt = new GeoPunkt(point.X, point.Y);
            Matrix P1 = new Matrix(4, 1);
            GeoPunkt geoPunkt1 = new GeoPunkt();


            P1.SetColumn(0, new double[4] { geoPunkt.Ygeo, geoPunkt.Zgeo, geoPunkt.Xgeo, 1.0 });
            Matrix E = drehung * P1;
            double[] point2 = E.GetColumn(0);
            geoPunkt1.FügeGeopunktEin(point2[2], point2[0], point2[1]);
            Point point1 = new Point(geoPunkt1.Lon, geoPunkt1.Lat);


            return point1;
        }
        private static GeoPunkt DrehePunkt(GeoPunkt geoPunkt, Matrix drehung)
        {

            Matrix P1 = new Matrix(4, 1);
            GeoPunkt geoPunkt1 = new GeoPunkt();


            P1.SetColumn(0, new double[4] { geoPunkt.Ygeo, geoPunkt.Zgeo, geoPunkt.Xgeo, 1.0 });
            Matrix E = drehung * P1;
            double[] point2 = E.GetColumn(0);
            geoPunkt1.FügeGeopunktEin(point2[2], point2[0], point2[1]);

            return geoPunkt1;
        }

        private void ZeichneRechteck(PointCollection punkte)
        {
            Polyline rechteckpunkte = new Polyline();
            AnzeigeFlächeBerechnen(punkte, out double GrößeH, out double GrößeB, out hoehe2, out breite2, out minLänge, out maxLänge, out minBreite, out maxBreite, out double Größe);
            double flaeche2 = hoehe2 * breite2;
            fläche.Text = Math.Round(flaeche2, 2).ToString(CultureInfo.CurrentCulture) + " km²";
            höhe.Text = Math.Round(hoehe2, 2).ToString(CultureInfo.CurrentCulture) + " km";
            breite.Text = Math.Round(breite2, 2).ToString(CultureInfo.CurrentCulture) + " km";
            Zeichenfläche.Children.Clear();
            PointCollection canvasrechteckpunkte = new PointCollection
            {
                new Point(GrößeB, -1 * GrößeH),
                new Point(0, -1 * GrößeH),
                new Point(0, -1 * 0),
                new Point(GrößeB, -1 * 0),
                new Point(GrößeB, -1 * GrößeH)
            };
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

            double maxlat = Math.Round(points1.Max(x => x.Y + 0.002) - 0.5);
            double minlat = Math.Round(points1.Min(x => x.Y - 0.002) - 0.5);
            double maxlon = Math.Round(points1.Max(x => x.X + 0.002) - 0.5);
            double minlon = Math.Round(points1.Min(x => x.X - 0.002) - 0.5);
            lbHgtFiles.Items.Clear();
            // HGTFiles.Text = "";
            if (maxlon - minlon > 180)
            {
                BildeHGTString(maxlat, minlat, 179, maxlon);
                BildeHGTString(maxlat, minlat, minlon, -180);
            }
            else
            {
                BildeHGTString(maxlat, minlat, maxlon, minlon);

            }
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
                    foreach (string directory in directorys)
                    {
                        if (directory.Length > 0)
                        {
                            if (!DateiVorhanden)
                            {
                                if (File.Exists(hgtPfad + "\\" + directory + "\\" + vs1[i] + ".hgt"))
                                {
                                    DateiVorhanden = true;
                                    vs2[i] = true;
                                    FärbeHgtLabel(hgtPfad + "\\" + directory + "\\" + vs1[i] + ".hgt");
                                }

                            }
                        }

                        if (!DateiVorhanden)
                        {
                            FärbeHgtLabel(hgtPfad + "\\" + directory + "\\" + vs1[i] + ".hgt");
                        }
                    }
                }

            }
            bool janein = true;
            for (int i = 0; i < vs2.Length; i++)
            {
                if (vs2[i] == false)
                {
                    janein = false;
                }
            }
            if (janein)
            {
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
                        hgt = "N" + i.ToString("D2", CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        hgt = "S" + (-i).ToString("D2", CultureInfo.CurrentCulture);
                    }
                    if (j >= 0)
                    {
                        hgt = hgt + "E" + j.ToString("D3", CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        hgt = hgt + "W" + (-j).ToString("D3", CultureInfo.CurrentCulture);
                    }
                    lbHgtFiles.Items.Add(hgt);

                }
            }

            return;
        }


        private void ZeichnePolygon(PointCollection punkte)
        {


            Polyline polypunkte = new Polyline();
            AnzeigeFlächeBerechnen(punkte, out double GrößeH, out double GrößeB, out hoehe2, out breite2, out minLänge, out minBreite, out maxLänge, out maxBreite, out double Größe);
            PointCollection canvaspunkte = new PointCollection();
            for (int i = 0; i < punkte.Count; i++)
            {
                canvaspunkte.Add(new Point(GrößeB / (maxLänge - minLänge) * (punkte[i].X - minLänge), -1 * GrößeH / (maxBreite - minBreite) * (punkte[i].Y - minBreite)));
            }
            polypunkte.Points = canvaspunkte;
            Zeichenfläche.Children.Add(polypunkte);
            Canvas.SetLeft(polypunkte, 0);
            Canvas.SetBottom(polypunkte, 0);

        }

        private void ZeichnePunkte(PointCollection punkte)
        {

            AnzeigeFlächeBerechnen(punkte, out double GrößeH, out double GrößeB, out hoehe2, out breite2, out double minLänge, out double minBreite, out double maxLänge, out double maxBreite, out double Größe);
            for (int i = 0; i < punkte.Count; i++)
            {
                Ellipse elli = new Ellipse
                {
                    Width = 5.0,
                    Height = 5.0,
                    Fill = Brushes.Red
                };
                Zeichenfläche.Children.Add(elli);

                Canvas.SetLeft(elli, GrößeB / (maxLänge - minLänge) * (punkte[i].X - minLänge) - 2.5);
                Canvas.SetBottom(elli, GrößeH / (maxBreite - minBreite) * (punkte[i].Y - minBreite) - 2.5);
            }

        }

        private void FülleAnzeigeFläche()
        {
            double höhendifferenz;
            AnzeigeFlächeBerechnen(out double GrößeH, out double GrößeB, out double hoehe2, out double breite2, out minLänge, out minBreite, out maxLänge, out maxBreite, out double Größe);
            Matrix drehung = BildeDrehungsMatrix(mittelpunkt.Lon, mittelpunkt.Lat, (-winkel));
            GeoPunkt tempPunkt;
            GeoPunkt temppunkt1;
            hGTConverter.SetInterpolationMethod(HGTConverter.InterpolationMethod.Bilinear);
            for (int i = 0; i < (int)GrößeH; i++)
            {
                for (int j = 0; j < GrößeB; j++)
                {

                    tempPunkt = new GeoPunkt(j / GrößeB * (maxLänge - minLänge) + minLänge, i / GrößeH * (maxBreite - minBreite) + minBreite);
                    temppunkt1 = DrehePunkt(tempPunkt, drehung);
                    double abshöhe = hGTConverter.GetHoehe(temppunkt1);

                }
            }
            maximaleHöhe = hGTConverter.maxhöhe + 1;
            minimaleHöhe = hGTConverter.minhöhe;

            höhendifferenz = maximaleHöhe - minimaleHöhe;

            for (int i = 0; i < (int)GrößeH; i++)
            {
                for (int j = 0; j < GrößeB; j++)
                {

                    tempPunkt = new GeoPunkt(j / GrößeB * (maxLänge - minLänge) + minLänge, i / GrößeH * (maxBreite - minBreite) + minBreite);
                    temppunkt1 = DrehePunkt(tempPunkt, drehung);
                    double abshöhe = hGTConverter.GetHoehe(temppunkt1);
                    int eephöhe = (int)Math.Round((abshöhe * 100) + 10000);
                    if (eephöhe < 0)
                    {
                        eephöhe = 0;
                    }

                    if (eephöhe == 0)
                    {

                    }
                    if (eephöhe >= 110000)
                    {
                        eephöhe = 109999;
                    }
                    int r1 = eephöhe % 256;
                    int g1 = (eephöhe / 256) % 256;
                    int b1 = (eephöhe / 256 / 256) % 256;
                    double relhöhe = (((abshöhe - minimaleHöhe) * 100 + 1000) / (höhendifferenz + 10) / 100 * 256 - 1);
                    byte höhe1 = (byte)relhöhe;

                    b1 = höhe1;
                    r1 = b1;
                    g1 = b1;
                    //b1 = 0;
                    byte r = (byte)r1;
                    byte g = (byte)g1;
                    byte b = (byte)b1;
                    SolidColorBrush mySolidColorBrush = new SolidColorBrush
                    {
                        Color = Color.FromRgb(r, g, b)
                    };

                    if ((i % 5 == 0) && (j % 5 == 0))
                    {
                        punkteAufCanvas.Enqueue(new ZeichePunkteAufCanvas(mySolidColorBrush, 7, j, i));
                    }
                }

            }
            tbMaxhöhe.Text = maximaleHöhe.ToString("N0", CultureInfo.CurrentCulture) + " m";
            tbMinHöhe.Text = minimaleHöhe.ToString("N0", CultureInfo.CurrentCulture) + " m";
            btWeiter3.IsEnabled = true;

        }

        private void ZeichneCanvasPunkte(SolidColorBrush mySolidColorBrush, double punktgröße, int Lon, int Lat)
        {
            Ellipse elli = new Ellipse
            {
                Width = punktgröße,
                Height = punktgröße,

                Fill = mySolidColorBrush
            };
            //elli.Fill = Brushes.Yellow;
            Zeichenfläche.Children.Add(elli);

            Canvas.SetLeft(elli, Lon - punktgröße / 2);
            Canvas.SetBottom(elli, Lat - punktgröße / 2);



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

            sepcoordinaten = coordinaten.Split(' ');
            geoPunkts = new GeoPunkt[sepcoordinaten.Length];
            orgpunkte.Clear();
            Datumgrenze = false;
            for (int i = 0; i < sepcoordinaten.Length; i++)
            {
                string[] einekoordinate = sepcoordinaten[i].Split(',');
                orgpunkte.Add(new Point(double.Parse(einekoordinate[0], CultureInfo.InvariantCulture), double.Parse(einekoordinate[1], CultureInfo.InvariantCulture)));
                geoPunkts[i] = new GeoPunkt(double.Parse(einekoordinate[0], CultureInfo.InvariantCulture), double.Parse(einekoordinate[1], CultureInfo.InvariantCulture));
                if (geoPunkts[i].Lon > 180 || geoPunkts[i].Lon < -180)
                {
                    Datumgrenze = true;
                }

                if (i > 0)
                {
                    geoPunkts[i].Entfernung = GeoPunkt.BestimmeAbstand(geoPunkts[i], geoPunkts[i - 1]);
                }
            }
            mittelpunkt = new GeoPunkt(((orgpunkte.Max(x => x.X) - orgpunkte.Min(x => x.X)) / 2.0 + orgpunkte.Min(x => x.X)), ((orgpunkte.Max(x => x.Y) - orgpunkte.Min(x => x.Y)) / 2.0 + orgpunkte.Min(x => x.Y)));
        }

        private void SuchenNode(XmlNode ge)
        {
            if (!ge.HasChildNodes)
            {
                return;
            }
            for (int i = 0; i < ge.ChildNodes.Count; i++)
            {
                if (ge.ChildNodes[i].Name == "coordinates")
                {
                    coordinaten += " ";
                    coordinaten += ge.ChildNodes[i].InnerText;
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
            winkel += 90;
            NeuPunkte neuPunkte = DrehePolygon(orgpunkte, winkel);
            punkte = neuPunkte.Punkte;
            ZeichneAlles(punkte);
        }

        private void Weiter_Click(object sender, RoutedEventArgs e)
        {
            ladeHGTFiles.IsEnabled = true;
            ladeHGTFiles.IsSelected = true;
            Zeichenfläche = Zeichenfläche2;
            if (usesrtm == true)
            {
                SRTM.IsChecked = true;
            }
            else
            {
                SRTM.IsChecked = false;
            }

            if (useview == true)
            {
                VIEW.IsChecked = true;
            }
            else
            {
                VIEW.IsChecked = false;
            }

            if (use1zoll == true)
            {
                einZoll.IsChecked = true;
            }
            else
            {
                einZoll.IsChecked = false;
            }

            ZeichneAlles(punkte);
        }

        private void LadenTab_GotFocus(object sender, RoutedEventArgs e)
        {
            ladeHGTFiles.IsEnabled = false;
            Verarbeitung.IsEnabled = false;
            generiereAnlage.IsEnabled = false;
            Zeichenfläche = Zeichenfläche1;

            Hauptfenster.ResizeMode = ResizeMode.CanResize;
        }

        private void LadeHGTFiles_GotFocus(object sender, RoutedEventArgs e)
        {
            Verarbeitung.IsEnabled = false;
            generiereAnlage.IsEnabled = false;
            Weiter2.IsEnabled = false;
            btnIndex.IsEnabled = true;
            ZeichneAlles(punkte);
            if (ÜberprüfeIndices())
            {
                LadeHGTFiles.IsEnabled = true;
            }
            else
            {
                LadeHGTFiles.IsEnabled = false;
            }

            Zeichenfläche = Zeichenfläche2;

            Hauptfenster.ResizeMode = ResizeMode.CanResize;
            int anzahl = lbHgtFiles.Items.Count;
            if (anzahl > 0)
            {
                lbFile1.Content = lbHgtFiles.Items[0].ToString();
            }
            else
            {
                lbFile1.Content = "";
            }

            if (anzahl > 1)
            {
                lbFile2.Content = lbHgtFiles.Items[1].ToString();
            }
            else
            {
                lbFile2.Content = "";
            }

            if (anzahl > 2)
            {
                lbFile3.Content = lbHgtFiles.Items[2].ToString();
            }
            else
            {
                lbFile3.Content = "";
            }

            if (anzahl > 3)
            {
                lbFile4.Content = lbHgtFiles.Items[3].ToString();
            }
            else
            {
                lbFile4.Content = "";
            }

            lb3Zoll.Content = "3\"";
            if (use1zoll)
            {
                lb1Zoll.Content = "1\"";
            }
            else
            {
                lb1Zoll.Content = "";
            }
        }



        private bool ÜberprüfeIndices()
        {
            bool vorhanden = true;
            if (use1zoll)
            {
                if (useview)
                {
                    if (!ÜberprüfeViewIndex(1, hgtPfad))
                    {
                        vorhanden = false;
                    }
                }

                if (usesrtm)
                {
                    if (!ÜberprüfeSRTMIndex(1, hgtPfad))
                    {
                        vorhanden = false;
                    }
                }
            }

            if (useview)
            {
                if (!ÜberprüfeViewIndex(3, hgtPfad))
                {
                    vorhanden = false;
                }
            }

            if (usesrtm)
            {
                if (!ÜberprüfeSRTMIndex(3, hgtPfad))
                {
                    vorhanden = false;
                }
            }

            return vorhanden;
        }

        private static bool ÜberprüfeSRTMIndex(int v, string hgtPfad)
        {
            if (File.Exists(hgtPfad + @"\srtmindex" + v + ".xml"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool ÜberprüfeViewIndex(int v, string hgtPfad)
        {
            if (File.Exists(hgtPfad + @"\viewindex" + v + ".xml"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void LadeHGTFiles_Click(object sender, RoutedEventArgs e)
        {



            DownloadeHgtFiles();



        }
        private static void UnZipHgtFiles(string zieldatei)
        {
            string pfad = System.IO.Path.GetDirectoryName(zieldatei);
            ZipArchive zipfile = ZipFile.OpenRead(zieldatei);
            foreach (ZipArchiveEntry entry in zipfile.Entries)
            {
                if (entry.Name.Length > 0)
                {
                    entry.ExtractToFile(pfad + "\\" + entry.Name, overwrite: true);
                }
            }
            zipfile.Dispose();
            File.Delete(zieldatei);


        }



        private void DownloadeHgtFiles()
        {
            List<string> srtm1 = new List<string>();
            List<string> srtm3 = new List<string>();
            List<string> view1 = new List<string>();
            List<string> view3 = new List<string>();

            for (int i = 0; i < lbHgtFiles.Items.Count; i++)
            {
                string file = lbHgtFiles.Items[i].ToString();
                if (file.Length > 0)
                {
                    ClGeneriereLeerHGTs.Enqueue(new HoehenGenerator.ClGeneriereLeerHGTs(hgtPfad + @"\noHGT\" + file + ".hgt"));
                    string[] url = FindeUrl(file);
                    if (url.Length != 0)

                    {
                        for (int j = 0; j < url.Length; j++)
                        {
                            if (url[j].Contains("SRTM1") && !File.Exists(hgtPfad + "\\SRTM1\\" + file + ".hgt"))
                            {
                                if (!srtm1.Contains(url[j]))
                                {
                                    srtm1.Add(url[j]);
                                }
                            }

                            if (url[j].Contains("SRTM3") && !File.Exists(hgtPfad + "\\SRTM3\\" + file + ".hgt"))
                            {
                                if (!srtm3.Contains(url[j]))
                                {
                                    srtm3.Add(url[j]);
                                }
                            }

                            if (url[j].Contains("dem1") && !File.Exists(hgtPfad + "\\VIEW1\\" + file + ".hgt"))
                            {
                                if (!view1.Contains(url[j]))
                                {
                                    view1.Add(url[j]);
                                }
                            }

                            if (url[j].Contains("dem3") && !File.Exists(hgtPfad + "\\VIEW3\\" + file + ".hgt"))
                            {
                                if (!view3.Contains(url[j]))
                                {
                                    view3.Add(url[j]);
                                }
                            }
                        }

                    }

                }

            }
            downloadcount = srtm1.Count + srtm3.Count + view1.Count + view3.Count;
            for (int i = 0; i < srtm1.Count; i++)
            {
                string dateiname = System.IO.Path.GetFileName(srtm1[i]);
                string Zielname = hgtPfad + "\\SRTM1\\" + dateiname;
                if (!File.Exists(Zielname))
                {
                    ladeDateiens.Enqueue(new LadeDateien(srtm1[i], Zielname));
                }
            }
            for (int i = 0; i < srtm3.Count; i++)
            {
                string dateiname = System.IO.Path.GetFileName(srtm3[i]);
                string Zielname = hgtPfad + "\\SRTM3\\" + dateiname;
                if (!File.Exists(Zielname))
                {
                    ladeDateiens.Enqueue(new LadeDateien(srtm3[i], Zielname));
                }
            }
            for (int i = 0; i < view1.Count; i++)
            {
                string dateiname = System.IO.Path.GetFileName(view1[i]);
                string Zielname = hgtPfad + "\\VIEW1\\" + dateiname;
                if (!File.Exists(Zielname))
                {
                    ladeDateiens.Enqueue(new LadeDateien(view1[i], Zielname));
                }
            }
            for (int i = 0; i < view3.Count; i++)
            {
                string dateiname = System.IO.Path.GetFileName(view3[i]);
                string Zielname = hgtPfad + "\\VIEW3\\" + dateiname;
                if (!File.Exists(Zielname))
                {
                    ladeDateiens.Enqueue(new LadeDateien(view3[i], Zielname));
                }
            }



        }

        private static bool LadeHGTDateien(string v, string zielname)
        {
            bool ergebnis = true;
            WebClient webClient = new WebClient()
            {
                Encoding = Encoding.UTF8
            };
            webClient.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0)");

            try
            {
                webClient.DownloadFile(v, zielname);
            }
            catch (Exception)
            {

                MessageBox.Show("Fehler! Kann Datei: " + v +
                   " nicht downloaden!\nBitte überprüfen Sie Ihre Internetverbindung");
                ergebnis = false;
            }

            webClient.Dispose();
            return ergebnis;
        }

        private string[] FindeUrl(string item)
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
                {
                    vs1.Add(vs[i]);
                }
            }
            string[] vs2 = new string[vs1.Count];
            for (int i = 0; i < vs1.Count; i++)
            {
                vs2[i] = vs1[i];
            }
            return vs2;
        }

        private static string DurchsucheIndex(int i, string hgtPfad, string item, string v)
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
                                {
                                    knoten = xmlReader.Name;
                                }

                                if (xmlReader.Name == "ZipDatei")
                                {
                                    knoten = xmlReader.Name;
                                }

                                if (xmlReader.HasAttributes)
                                {
                                    // Durch die Attribute navigieren
                                    while (xmlReader.MoveToNextAttribute())
                                    {

                                        if (knoten == "Abschnitt" && xmlReader.Name == "Url")
                                        {
                                            ersterTeil = xmlReader.Value;
                                        }

                                        if (knoten == "ZipDatei" && (xmlReader.Name == "Dateiname" || xmlReader.Name == "Url"))
                                        {
                                            zweiterTeil = xmlReader.Value;
                                        }
                                    }
                                }
                            }

                            else
                            { }
                            break;

                        case XmlNodeType.Text:
                            if (xmlReader.Value == item)
                            {
                                ergebnis = ersterTeil + zweiterTeil;
                            }

                            break;
                        default:
                            break;

                    }
                }
                xmlReader.Close();
            }
            return ergebnis;
        }

        private void GeneriereIndices()
        {

            if (use1zoll)
            {
                if (useview)
                {
                    aufgabeIndices.Enqueue(new AufgabeIndices("view", 1, hgtPfad));
                }

                if (usesrtm)
                {
                    aufgabeIndices.Enqueue(new AufgabeIndices("srtm", 1, hgtPfad));
                }
            }

            if (useview)
            {
                aufgabeIndices.Enqueue(new AufgabeIndices("view", 3, hgtPfad));
            }

            if (usesrtm)
            {
                aufgabeIndices.Enqueue(new AufgabeIndices("srtm", 3, hgtPfad));
            }

            if (ÜberprüfeIndices())
            {
                LadeHGTFiles.IsEnabled = true;
            }
            else
            {
                LadeHGTFiles.IsEnabled = false;
            }
        }

        private static bool GeneriereIndices(string s, int i, string hgtPfad)
        {
            bool ergebnis = false;
            if (s.ToLower(CultureInfo.CurrentCulture) == "srtm")
            {
                if (GeneriereSRTMIndex(i, hgtPfad))
                {
                    ergebnis = true;
                }
            }

            if (s.ToLower(CultureInfo.CurrentCulture) == "view")
            {
                if (GeneriereViewIndex(i, hgtPfad))
                {
                    ergebnis = true;
                }
            }

            return ergebnis;
        }

        private static bool GeneriereSRTMIndex(int i, string hgtPfad)
        {
            bool ergebnis = false;

            if (!File.Exists(hgtPfad + @"\srtmindex" + i + ".xml"))
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  "
                };
                XmlWriter xmlWriter = XmlWriter.Create(hgtPfad + @"\srtmindex" + i + ".xml", settings);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("HgtDateien");
                xmlWriter.WriteAttributeString("Quelle", "SRTM" + i);

                List<string> vs1 = new List<string>();


                string baseurl = "https://dds.cr.usgs.gov/";
                string url = baseurl + "srtm/version2_1/SRTM" + i;

                string[] vs = SammleUrls(url);
                if (vs.Length > 0)
                {
                    ergebnis = true;
                }

                for (int j = 0; j < vs.Length; j++)
                {

                    if (!vs[j].StartsWith("/", StringComparison.CurrentCulture) && vs[j].EndsWith("/", StringComparison.CurrentCulture))
                    {
                        xmlWriter.WriteStartElement("Abschnitt");
                        xmlWriter.WriteAttributeString("Url", url + "/" + vs[j]);
                        string[] vs2 = SammleUrls(url + "/" + vs[j]);
                        for (int k = 0; k < vs2.Length; k++)
                        {
                            if (!vs2[k].StartsWith("/", StringComparison.CurrentCulture))
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
                if (!ergebnis)
                {
                    File.Delete(hgtPfad + @"\srtmindex" + i + ".xml");
                }
            }
            else
            {
                ergebnis = true;
            }

            if (!Directory.Exists(hgtPfad + @"\SRTM" + i))
            {
                Directory.CreateDirectory(hgtPfad + @"\SRTM" + i);
            }

            if (!Directory.Exists(hgtPfad + @"\noHGT"))
            {
                Directory.CreateDirectory(hgtPfad + @"\noHGT");
            }

            return ergebnis;
        }

        private static string[] SammleUrls(string url)
        {
            string s = "";
            WebClient w = new WebClient
            {
                Encoding = Encoding.UTF8
            };
            w.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0)");
            try
            {
                s = w.DownloadString(url);
            }
            catch (Exception)
            {

                MessageBox.Show("Fehler!\nKann Datei: " + url +
                    " nicht downloaden!\nBitte überprüfen Sie Ihre Internetverbindung");

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

        private static bool GeneriereViewIndex(int i, string hgtPfad)
        {
            bool ergebnis = false;

            if (!File.Exists(hgtPfad + @"\viewindex" + i + ".xml"))
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  "
                };
                XmlWriter xmlWriter = XmlWriter.Create(hgtPfad + @"\viewindex" + i + ".xml", settings);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("HgtDateien");
                xmlWriter.WriteAttributeString("Quelle", "VIEW" + i);
                string url = "http://www.viewfinderpanoramas.org/Coverage%20map%20viewfinderpanoramas_org" + i + ".htm";

                string[] vs = SammleAreas(url);
                if (vs.Length > 0)
                {
                    ergebnis = true;
                }

                for (int j = 0; j < vs.Length; j++)
                {
                    MatchCollection m1 = Regex.Matches(vs[j], "coords=\"([^\"]*)\"");
                    MatchCollection m2 = Regex.Matches(vs[j], "href=\"([^\"]*)\"");
                    xmlWriter.WriteStartElement("ZipDatei");
                    xmlWriter.WriteAttributeString("Url", m2[0].Groups[1].Value);

                    xmlWriter.WriteElementString("Koordinaten", m1[0].Groups[1].Value);
                    string[] zf = FindeZipFiles(m1[0].Groups[1].Value);
                    for (int k = 0; k < zf.Length; k++)
                    {
                        xmlWriter.WriteElementString("Datei", zf[k]);
                    }
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
                if (!ergebnis)
                {
                    File.Delete(hgtPfad + @"\viewindex" + i + ".xml");
                }
            }
            else
            {
                ergebnis = true;
            }

            if (!Directory.Exists(hgtPfad + @"\VIEW" + i))
            {
                Directory.CreateDirectory(hgtPfad + @"\VIEW" + i);
            }

            if (!Directory.Exists(hgtPfad + @"\noHGT"))
            {
                Directory.CreateDirectory(hgtPfad + @"\noHGT");
            }

            return ergebnis;
        }

        private static string[] FindeZipFiles(string value)
        {
            double viewDimension = 1800.0 / 360.0;
            string lonName;
            string latName;
            string[] vs = value.Split(',');

            int l = int.Parse(vs[0], CultureInfo.CurrentCulture);
            int t = int.Parse(vs[1], CultureInfo.CurrentCulture);
            int r = int.Parse(vs[2], CultureInfo.CurrentCulture);
            int b = int.Parse(vs[3], CultureInfo.CurrentCulture);
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
                        lonName = "W" + (-lon).ToString("D3", CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        lonName = "E" + (lon).ToString("D3", CultureInfo.CurrentCulture);
                    }
                    if (lat < 0)
                    {
                        latName = "S" + (-lat).ToString("D2", CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        latName = "N" + (lat).ToString("D2", CultureInfo.CurrentCulture);
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
            w.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0)");
            string s = "";
            try
            {
                s = w.DownloadString(url);
            }
            catch (Exception)
            {
                MessageBox.Show("Fehler!\nKann Idex nicht erstellen!\nÜberprüfen Sie Ihre Internetverbindung");

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



        private void SRTM_Checked(object sender, RoutedEventArgs e)
        {
            if (SRTM.IsChecked == true)
            {
                usesrtm = true;
            }
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
            {
                LadeHGTFiles.IsEnabled = true;
            }
            else
            {
                LadeHGTFiles.IsEnabled = false;
            }

            GeneriereDirString();
            ZeichneAlles(punkte);
        }

        private void VIEW_Checked(object sender, RoutedEventArgs e)
        {
            if (VIEW.IsChecked == true)
            {
                useview = true;
            }
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
            {
                LadeHGTFiles.IsEnabled = true;
            }
            else
            {
                LadeHGTFiles.IsEnabled = false;
            }

            GeneriereDirString();
            ZeichneAlles(punkte);
        }

        private void GeneriereDirString()
        {
            if (!useview)
            {
                directorys[0] = "";
                directorys[2] = "";
            }
            else
            {
                directorys[0] = "VIEW1";
                directorys[2] = "VIEW3";

            }


            if (!usesrtm)
            {
                directorys[1] = "";
                directorys[3] = "";
            }
            else
            {
                directorys[1] = "SRTM1";
                directorys[3] = "SRTM3";

            }

            if (!use1zoll)
            {
                directorys[0] = "";
                directorys[1] = "";
            }
            else
            {
                if (useview)
                {
                    directorys[0] = "VIEW1";
                }

                if (usesrtm)
                {
                    directorys[1] = "SRTM1";
                }
            }

        }

        private void EinZoll_Checked(object sender, RoutedEventArgs e)
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
            {
                LadeHGTFiles.IsEnabled = true;
            }
            else
            {
                LadeHGTFiles.IsEnabled = false;
            }

            GeneriereDirString();
            ZeichneAlles(punkte);
        }



        private void Weiter2_Click(object sender, RoutedEventArgs e)
        {
            Verarbeitung.IsEnabled = true;
            Verarbeitung.IsSelected = true;
        }

        private void Verarbeitung_GotFocus(object sender, RoutedEventArgs e)
        {

            generiereAnlage.IsEnabled = false;
            Zeichenfläche = Zeichenfläche3;
            Hauptfenster.ResizeMode = ResizeMode.NoResize;
            ZeichneAlles(punkte);

        }






        private void Einlesen_Click(object sender, RoutedEventArgs e)
        {

            hGTConverter = new HGTConverter(hgtPfad, directorys, hgtlinksunten, hgtrechtsoben);
            hGTConverter.SetInterpolationMethod(HGTConverter.InterpolationMethod.Bilinear);
            FülleAnzeigeFläche();
            hGTConverter.SetInterpolationMethod(HGTConverter.InterpolationMethod.Automatic);
            //clZeichneMatrices.Enqueue(new ClZeichneMatrix("mach hin"));
            // LeseEinUndMachWeiter();

        }



        private void BtnAnlagenDirectory_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Bitte Verzeichnis für Anlagen-Dateien auswählen"
            };

            //fbd.RootFolder = Environment.SpecialFolder.Personal;

            fbd.SelectedPath = anlagenpfad;

            System.Windows.Forms.DialogResult result = fbd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                anlagenpfad = fbd.SelectedPath;
                btnGeneriereAnlage.IsEnabled = true;

            }
            fbd.Dispose();

        }



        private void TbAnlagenname_TextChanged(object sender, TextChangedEventArgs e)
        {
            anlagenname = tbAnlagenname.Text;
            if (anlagenname.Length == 0)
            {
                btnAnlagenDirectory.IsEnabled = false;
            }
            else
            {
                btnAnlagenDirectory.IsEnabled = true;
            }
        }
        private void BtnGeneriereAnlage_Click(object sender, RoutedEventArgs e)
        {

            if (cbORM.IsChecked == true)
            {
                SetMaptype(new string[2]);
                GetMaptype()[1] = "ORM";
            }
            else
            {
                SetMaptype(new string[1]);
            }
            GetMaptype()[0] = "OSM";
            if (rbKeinHG.IsChecked == true)
            {
                GetMaptype()[0] = "kein";
            }

            if (rbOSMHG.IsChecked == true)
            {
                GetMaptype()[0] = "OSM";
            }
            if (rbGMHG.IsChecked == true)
            {
                GetMaptype()[0] = "GoM";
            }

            string[] bitmapnamen = { anlagenname + "B.bmp", anlagenname + "F.bmp", anlagenname + "H.bmp", anlagenname + "S.bmp", anlagenname + "T.bmp" };
            int höhe = (int)(zahltbHöheDerAnlage * zahltbRasterdichte);
            int breite = (int)(zahlbreiteDerAnlage * zahltbRasterdichte);
            int rasterdichte = zahltbRasterdichte;
            System.Drawing.Color[] colors = { System.Drawing.Color.FromArgb(255,0, 100, 0) ,
                System.Drawing.Color.FromArgb(255, 200, 200, 200),
               System.Drawing.Color.FromArgb(255, 16, 39, 0),
               System.Drawing.Color.FromArgb(255,0,0,1),
               System.Drawing.Color.FromArgb(255,1, 12, 75) };
            if (rbKeinHG.IsChecked == true)
            {
                colors[4] = System.Drawing.Color.FromArgb(255, 0, 0, 100);
            }

            System.Drawing.Imaging.PixelFormat pixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;




            for (int i = 0; i < bitmapnamen.Length; i++)
            {
                GeneriereEEPBitMap(bitmapnamen[i], höhe, breite, colors[i], pixelFormat); // TODO: Thread erstellen

            }

            using (TemporaryFile temporaryFile = new TemporaryFile())
            {

                File.Copy(anlagenpfad + "\\" + bitmapnamen[1], temporaryFile.Name, true);

                ImageSource imageSrc;

                imageSrc = BitmapFromUri(new Uri(temporaryFile.Name, UriKind.Absolute));

                imageHintergrund.Source = imageSrc;
                imageHintergrund.Stretch = Stretch.Uniform;
            }

            GeneriereBäume(zahltbHöheDerAnlage, zahlbreiteDerAnlage);
            if (rbBaum.IsChecked == true)
            {
                baum = true;
            }

            if (rbPfosten.IsChecked == true)
            {
                pfahl = true;
            }

            zoom = (int)slZoom.Value;




            SchreibeEEPAnlagenDatei(höhe, breite, rasterdichte, baeume, pfahl, baum, zoom);

        }

        public static ImageSource BitmapFromUri(Uri source)
        {
            System.Windows.Media.Imaging.BitmapImage bitmap = new System.Windows.Media.Imaging.BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        private void GeneriereBäume(double zahltbHöheDerAnlage, double zahlbreiteDerAnlage)
        {
            baeume = new int[punkte.Count, 3];
            Matrix drehung = BildeDrehungsMatrix(mittelpunkt.Lon, mittelpunkt.Lat, -winkel);
            GeoPunkt tempPunkt;
            GeoPunkt temppunkt1;
            for (int i = 0; i < punkte.Count; i++)
            {
                tempPunkt = new GeoPunkt(punkte[i].X, punkte[i].Y);
                temppunkt1 = DrehePunkt(tempPunkt, drehung);
                double abshöhe = hGTConverter.GetHoehe(temppunkt1);

                //double abshöhe = ZwspeicherHgt.HöheVonPunkt(temppunkt1);

                double abshöhe2 = ((abshöhe + höhenausgleich) * ausgleichfaktor);
                //if (abshöhe2 < 0)
                //{
                //    int c;
                //}
                int eephöhe = (int)(abshöhe2 * 100);
                baeume[i, 0] = (int)((((punkte[i].X - minLänge) / (maxLänge - minLänge) * zahlbreiteDerAnlage * 1000) - zahlbreiteDerAnlage * 1000 / 2) * 100);
                baeume[i, 1] = (int)((((punkte[i].Y - minBreite) / (maxBreite - minBreite) * zahltbHöheDerAnlage * 1000) - zahltbHöheDerAnlage * 1000 / 2) * 100);
                baeume[i, 2] = eephöhe;
            }

        }

        private void SchreibeEEPAnlagenDatei(int höhe, int breite, int rasterdichte, int[,] baeume, bool pfahl = false, bool baum = false, int zoom = 20)
        {
            SchreibeAnlagenFile af = new SchreibeAnlagenFile(anlagenpfad, anlagenname, höhe, breite, rasterdichte, baeume, pfahl, baum, zoom);
            if (af.SchreibeFile())
            {
                MessageBox.Show("Anlagendatei geschrieben");
            }
            else
            {
                MessageBox.Show("Fehler beim Anlagenscheiben");
            }
        }

        private void GeneriereEEPBitMap(string bitmapnamen, int höhe, int breite, System.Drawing.Color colors, System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(breite, höhe, pixelFormat);
            ZeichneBitMap zeichneBitMap;
            if (bitmapnamen.EndsWith("H.bmp", StringComparison.CurrentCulture) && hGTConverter != null)
            {
                System.Drawing.Color[,] colors1 = new System.Drawing.Color[höhe, breite];
                Matrix drehung = BildeDrehungsMatrix(mittelpunkt.Lon, mittelpunkt.Lat, -winkel);
                GeoPunkt tempPunkt;
                GeoPunkt temppunkt1;
                for (int i = 0; i < höhe; i++)
                {
                    for (int j = 0; j < breite; j++)
                    {
                        tempPunkt = new GeoPunkt(j / (double)breite * (maxLänge - minLänge) + minLänge, i / (double)höhe * (maxBreite - minBreite) + minBreite);
                        temppunkt1 = DrehePunkt(tempPunkt, drehung);
                        double abshöhe = hGTConverter.GetHoehe(temppunkt1);
                        if (abshöhe == 0)
                        {

                        };
                        double abshöhe2 = ((abshöhe + höhenausgleich) * ausgleichfaktor);
                        int eephöhe = (int)Math.Round((abshöhe2 * 100) + 10000);
                        if (eephöhe < 0)
                        {
                            eephöhe = 0;
                        }

                        if (eephöhe == 0)
                        {

                        }
                        if (eephöhe >= 110000)
                        {
                            eephöhe = 109999;
                        }
                        int r1 = eephöhe % 256;
                        int g1 = (eephöhe / 256) % 256;
                        int b1 = (eephöhe / 256 / 256) % 256;



                        colors1[i, j] = System.Drawing.Color.FromArgb(255, r1, g1, b1);
                    }
                }

                zeichneBitMap = new ZeichneBitMap(bitmap, colors1);



            }


            else if (bitmapnamen.EndsWith("F.bmp", StringComparison.CurrentCulture))
            {
                //maptype = new string[2];
                //maptype[0] = "OSM";
                //maptype[1] = "ORM";


                Matrix drehung = BildeDrehungsMatrix(mittelpunkt.Lon, mittelpunkt.Lat, -winkel);
                GeoPunkt tempPunkt;
                GeoPunkt temppunkt1;
                auflösung = (int)Math.Ceiling(Math.Log(40030 * zahltbRasterdichte * Math.Cos(mittelpunkt.Lat / 180 * Math.PI) / 256, 2));
                if (auflösung >= 17)
                {
                    auflösung = 16;
                }

                mapConverter = new MapConverter(pfad + "\\OSM\\", GetMaptype(), hgtlinksunten, hgtrechtsoben, auflösung, mittelpunkt, zahltbRasterdichte);


                System.Drawing.Color[,] colors1 = new System.Drawing.Color[höhe, breite];
                System.Drawing.Bitmap bitmap1 = new System.Drawing.Bitmap(2, 2);
                for (int i = 0; i < höhe; i++)
                {
                    for (int j = 0; j < breite; j++)
                    {
                        tempPunkt = new GeoPunkt(j / (double)breite * (maxLänge - minLänge) + minLänge, i / (double)höhe * (maxBreite - minBreite) + minBreite);
                        temppunkt1 = DrehePunkt(tempPunkt, drehung);

                        colors1[i, j] = mapConverter.GibFarbe(temppunkt1);




                    }
                }

                bitmap1.Dispose();
                zeichneBitMap = new ZeichneBitMap(bitmap, colors1);
            }
            else
            {
                zeichneBitMap = new ZeichneBitMap(bitmap, colors);
            }

            zeichneBitMap.FülleBitmap();

            SpeicherEEPBitMap(bitmapnamen, zeichneBitMap);
            bitmap.Dispose();
        }


        private void SpeicherEEPBitMap(string bitmapnamen, ZeichneBitMap zeichneBitMap)
        {
            SpeicherBild speicherBild = new SpeicherBild(zeichneBitMap.Bitmap, anlagenpfad + "\\" + bitmapnamen);

            speicherBild.Speichern(anlagenpfad + "\\" + bitmapnamen);
        }

        private void GeneriereAnlage_GotFocus(object sender, RoutedEventArgs e)
        {
            tbAnlagenname.Text = anlagenname;
            winkel = winkel % 360;
            lbDrehung.Content = winkel.ToString("N0", CultureInfo.CurrentCulture) + " Grad";
            tbBreiteDerAnlage.Text = zahlbreiteDerAnlage.ToString("N2", CultureInfo.CurrentCulture);
            tbHöheDerAnlage.Text = zahltbHöheDerAnlage.ToString("N2", CultureInfo.CurrentCulture);
            tbRasterDichte.Text = zahltbRasterdichte.ToString("N0", CultureInfo.CurrentCulture);
            if (maximaleHöhe > -10000)
            {
                tbMaxGeländeHöhe.Text = maximaleHöhe.ToString("N0", CultureInfo.CurrentCulture) + " m";
            }

            if (minimaleHöhe < 10000)
            {
                tbMinGeländeHöhe.Text = minimaleHöhe.ToString("N0", CultureInfo.CurrentCulture) + " m";
            }

            AnzeigeHöhenAufLetztemTab();
        }

        private void AnzeigeHöhenAufLetztemTab()
        {
            if (minimaleHöhe < 10000)
            {
                minimaleEEPHöhe = minimaleHöhe + höhenausgleich;
            }

            if (minimaleEEPHöhe < -100 || minimaleEEPHöhe > 1000)
            {
                tbMinEEPHöhe.Background = Brushes.Red;
            }
            else
            {
                tbMinEEPHöhe.Background = Brushes.LightGreen;
            }

            if (maximaleHöhe > -10000)
            {
                maximaleEEPHöhe = (maximaleHöhe + höhenausgleich) * ausgleichfaktor;
            }

            if (maximaleEEPHöhe < -100 || maximaleEEPHöhe > 1000)
            {
                tbMaxEEPHöhe.Background = Brushes.Red;
            }
            else
            {
                tbMaxEEPHöhe.Background = Brushes.LightGreen;
            }

            tbMaxEEPHöhe.Text = maximaleEEPHöhe.ToString("N0", CultureInfo.CurrentCulture) + " m";
            tbMinEEPHöhe.Text = minimaleEEPHöhe.ToString("N0", CultureInfo.CurrentCulture) + " m";
            tbHöhenausgleich.Text = höhenausgleich.ToString("N0", CultureInfo.CurrentCulture);
            tbScalierung.Text = (ausgleichfaktor * 100).ToString("N0", CultureInfo.CurrentCulture);
            tbScalierungEEPBreite.Text = (zahlScalierungEEPBreite * 100).ToString("N0", CultureInfo.CurrentCulture);
            tbScalierungEEPHöhe.Text = (zahlScalierungEEPHöhe * 100).ToString("N0", CultureInfo.CurrentCulture);

        }

        private void BtnIndex_Click(object sender, RoutedEventArgs e)
        {
            GeneriereIndices();
        }




        private void TbBreiteDerAnlage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(tbBreiteDerAnlage.Text, out double test))
            {
                zahlbreiteDerAnlage = test;
                lbKnotenAktuell.Content = ((int)(zahlbreiteDerAnlage * zahltbHöheDerAnlage * zahltbRasterdichte * zahltbRasterdichte)).ToString(CultureInfo.CurrentCulture);

                ÄndereBackgroundKnotenzahl();
            }

        }

        private void ÄndereBackgroundKnotenzahl()
        {
            if ((int)(zahlbreiteDerAnlage * zahltbHöheDerAnlage * zahltbRasterdichte * zahltbRasterdichte) <= 800000)
            {
                lbKnotenAktuell.Background = Brushes.LightGreen;
            }
            else
            if ((int)(zahlbreiteDerAnlage * zahltbHöheDerAnlage * zahltbRasterdichte * zahltbRasterdichte) <= 1000000)
            {
                lbKnotenAktuell.Background = Brushes.LightCyan;
            }
            else
            if ((int)(zahlbreiteDerAnlage * zahltbHöheDerAnlage * zahltbRasterdichte * zahltbRasterdichte) <= 5000000)
            {
                lbKnotenAktuell.Background = Brushes.Yellow;
            }
            else
            {
                lbKnotenAktuell.Background = Brushes.Red;
            }
        }

        private void TbHöheDerAnlage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(tbHöheDerAnlage.Text, out double test))
            {
                zahltbHöheDerAnlage = test;


                lbKnotenAktuell.Content = ((int)(zahlbreiteDerAnlage * zahltbHöheDerAnlage * zahltbRasterdichte * zahltbRasterdichte)).ToString(CultureInfo.CurrentCulture);
                ÄndereBackgroundKnotenzahl();
            }

        }

        private void TbRasterDichte_TextChanged(object sender, TextChangedEventArgs e)
        {
            //tbRasterDichte.Text = zahltbRasterdichte.ToString();
            if (int.TryParse(tbRasterDichte.Text, out int test))
            {


                zahltbRasterdichte = test;

                lbKnotenAktuell.Content = ((int)(zahlbreiteDerAnlage * zahltbHöheDerAnlage * zahltbRasterdichte * zahltbRasterdichte)).ToString(CultureInfo.CurrentCulture);
                ÄndereBackgroundKnotenzahl();
            }
        }

        private void BtWeiter3_Click(object sender, RoutedEventArgs e)
        {
            generiereAnlage.IsEnabled = true;
            AnlagewerteAufTabAnzeigen();
            btWeiter3.IsEnabled = false;
            generiereAnlage.IsSelected = true;
            tbBreiteDerAnlage.IsEnabled = false;
            tbHöheDerAnlage.IsEnabled = false;
            tbScalierung.IsEnabled = true;
            tbHöhenausgleich.IsEnabled = true;
            btnAutoAnpassung.IsEnabled = true;
            btnSkalierungZurücksetzen.IsEnabled = true;
            btEEPHBScalieren.IsEnabled = true;
            btEEPHBzurücksetzen.IsEnabled = true;
            tbScalierungEEPBreite.IsEnabled = true;
            tbScalierungEEPHöhe.IsEnabled = true;
        }

        private void AnlagewerteAufTabAnzeigen()
        {
            zahlbreiteDerAnlage = zahlScalierungEEPBreite * breite2;
            zahltbHöheDerAnlage = zahlScalierungEEPHöhe * hoehe2;
            tbBreiteDerAnlage.Text = Math.Round(zahlbreiteDerAnlage, 2).ToString("N2", CultureInfo.CurrentCulture);
            tbHöheDerAnlage.Text = Math.Round(zahltbHöheDerAnlage, 2).ToString("N2", CultureInfo.CurrentCulture);
            tbScalierungEEPBreite.Text = (zahlScalierungEEPBreite * 100).ToString("N0", CultureInfo.CurrentCulture);
            tbScalierungEEPHöhe.Text = (zahlScalierungEEPHöhe * 100).ToString("N0", CultureInfo.CurrentCulture);
            ÄndereBackgroundKnotenzahl();
        }




        private void BtnAutoAnpassung_Click(object sender, RoutedEventArgs e)
        {
            if ((höhenausgleich == 0) && (ausgleichfaktor == 1))
            {
                höhenausgleich = -1 * minimaleHöhe;
                ausgleichfaktor = ((int)((1000 / (maximaleHöhe - minimaleHöhe)) * 100)) / 100.0;
            }
            else
            {
                höhenausgleich = -1 * (minimaleHöhe + 100);
                ausgleichfaktor = ((int)((1000 / (maximaleHöhe - minimaleHöhe - 100)) * 100)) / 100.0;

            }

            AnzeigeHöhenAufLetztemTab();

        }

        private void BtnSkalierungZurücksetzen_Click(object sender, RoutedEventArgs e)
        {
            höhenausgleich = 0.0;
            ausgleichfaktor = 1.0;
            AnzeigeHöhenAufLetztemTab();
        }

        private void TbScalierung_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(tbScalierung.Text, out double test))
            {
                ausgleichfaktor = test / 100;
                AnzeigeHöhenAufLetztemTab();
            }


        }

        private void TbHöhenausgleich_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(tbHöhenausgleich.Text, out double test))
            {
                höhenausgleich = test;
                AnzeigeHöhenAufLetztemTab();
            }
        }

        private void TbScalierungEEPBreite_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(tbScalierungEEPBreite.Text, out double test))
            {
                zahlScalierungEEPBreite = test / 100;
                AnlagewerteAufTabAnzeigen();
            }

        }

        private void BtnRasterdichte8_Click(object sender, RoutedEventArgs e)
        {
            zahltbRasterdichte = (int)Math.Sqrt(800000.0 / zahlbreiteDerAnlage / zahltbHöheDerAnlage);
            lbKnotenAktuell.Content = ((int)(zahlbreiteDerAnlage * zahltbHöheDerAnlage * zahltbRasterdichte * zahltbRasterdichte)).ToString(CultureInfo.CurrentCulture);
            tbRasterDichte.Text = zahltbRasterdichte.ToString("N0", CultureInfo.CurrentCulture);

            ÄndereBackgroundKnotenzahl();
            //AnlagewerteAufTabAnzeigen();
        }

        private void BtnRasterdichte10_Click(object sender, RoutedEventArgs e)
        {
            zahltbRasterdichte = (int)Math.Sqrt(1000000.0 / zahlbreiteDerAnlage / zahltbHöheDerAnlage);
            lbKnotenAktuell.Content = ((int)(zahlbreiteDerAnlage * zahltbHöheDerAnlage * zahltbRasterdichte * zahltbRasterdichte)).ToString(CultureInfo.CurrentCulture);
            tbRasterDichte.Text = zahltbRasterdichte.ToString("N0", CultureInfo.CurrentCulture);
            ÄndereBackgroundKnotenzahl();
            //AnlagewerteAufTabAnzeigen();
        }

        private void BtnRasterdichte50_Click(object sender, RoutedEventArgs e)
        {
            zahltbRasterdichte = (int)Math.Sqrt(5000000.0 / zahlbreiteDerAnlage / zahltbHöheDerAnlage);
            lbKnotenAktuell.Content = ((int)(zahlbreiteDerAnlage * zahltbHöheDerAnlage * zahltbRasterdichte * zahltbRasterdichte)).ToString(CultureInfo.CurrentCulture);
            tbRasterDichte.Text = zahltbRasterdichte.ToString("N0", CultureInfo.CurrentCulture);
            ÄndereBackgroundKnotenzahl();
            //AnlagewerteAufTabAnzeigen();
        }

        private void OsmDaten_Click(object sender, RoutedEventArgs e)
        {
            if (cbORM.IsChecked == true)
            {
                SetMaptype(new string[2]);
                GetMaptype()[1] = "ORM";
            }
            else
            {
                SetMaptype(new string[1]);
            }
            GetMaptype()[0] = "OSM";
            if (rbKeinHG.IsChecked == true)
            {
                GetMaptype()[0] = "kein";
            }

            if (rbOSMHG.IsChecked == true)
            {
                GetMaptype()[0] = "OSM";
            }
            if (rbGMHG.IsChecked == true)
            {
                GetMaptype()[0] = "GoM";
            }


            auflösung = (int)Math.Ceiling(Math.Log(40030 * zahltbRasterdichte * Math.Cos(mittelpunkt.Lat / 180 * Math.PI) / 256, 2));
            if (auflösung >= 17)
            {
                auflösung = 16;
            }

            if (mapConverter == null)
            {
                mapConverter = new MapConverter(pfad + "\\OSM\\", GetMaptype(), hgtlinksunten, hgtrechtsoben, auflösung, mittelpunkt, zahltbRasterdichte);
            }
            mapConverter.PrepReaders();

            osmDaten.Background = Brushes.LightGreen;
        }

        private void Weiter4_Click(object sender, RoutedEventArgs e)
        {
            tabGenerieren.IsSelected = true;
            tabGenerieren.IsEnabled = true;
        }

        private void rbGMHG_Checked(object sender, RoutedEventArgs e)
        {
            osmDaten.Background = Brushes.Transparent;
        }

        private void rbOSMHG_Checked(object sender, RoutedEventArgs e)
        {
            osmDaten.Background = Brushes.Transparent;

        }

        private void rbKeinHG_Checked(object sender, RoutedEventArgs e)
        {
            osmDaten.Background = Brushes.Transparent;

        }

        private void cbORM_Checked(object sender, RoutedEventArgs e)
        {
            osmDaten.Background = Brushes.Transparent;

        }

        private void cbORM_Unchecked(object sender, RoutedEventArgs e)
        {
            osmDaten.Background = Brushes.Transparent;

        }

        private void TbScalierungEEPHöhe_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(tbScalierungEEPHöhe.Text, out double test))
            {
                zahlScalierungEEPHöhe = test / 100;
                AnlagewerteAufTabAnzeigen();
            }
        }

        private void BtEEPHBScalieren_Click(object sender, RoutedEventArgs e)
        {
            zahlScalierungEEPBreite = ausgleichfaktor;
            zahlScalierungEEPHöhe = ausgleichfaktor;
            AnlagewerteAufTabAnzeigen();
        }

        private void BtEEPHBzurücksetzen_Click(object sender, RoutedEventArgs e)
        {
            zahlScalierungEEPBreite = 1.0;
            zahlScalierungEEPHöhe = 1.0;
            AnlagewerteAufTabAnzeigen();

        }
    }



}

