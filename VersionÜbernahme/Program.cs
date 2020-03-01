using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace VersionÜbernahme
{
    class Program
    {
        static string version;
        static string revision;
   
        static void Main(string[] args)
        {
            XmlDocument proj = new XmlDocument();
            string pfad = Directory.GetCurrentDirectory();
            //int temp = pfad.LastIndexOf("\\");
            pfad = pfad.Remove(pfad.LastIndexOf("\\"));
            pfad = pfad.Remove(pfad.LastIndexOf("\\"));
            pfad = pfad.Remove(pfad.LastIndexOf("\\"));
            string projektname = pfad + "\\Hoehengenerator\\HoehenGenerator.csproj";
            if (File.Exists(projektname))
            {
                proj.Load(projektname);
                SucheVersion(proj);
                char[] charsToTrim = { '\n', ' ', '\t' };
                revision = revision.Trim(charsToTrim);
                version = version.Trim(charsToTrim);
                version = version.Remove(version.LastIndexOf("."));
                version = version + "." + revision;
            }
            string assemblyname = pfad + "\\HoehenGenerator\\Properties\\AssemblyInfo.cs";
            if (File.Exists(assemblyname))
            {
                string[] vs = File.ReadAllLines(assemblyname);
                for (int i = 0; i < vs.Length; i++)
                {
                    if (vs[i].StartsWith("[assembly: AssemblyVersion("))
                    {
                        vs[i] = "[assembly: AssemblyVersion(\"" + version + "\")]";
                    }
                    if (vs[i].StartsWith("[assembly: AssemblyFileVersion("))
                    {
                        vs[i] = "[assembly: AssemblyFileVersion(\""+ version + "\")]";
                    }
                }
                File.WriteAllLines(assemblyname, vs);
            }
        }
        //string pfad1 = System.IO.Path.GetPathRoot(pfad);

        private static void SucheVersion(XmlNode proj)
        {
            if (!proj.HasChildNodes)
            {
                return;
            }
            for (int i = 0; i < proj.ChildNodes.Count; i++)
            {
                if (proj.ChildNodes[i].Name == "ApplicationVersion")
                {
                    version += " ";
                    version += proj.ChildNodes[i].InnerText;
                }
                if (proj.ChildNodes[i].Name == "ApplicationRevision")
                {
                    revision += " ";
                    revision += proj.ChildNodes[i].InnerText;
                }
                SucheVersion(proj.ChildNodes[i]);
  
            }

        }
    }
}

