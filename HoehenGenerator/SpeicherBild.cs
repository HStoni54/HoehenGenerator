using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HoehenGenerator
{
    class SpeicherBild
    {

        Bitmap bitmap;
        string dateiname;

        public SpeicherBild(Bitmap bitmap, string dateiname)
        {

            Bitmap = bitmap;
            Dateiname = dateiname;
        }

        public void Speichern(Bitmap bitmap, string dateiname)
        {
            try
            {

                if (dateiname.EndsWith(".bmp"))
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
