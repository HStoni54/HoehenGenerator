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
        double entfernung;
 
        public GeoPunkt(double lon, double lat, double entfernung = 0)
        {
            this.lat = lat;
            this.lon = lon;
            this.entfernung = entfernung;


        }

        const double radius = 6371.0; //km

        double bogen(double winkelInGrad)
        {
            return winkelInGrad / 180.0 * Math.PI;
        }
        public double Xgeo
        { get { return radius * Math.Cos(bogen(Lat)) * Math.Cos(bogen(Lon)); } }

        public double Ygeo
        { get { return radius * Math.Cos(bogen(Lat)) * Math.Sin(bogen(Lon)); } }

        public double Zgeo
        { get { return radius * Math.Sin(bogen(Lat)); } }

        public double Entfernung { get => entfernung; set => entfernung = value; }
    }
}
