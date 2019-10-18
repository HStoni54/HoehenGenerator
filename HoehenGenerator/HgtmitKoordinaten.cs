﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HoehenGenerator
{
    class HgtmitKoordinaten
    {
        string name;
        int dezLat;
        int dezLon;

        public HgtmitKoordinaten(string name, int dezLat, int dezLon)
        {
            this.name = name;
            this.dezLat = dezLat;
            this.dezLon = dezLon;
        }

        public string Name { get => name; set => name = value; }
        public int DezLat { get => dezLat; set => dezLat = value; }
        public int DezLon { get => dezLon; set => dezLon = value; }
    }
}