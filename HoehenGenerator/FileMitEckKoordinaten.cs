using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoehenGenerator
{
    class FileMitEckKoordinaten
    {
        string name;
        int auflösung;
        int[] linksoben = new int[2];
        int[] rechtsoben = new int[2];
        int[] linksunten = new int[2];
        int[] rechtsunten = new int[2];

        public FileMitEckKoordinaten(string name, int auflösung)
        {
            this.name = name;
            this.auflösung = 3 / auflösung;
            rechtsoben[0] = 1200 * this.auflösung;
            rechtsoben[1] = 1200 * this.auflösung;
            linksoben[0] = 0;
            linksoben[1] = 1200 * this.auflösung;
            rechtsunten[0] = 1200 * this.auflösung;
            rechtsunten[1] = 1200 * this.auflösung;
            linksunten[0] = 0;
            linksunten[1] = 0;




        }

        public string Name { get => name; set => name = value; }
        public int Auflösung { get => auflösung; set => auflösung = value; }
        public int[] Linksoben { get => linksoben; set => linksoben = value; }
        public int[] Rechtsoben { get => rechtsoben; set => rechtsoben = value; }
        public int[] Linksunten { get => linksunten; set => linksunten = value; }
        public int[] Rechtsunten { get => rechtsunten; set => rechtsunten = value; }
    }
}
