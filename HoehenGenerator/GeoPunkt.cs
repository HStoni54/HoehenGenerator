using System;

namespace HoehenGenerator
{
    struct GeoPunkt
    {
        double lon;
        public double Lon
        { get { return lon; } set { lon = value; } }

        double lat;
        public double Lat
        { get { return lat; } set { lat = value; } }
        double entfernung;

        int höhe;
        public int Höhe
        { get { return höhe; } set { höhe = value; } }
        public GeoPunkt(double lon = 0, double lat = 0, double entfernung = 0, int höhe = 0)
        {
            this.lat = lat;
            this.lon = lon;
            this.entfernung = entfernung;
            this.höhe = höhe;


        }

        const double radius = 6371.0; //km

        public static double bogen(double winkelInGrad)
        {
            return winkelInGrad / 180.0 * Math.PI;
        }
        static double grad(double BogenInGrad)
        {
            return BogenInGrad * 180.0 / Math.PI;
        }
        public double Xgeo
        { get { return radius * Math.Cos(bogen(Lat)) * Math.Cos(bogen(Lon)); } }

        public double Ygeo
        { get { return radius * Math.Cos(bogen(Lat)) * Math.Sin(bogen(Lon)); } }

        public double Zgeo
        { get { return radius * Math.Sin(bogen(Lat)); } }

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
                lat = grad(Math.Asin(Z / radius));

            }
            else if (Z > 0)
            { lat = 90; }
            else
            {
                lat = -90;
            }

            if (Math.Abs(lat) != 90)
            {
                lon = grad(Math.Acos(X / radius / Math.Cos(bogen(lat))));
                if (Math.Asin(Y / radius / Math.Cos(bogen(lat))) < 0)
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
