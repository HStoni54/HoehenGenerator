using System.Collections.Generic;
using System.Windows.Media;

namespace HoehenGenerator
{
    internal class zeichePunkteAufCanvas

    {
        
        SolidColorBrush mySolidColorBrush;
        double punktgröße;
        
        int Lon;
        int Lat;

        public zeichePunkteAufCanvas(SolidColorBrush mySolidColorBrush, double punktgröße,  int lon, int lat)
        {
            
            this.mySolidColorBrush = mySolidColorBrush;
            this.punktgröße = punktgröße;
            
            Lon = lon;
            Lat = lat;
        }

        public SolidColorBrush MySolidColorBrush { get => mySolidColorBrush; set => mySolidColorBrush = value; }
        public double Punktgröße { get => punktgröße; set => punktgröße = value; }
       
        public int Lon1 { get => Lon; set => Lon = value; }
        public int Lat1 { get => Lat; set => Lat = value; }
        
    }

}