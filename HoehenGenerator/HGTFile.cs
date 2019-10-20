using System.IO;
using System.Windows;

namespace HoehenGenerator
{
    class HGTFile
    {
        readonly short[,] hgtDaten;
        int auflösung;
        string name;
        string dateiname;

        public HGTFile(int auflösung, string dateiname)
        {
            this.auflösung = 3 / auflösung;


            this.dateiname = dateiname;
            byte[] vs = new byte[2];
            hgtDaten = new short[this.auflösung * 1200 + 1, this.auflösung * 1200 + 1];
            FileStream fs = File.OpenRead(dateiname);
            for (int i = 0; i < this.auflösung * 1200 + 1; i++)
            {
                for (int j = 0; j < this.auflösung * 1200 + 1; j++)
                {

                    vs[1] = (byte)fs.ReadByte();
                    vs[0] = (byte)fs.ReadByte();
                    hgtDaten[i, j] = (short)(256 * vs[1] + vs[0]);
                }
            }
            fs.Close();
        }
        public void Clear()
        {
            for (int i = 0; i < auflösung * 1200 + 1; i++)
            {
                for (int j = 0; j < auflösung * 1200 + 1; j++)
                {
                    hgtDaten[i, j] = 0;
                }

            }
        }
        public short[,] LeseDaten()
        {
            byte[] vs = new byte[2];
            if (File.Exists(dateiname))
            {
                FileStream fs = File.OpenRead(dateiname);
                //for (int i = auflösung * 1200; i >= 0; i--)
                    for (int i = 0; i < auflösung * 1200 + 1;  i++)
                {
                    for (int j = 0; j < auflösung * 1200 + 1; j++)
                    {

                        vs[1] = (byte)fs.ReadByte();
                        vs[0] = (byte)fs.ReadByte();
                        hgtDaten[i, j] = (short)(256 * vs[1] + vs[0]);
                    }
                }
                fs.Close();
            }

            return hgtDaten;
        }
        public short LeseHöhe(int i, int j)

        {


            return hgtDaten[i, j];
        }
        public short[,] HgtDaten => hgtDaten;
        public int Auflösung => auflösung;

        public string Name { get => name; set => name = value; }
    }
}
