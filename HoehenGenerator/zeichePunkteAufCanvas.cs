using System.Windows.Media;

namespace HoehenGenerator
{
    internal class ZeichePunkteAufCanvas

    {
        public ZeichePunkteAufCanvas(SolidColorBrush mySolidColorBrush, double punktgröße, int lon, int lat)
        {

            MySolidColorBrush = mySolidColorBrush;
            Punktgröße = punktgröße;

            Lon1 = lon;
            Lat1 = lat;
        }

        public SolidColorBrush MySolidColorBrush { get; set; }
        public double Punktgröße { get; set; }

        public int Lon1 { get; set; }
        public int Lat1 { get; set; }

    }

}