using System;
using System.Collections.Generic;
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
        bool read;
        public string path;
        private long count;
        private short[] buffer;


        public HGTReader(int lat, int lon, string dirsWithHGT)
        {

        }
        public int getRes()
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
        public short ele(int x, int y)
        {
            if (!read && path != null)
            {
                prepRead();
            }
            if (buffer == null)
                return 0;
            if (x >= 0 && x <= res && y >= 0 && y <= res)
            {


                count++;
                return buffer[(2 * ((res - y) * (res + 1) + x))];
            }
            else return 0;
        }

        public void prepRead()
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
                buffer = new short[fs.Length/2];
                for (int i = 0; i < fs.Length/2; i++)

                {
                    vs[1] = (byte)fs.ReadByte();
                    vs[0] = (byte)fs.ReadByte();
                    buffer[i] = (short)(256 * vs[1] + vs[0]);
                }
                fs.Dispose();
                read = true;




            }
        }

    }
}
