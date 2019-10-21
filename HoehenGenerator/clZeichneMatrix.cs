using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoehenGenerator
{
    class clZeichneMatrix
    {
        string macheEs;

        public clZeichneMatrix(string macheEs)
        {
            this.macheEs = macheEs;
        }

        public string MacheEs { get => macheEs; set => macheEs = value; }
    }
}
