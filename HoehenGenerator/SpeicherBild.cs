using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;

namespace HoehenGenerator
{
    internal class SpeicherBild
    {
        private Bitmap bitmap;
        private string dateiname;

        public SpeicherBild(Bitmap bitmap, string dateiname)
        {

            Bitmap = bitmap;
            Dateiname = dateiname;
        }

        public void Speichern(Bitmap bitmap, string dateiname)
        {
            try
            {

                if (dateiname.EndsWith(".bmp",StringComparison.CurrentCulture))
                {
                    Bitmap.Save(dateiname, ImageFormat.Bmp);
                }
            }
            catch (Exception SaveException) { MessageBox.Show("Datei konnte nicht gespeichert werden!\n\n\nCode:\n" + SaveException.Message); }
        }


        public Bitmap Bitmap { get => bitmap; set => bitmap = value; }
        public string Dateiname { get => dateiname; set => dateiname = value; }
    }


}
