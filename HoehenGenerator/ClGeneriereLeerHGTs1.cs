﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoehenGenerator
{
    class ClGeneriereLeerHGTs
    {
        string HgtDateiname;

        public ClGeneriereLeerHGTs(string hgtDateiname)
        {
            HgtDateiname = hgtDateiname;
        }

        public string HgtDateiname1 { get => HgtDateiname; set => HgtDateiname = value; }
    }
}
