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
            this.auflösung = auflösung;


            this.dateiname = dateiname;
            hgtDaten = new short[auflösung * 1200 + 1, auflösung * 1200 + 1];
        }
        public short[,] LeseDaten()
        {
            byte[] vs = new byte[2];
            if (File.Exists(dateiname))
            {
                FileStream fs = File.OpenRead(dateiname);
                for (int i = 0; i < auflösung * 1200 + 1; i++)
                {
                    for (int j = 0; j < auflösung * 1200 + 1; j++)
                    {
                        //int vs1 = fs.Read(vs, 0, 2);
                        //if (vs1 == 2)
                        //    hgtDaten[i, j] = 256 * vs[0] + vs[1];
                        //else
                        //    MessageBox.Show("HGT-File zu klein");
                        vs[1] = (byte)fs.ReadByte();
                        vs[0] = (byte)fs.ReadByte();
                        hgtDaten[i, j] = (short)(256 * vs[1] + vs[0]);
                    }
                }
                fs.Close();
            }

            return hgtDaten;
        }
        public short[,] HgtDaten => hgtDaten;
        public int Auflösung => auflösung;

        public string Name { get => name; set => name = value; }
    }
}
