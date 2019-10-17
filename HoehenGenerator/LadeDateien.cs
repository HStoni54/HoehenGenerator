using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoehenGenerator
{
    class LadeDateien
    {
        string url;
        string zieldatei;

        public LadeDateien(string url, string zieldatei)
        {
            this.url = url;
            this.zieldatei = zieldatei;
        }

        public string Url { get => url; set => url = value; }
        public string Zieldatei { get => zieldatei; set => zieldatei = value; }
    }
}
