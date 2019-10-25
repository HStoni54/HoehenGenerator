using System.Globalization;
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
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true
            };
            drasterdichte = rasterdichte / 100000.0;
            settings.IndentChars = "  ";
            XmlWriter xmlWriter = XmlWriter.Create(path + "\\" + anlagenname + ".anl3", settings);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("sutrackp");
            xmlWriter.WriteStartElement("Schandlaft");
            xmlWriter.WriteAttributeString("extX", breite.ToString( CultureInfo.CurrentCulture));
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
