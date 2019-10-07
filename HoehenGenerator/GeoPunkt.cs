using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoehenGenerator
{
    struct GeoPunkt
    {
        double lon;
        public double Lon
        { get { return lon; } }
        
        double lat;
        double Lat
        { get { return lat; } }

        public GeoPunkt(double lon, double lat)
        {
            this.lat = lat;
            this.lon = lon;

        }

        const double radius = 6371.0; //km
        public double Xgeo
        { get { return radius * Math.Cos(Lat) * Math.Cos(Lon); } }

        public double Ygeo
        { get { return radius * Math.Cos(Lat) * Math.Sin(Lon); } }

        public double Zgeo
        { get { return radius * Math.Sin(Lat); } }

    }
}
