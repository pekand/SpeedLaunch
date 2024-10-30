using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SpeedLaunch
{
    public class Diagram
    {

        public static string RemoveInvalidFileNameChars(string input)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Concat(input.Where(ch => !invalidChars.Contains(ch)));
        }

        public static void CreateDiagram(string name) {
            if (name == null || name.Trim() == "") {
                return;
            }

            string destination = @"c:\Documents\Diagrams\dictionary\";

            if (!Directory.Exists(destination)) {
                return;                
            }

            name = RemoveInvalidFileNameChars(name.Trim()) + ".diagram";
            string path = Path.Combine(destination, name);

            if (!File.Exists(path))
            {
                using (FileStream fs = File.Create(path))
                {
                    fs.Close();
                }
            }

            var psi = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            };

            try
            {
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
