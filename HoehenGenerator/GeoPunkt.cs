using System;

namespace HoehenGenerator
{
    internal struct GeoPunkt
    {
        private double lon;
        public double Lon
        { get { return lon; } set { lon = value; } }

        private double lat;
        public double Lat
        { get { return lat; } set { lat = value; } }

        private double entfernung;
        private short höhe;
        public short Höhe
        { get { return höhe; } set { höhe = value; } }
        public GeoPunkt(double lon = 0, double lat = 0, double entfernung = 0, short höhe = 0)
        {
            this.lat = lat;
            this.lon = lon;
            this.entfernung = entfernung;
            this.höhe = höhe;


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
        { get { return radius * Math.Cos(Bogen(Lat)) * Math.Cos(Bogen(Lon)); } }

        public double Ygeo  // zur Seite
        { get { return radius * Math.Cos(Bogen(Lat)) * Math.Sin(Bogen(Lon)); } }

        public double Zgeo // zum Nordpol
        { get { return radius * Math.Sin(Bogen(Lat)); } }

        public double Entfernung { get => entfernung; set => entfernung = value; }

        static public double BestimmeAbstand(GeoPunkt p1, GeoPunkt p2)
        {
            double skalarprodukt = p1.Xgeo * p2.Xgeo + p1.Ygeo * p2.Ygeo + p1.Zgeo * p2.Zgeo;
            return radius * Math.Acos(skalarprodukt / (radius * radius));
        }
        public void FügeGeopunktEin(double X, double Y, double Z)
        {
            if (Math.Abs(Z) <= radius)
            {
                lat = Grad(Math.Asin(Z / radius));

            }
            else if (Z > 0)
            { lat = 90; }
            else
            {
                lat = -90;
            }

            if (Math.Abs(lat) != 90)
            {
                lon = Grad(Math.Acos(X / radius / Math.Cos(Bogen(lat))));
                if (Math.Asin(Y / radius / Math.Cos(Bogen(lat))) < 0)
                {
                    lon = -lon;
                }



            }
            else
            {
                lon = 0;
            }
        }
    }
}
