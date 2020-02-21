using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoehenGenerator
{
    class HGTReader
    {
        private int res;
        public static short UNDEF = short.MinValue;
        private bool read = false;
        public string path;
        private long count;
        private short[] buffer;
        public string fileName;

        public HGTReader(int lat, int lon, string hgtPath, string[] hgtDirectorys)
        {
            string nordsued = lat < 0 ? "S" : "N";
            string ostwest = lon < 0 ? "W" : "E";
            int lat1 = lat < 0 ? -lat : lat;
            int lon1 = lon < 0 ? -lon : lon;
            string baseName =  nordsued + lat1.ToString("D2",CultureInfo.CurrentCulture) + ostwest + lon1.ToString("D3",CultureInfo.CurrentCulture);
            //string baseName = string.Format(CultureInfo.CurrentCulture,"%s%02d%s%03d", basename1 );

            string[] dirs = hgtDirectorys;
            string[] dirs1 = new string[dirs.Length];
            for (int i = 0; i < dirs.Length; i++)
            {
                if (dirs[i].Length > 0 && dirs[i] != "noHGT")
                    dirs1[i] = hgtPath + "\\" + dirs[i];
                else dirs1[i] = "";
            }
            fileName = baseName + ".hgt";
            string fName;
            foreach (string dir in dirs1)
            {
                if (dir.Length > 0   && dir != "noHGT")
                {
                    fName = dir + "\\" + fileName;
                    FileStream fis = File.OpenRead(fName);
                    try
                    {
                        res = CalcRes(fis.Length);
                        if (res >= 0)
                            path = fName;
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    fis.Dispose();
                };
            }

        }

        public int GetRes()
        {
            return res;
        }
        /**
         * HGT files are organised as a matrix of n*n (short) values giving the elevation in metres.
         * Invalid values are coded as 0x8000 = -327678 = Short.MIN_VALUE.
         * @param x index for column west to east 
         * @param y index for row north to south
         * @return the elevation value stored in the file or 0 if 
         */
        public short Ele(int x, int y)
        {
            if (!read && path != null)
            {
                PrepRead();
            }
            if (buffer == null)
                return 0;
            if (x >= 0 && x <= res && y >= 0 && y <= res)
            {


                count++;
                return buffer[( ((res - y) * (res + 1) + x))];
            }
            else return 0;
        }

        public void PrepRead()
        {
            if (!read && path != null)
            {

                //if (count == 0)
                //log.info("allocating buffer for", fileName);
                //else
                //log.warn("re-allocating buffer for", fileName);
                //if (path.endsWith(".zip"))
                //extractFromZip(path, fileName);
                //else
                //{
                byte[] vs = new byte[2];
                FileStream fs = File.OpenRead(path);
                buffer = new short[fs.Length / 2];
                for (int i = 0; i < fs.Length / 2; i++)

                {
                    vs[1] = (byte)fs.ReadByte();
                    vs[0] = (byte)fs.ReadByte();
                    buffer[i] = (short)(256 * vs[1] + vs[0]);
                }
                fs.Dispose();
                read = true;




            }
        }

        /**
        * calculate the resolution of the hgt file. size should be exactly 2 * (res+1) * (res+1) 
        * @param size number of bytes
        * @param fname file name (for error possible message)
        * @return resolution (typically 1200 for 3'' or 3600 for 1'')
        */
        private int CalcRes(long size)
        {
            long numVals = (long)Math.Sqrt(size / 2);
            if (2 * numVals * numVals == size)
                return (int)(numVals - 1);
            //log.error("file", fname, "has unexpected size", size, "and is ignored");
            return -1;
        }

        /**
         * Return memory to GC. 
        * @return true if heap memory was freed.
        */

        public bool FreeBuf()
        {
            if (buffer == null)
                return false;
            buffer = null;
            read = false;
            return true;
        }

    }
}
