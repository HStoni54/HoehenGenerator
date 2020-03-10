using System;
using System.IO;

namespace HoehenGenerator
{
    internal class TemporaryFile : IDisposable
    {
        public string Name
        {
            get;
            private set;

        }
        public TemporaryFile()
        {
            Name = Path.GetTempFileName();
        }
        public void Dispose()
        {
            File.Delete(Name);
        }
    }
}
