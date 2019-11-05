﻿using System.Globalization;
using System.Xml;

namespace HoehenGenerator
{
    internal class SchreibeAnlagenFile
    {
        private readonly string path;
        private readonly string anlagenname;
        private readonly int höhe;
        private readonly int breite;
        private readonly int rasterdichte;
        private double drasterdichte;
        private readonly bool Ok;
        int[,] baeume;

        public SchreibeAnlagenFile(string path, string anlagenname, int höhe, int breite, int rasterdichte,int[,] baeume)
        {
            this.path = path;
            this.anlagenname = anlagenname;
            this.höhe = höhe;
            this.baeume = baeume;
            this.breite = breite;
            this.rasterdichte = rasterdichte;
            Ok = SchreibeFile();
        }

        public bool SchreibeFile()
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true
            };
            drasterdichte = rasterdichte / 100000.0;
            settings.IndentChars = "  ";
            XmlWriter xmlWriter = XmlWriter.Create(path + "\\" + anlagenname + ".anl3", settings);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("sutrackp");
            xmlWriter.WriteStartElement("Gebaeudesammlung");
            xmlWriter.WriteAttributeString("GebaudesammlungID", "5");
            for (int i = 0; i < baeume.Length/3; i++)
            {
                xmlWriter.WriteStartElement("Immobile");
                xmlWriter.WriteAttributeString("gsbname", @"\Lselemente\Flora\Vegetation\Nadel_Baum_02.3dm");
                xmlWriter.WriteAttributeString("ImmoIdx", (i + 1).ToString()); // hier hochzählen
                xmlWriter.WriteAttributeString("TreeShake", "2");
                xmlWriter.WriteStartElement("Dreibein");
                xmlWriter.WriteStartElement("Vektor");
                xmlWriter.WriteAttributeString("x", baeume[i,0].ToString());  // hier Koordinaten und Höhe
                xmlWriter.WriteAttributeString("y", baeume[i,1].ToString());
                xmlWriter.WriteAttributeString("z", baeume[i, 2].ToString());
                xmlWriter.WriteString("Pos");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Vektor");
                xmlWriter.WriteAttributeString("x", "20");  // hier Koordinaten und Höhe
                xmlWriter.WriteAttributeString("y", "0");
                xmlWriter.WriteAttributeString("z", "0");
                xmlWriter.WriteString("Ditr");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Vektor");
                xmlWriter.WriteAttributeString("x", "0");  // hier Koordinaten und Höhe
                xmlWriter.WriteAttributeString("y", "20");
                xmlWriter.WriteAttributeString("z", "0");
                xmlWriter.WriteString("Nor");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Vektor");
                xmlWriter.WriteAttributeString("x", "0");  // hier Koordinaten und Höhe
                xmlWriter.WriteAttributeString("y", "0");
                xmlWriter.WriteAttributeString("z", "20");
                xmlWriter.WriteString("Bin");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Modell");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();

            }
           
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("Schandlaft");
            xmlWriter.WriteAttributeString("extX", breite.ToString(CultureInfo.CurrentCulture));
            xmlWriter.WriteAttributeString("extY", höhe.ToString(CultureInfo.CurrentCulture));
            xmlWriter.WriteAttributeString("dichte", drasterdichte.ToString(CultureInfo.InvariantCulture));
            xmlWriter.WriteAttributeString("HoehenFile", "\\" + anlagenname + "H.bmp");
            xmlWriter.WriteAttributeString("FarbenFile", "\\" + anlagenname + "F.bmp");
            xmlWriter.WriteAttributeString("TexturenFile", "\\" + anlagenname + "T.bmp");
            xmlWriter.WriteAttributeString("TextureBinFile", "\\" + anlagenname + "B.bmp");
            xmlWriter.WriteAttributeString("TextureScaleFile", "\\" + anlagenname + "S.bmp");

            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("Beschreibung");
            xmlWriter.WriteString("Diese Anlage wurde mit dem Höhengenerator erstellt.\nDie Anlagendatei wurde nur mit minimalen Angaben gefüllt.\nZum automatischen Vervollständigen der Angaben die Anlage einmal mit EEP neu speichern.\n\nGruß Holger(HStoni54)");
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return true;

        }
    }
}
