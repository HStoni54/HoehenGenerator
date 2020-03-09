using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoehenGenerator
{
    class TemporaryFile : IDisposable
    {
        public string Name
        {get;
            private set;

        }
        public TemporaryFile()
        {
            this.Name = Path.GetTempFileName();
        }
        public void Dispose() => File.Delete(this.Name);
    }
}
