using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Xml;
using System.Xml.Serialization;

namespace XmlTest
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string[] vs = new string[20];
            TestDaten testDaten1 = new TestDaten("Teststring", vs);
       
            for (int i = 0; i < 20; i++)
            {
                testDaten1.Test1[i] = ("Test " + i);
            }
            



            FileStream fs = new FileStream("text.xml", FileMode.Create);
            XmlSerializer x = new XmlSerializer(typeof(TestDaten));
          
            x.Serialize(fs, testDaten1);
            fs.Close();

            write_Xml(testDaten1);
           
        }

        private void write_Xml(TestDaten testDaten1)
        {

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "       ";
            XmlWriter xmlWriter = XmlWriter.Create("test2.xml",settings);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("TestDaten");
            for (int i = 0; i < testDaten1.Test1.Length; i++)
            {
                xmlWriter.WriteElementString("Datensatz",testDaten1.Test1[i]);
                
            }


            xmlWriter.WriteEndElement();


            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("text.xml")) 
            {
                FileStream fs = new FileStream("text.xml", FileMode.Open);
                XmlSerializer x = new XmlSerializer(typeof(TestDaten));
                TestDaten ausgabe = (TestDaten)x.Deserialize(fs);

            }

            if (File.Exists("test2.xml"))
            {
                XmlReader reader = XmlReader.Create(@"test2.xml");
                while (reader.Read())
                {
                    // weitere Anweisungen
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.XmlDeclaration:
                            Console.WriteLine("{0,-20}<{1}>", "DEKLARATION", reader.Value);
                            break;
                        case XmlNodeType.CDATA:
                            Console.WriteLine("{0,-20}{1}", "CDATA", reader.Value);
                            break;
                        case XmlNodeType.Whitespace:
                            Console.WriteLine("{0,-20}", "WHITESPACE");
                            break;
                        case XmlNodeType.Comment:
                            Console.WriteLine("{0,-20}<!--{1}-->", "COMMENT", reader.Value);
                            break;
                        case XmlNodeType.Element:
                            if (reader.IsEmptyElement)
                                Console.WriteLine("{0,-20}<{1} />", "EMPTY_ELEMENT", reader.Name);
                            else
                            {
                                Console.WriteLine("{0,-20}<{1}>", "ELEMENT", reader.Name);
                                // prüfen, ob der Knoten Attribute hat
                                if (reader.HasAttributes)
                                {
                                    // Durch die Attribute navigieren
                                    while (reader.MoveToNextAttribute())
                                    {
                                        Console.WriteLine("{0,-20}{1}",
                                               "ATTRIBUT", reader.Name + "=" + reader.Value);
                                    }
                                }
                            }
                            break;
                        case XmlNodeType.EndElement:
                            Console.WriteLine("{0,-20}</{1}>", "END_ELEMENT", reader.Name);
                            break;
                        case XmlNodeType.Text:
                            Console.WriteLine("{0,-20}{1}", "TEXT", reader.Value);
                            break;
                    }

                }
            }
        }
    }

    public class TestDaten
    {
        string[] test1;
        string test2;

        public TestDaten()
        {

        }
        public TestDaten( string test2, string[] test1)
        {
            Test1 = test1;
            Test2 = test2;
        }

        public string[] Test1 { get => test1; set => test1 = value; }
        public string Test2 { get => test2; set => test2 = value; }
    }

 

}
 
