﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;

namespace HoehenGenerator
{
    internal class SpeicherBild
    {
        public SpeicherBild(Bitmap bitmap, string dateiname)
        {

            Bitmap = bitmap;
            Dateiname = dateiname;
        }

        //public void Speichern(Bitmap bitmap, string dateiname)
        public void Speichern(string dateiname)
        {
            try
            {

                if (dateiname.EndsWith(".bmp", StringComparison.CurrentCulture))
                {
                    Bitmap.Save(dateiname, ImageFormat.Bmp);
                }
            }
            catch (Exception SaveException) { MessageBox.Show("Datei konnte nicht gespeichert werden!\n\n\nCode:\n" + SaveException.Message); }
        }


        public Bitmap Bitmap { get; set; }
        public string Dateiname { get; set; }
    }


}
