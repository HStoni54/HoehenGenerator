using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoehenGenerator
{
    class AufgabeIndices
    {
        string hgtart;
        int auflösung;
        string pfad;

        public AufgabeIndices(string hgtart, int auflösung, string pfad)
        {
            this.hgtart = hgtart;
            this.auflösung = auflösung;
            this.pfad = pfad;
        }

        public string Hgtart { get => hgtart; set => hgtart = value; }
        public int Auflösung { get => auflösung; set => auflösung = value; }
        public string Pfad { get => pfad; set => pfad = value; }
    }
}
