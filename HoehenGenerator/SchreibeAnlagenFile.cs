using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HoehenGenerator
{
    class SchreibeAnlagenFile
    {
        string path; 
        string anlagenname; 
        int höhe; 
        int breite; 
        int rasterdichte;
        double drasterdichte;
        bool Ok;

        public SchreibeAnlagenFile(string path, string anlagenname, int höhe, int breite, int rasterdichte)
        {
            this.path = path;
            this.anlagenname = anlagenname;
            this.höhe = höhe;
            
            this.breite = breite;
            this.rasterdichte = rasterdichte;
            Ok = SchreibeFile();
        }

        public bool SchreibeFile()
        {  
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            drasterdichte = rasterdichte / 100000.0;
            settings.IndentChars = "  ";
            XmlWriter xmlWriter = XmlWriter.Create(path + "\\" + anlagenname + ".anl3", settings);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("sutrackp");
            xmlWriter.WriteStartElement("Schandlaft");
            xmlWriter.WriteAttributeString("extX",breite.ToString());
            xmlWriter.WriteAttributeString("extY", höhe.ToString());
            xmlWriter.WriteAttributeString("dichte", drasterdichte.ToString(System.Globalization.CultureInfo.InvariantCulture));
            xmlWriter.WriteAttributeString("HoehenFile", "\\" + anlagenname + "H.bmp");
            xmlWriter.WriteAttributeString("FarbenFile", "\\" + anlagenname + "F.bmp");
            xmlWriter.WriteAttributeString("TexturenFile", "\\" + anlagenname + "T.bmp");
            xmlWriter.WriteAttributeString("TextureBinFile", "\\" + anlagenname + "B.bmp");
            xmlWriter.WriteAttributeString("TextureScaleFile", "\\" + anlagenname + "S.bmp");

            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("Beschreibung");
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return true;

        }
    }
}
