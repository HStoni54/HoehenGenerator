using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HoehenGenerator
{
    class HGTFile
    {
        readonly int[,] hgtDaten;
        int auflösung;
        string name;
        string dateiname;

        public HGTFile(int auflösung, string dateiname)
        {
            this.auflösung = auflösung;


            this.dateiname = dateiname;
            hgtDaten = new int[auflösung * 1201, auflösung * 1201];
        }
        public int[,] LeseDaten()
        {
            byte[] vs = new byte[2];
            if (File.Exists(dateiname))
            {
                FileStream fs = File.OpenRead(dateiname);
                for (int i = 0; i < auflösung * 1201; i++)
                {
                    for (int j = 0; j < auflösung * 1201; j++)
                    {
                       int vs1 =  fs.Read(vs,0,2);
                        if (vs1 == 2)
                            hgtDaten[i, j] = 256 * vs[1] + vs[0];
                        else
                            MessageBox.Show("HGT-File zu klein");
                    }
                }
                fs.Close();
            }

            return hgtDaten;
        }
        public int[,] HgtDaten => hgtDaten;
        public int Auflösung => auflösung;

        public string Name { get => name; set => name = value; }
    }
}
