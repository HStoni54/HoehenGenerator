using System;

namespace HoehenGenerator
{
    internal struct GeoPunkt
    {
        public double Lon { get; set; }
        public double Lat { get; set; }
        public short Höhe { get; set; }
        public GeoPunkt(double lon = 0, double lat = 0, double entfernung = 0, short höhe = 0)
        {
            Lat = lat;
            Lon = lon;
            Entfernung = entfernung;
            Höhe = höhe;


        }

        private const double radius = 6371.0; //km

        public static double Bogen(double winkelInGrad)
        {
            return winkelInGrad / 180.0 * Math.PI;
        }

        private static double Grad(double BogenInGrad)
        {
            return BogenInGrad * 180.0 / Math.PI;
        }
        public double Xgeo // nach Greenwich
=> radius * Math.Cos(Bogen(Lat)) * Math.Cos(Bogen(Lon));

        public double Ygeo  // zur Seite
=> radius * Math.Cos(Bogen(Lat)) * Math.Sin(Bogen(Lon));

        public double Zgeo // zum Nordpol
=> radius * Math.Sin(Bogen(Lat));

        public double Entfernung { get; set; }

        public static double BestimmeAbstand(GeoPunkt p1, GeoPunkt p2)
        {
            double skalarprodukt = p1.Xgeo * p2.Xgeo + p1.Ygeo * p2.Ygeo + p1.Zgeo * p2.Zgeo;
            return radius * Math.Acos(skalarprodukt / (radius * radius));
        }
        public void FügeGeopunktEin(double X, double Y, double Z)
        {
            if (Math.Abs(Z) <= radius)
            {
                Lat = Grad(Math.Asin(Z / radius));

            }
            else if (Z > 0)
            { Lat = 90; }
            else
            {
                Lat = -90;
            }

            if (Math.Abs(Lat) != 90)
            {
                Lon = Grad(Math.Acos(X / radius / Math.Cos(Bogen(Lat))));
                if (Math.Asin(Y / radius / Math.Cos(Bogen(Lat))) < 0)
                {
                    Lon = -Lon;
                }



            }
            else
            {
                Lon = 0;
            }
        }
    }
}
