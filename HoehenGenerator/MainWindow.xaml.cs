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
        PointCollection punkte = new PointCollection();
        public MainWindow()
        {
            InitializeComponent();
            Title = "Höhengenerator für EEP";
           
        }

        private void LadeDatei_Click(object sender, RoutedEventArgs e)
        {
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
            SuchenNode(ge);
            if(coordinaten != "")
            {
                MessageBox.Show(coordinaten);
                SepariereKoordinaten(coordinaten);
                ZeichnePunkte(punkte);
            }
            // TODO Koordinaten separieren und anzeigen
           
        }

        private void ZeichnePunkte(PointCollection punkte)
        {
            //Zeichenfläche.ActualHeight;

            //Zeichenfläche.ActualWidth;
            Double Größe = Zeichenfläche.ActualHeight;
            if (Zeichenfläche.ActualWidth < Zeichenfläche.ActualHeight)
            {
                Größe = Zeichenfläche.ActualWidth;
            }
            double minLänge = punkte.Min(x => x.X);
            double minBreite = punkte.Min(x => x.Y);
            double maxLänge = punkte.Max(x => x.X);
            double maxBreite = punkte.Max(x => x.Y);
            Zeichenfläche.Children.Clear();
            for (int i = 0; i < punkte.Count; i++)
            {
                Ellipse elli = new Ellipse();
                elli.Width = 5.0;
                elli.Height = 5.0;
                elli.Fill = Brushes.Red;
                Zeichenfläche.Children.Add(elli);
                
                Canvas.SetLeft(elli, Größe / (maxLänge - minLänge) *  (punkte[i].X - minLänge) );
                Canvas.SetBottom(elli, Größe / (maxBreite -minBreite) *  (punkte[i].Y -minBreite) );
            }
        }

        private void SepariereKoordinaten(string coordinaten)
        {
            sepcoordinaten = coordinaten.Split(' ');
            for (int i = 0; i < sepcoordinaten.Length; i++)
            {
                string[] einekoordinate = sepcoordinaten[i].Split(',');
                //CultureInfo culture = new CultureInfo("en-US");
                
                //Point einpunkt = new Point(Double.Parse(einekoordinate[0], culture), Double.Parse(einekoordinate[1], culture));
                punkte.Add( new Point(Double.Parse(einekoordinate[0], CultureInfo.InvariantCulture), Double.Parse(einekoordinate[1], CultureInfo.InvariantCulture)));
            }
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
                ZeichnePunkte(punkte);
            }

        }
    }
}

