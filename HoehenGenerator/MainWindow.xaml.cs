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

namespace HoehenGenerator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        XmlDocument ge = new XmlDocument();
        String coordinaten;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ladeDatei_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Title = "Bitte GoogleEarth Datei auswählen";
            ofd.Filter = "GoogleEarth Dateien|*.kml;*.kmz;";
            if (ofd.ShowDialog() == true)
            {

                string vName = ofd.FileName;
                string name = ofd.SafeFileName;
                String xmlName = vName + ".xml";
                if (vName.EndsWith(".kmz", StringComparison.OrdinalIgnoreCase))
                {
                    Title = "Kmz-Datei";
                    ZipArchive archive = ZipFile.OpenRead(vName);
                    StreamReader p = new StreamReader(archive.Entries[0].Open());
                    ge.Load(p);
                    p.Close();


                    archive.Dispose();

                }
                else
                {
                    Title = "Kml-Datei";

                    ge.Load(vName);
                }
                //MessageBox.Show(name);
            }
            //XmlNode root = ge.LastChild;

            //Display the contents of the child nodes.
            suchenNode(ge);
        }

        private void suchenNode(XmlNode ge)
        {
            if (ge.HasChildNodes)
            {
                for (int i = 0; i < ge.ChildNodes.Count; i++)
                {
                    if (ge.ChildNodes[i].Name == "coordinates")
                    {
                        coordinaten += ge.ChildNodes[i].InnerText;
                    }
                    suchenNode(ge.ChildNodes[i]);
                }
            }
        }
    }
}

