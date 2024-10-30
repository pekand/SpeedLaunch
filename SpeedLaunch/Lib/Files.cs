using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedLaunch
{
    public class Files
    {

        public static List<string> ProcessDirectory(string dirPath, List<string> excludeNames, List<string> excludeExtensions, List<string> includeExtensions, bool includeDirectories = false)
        {
            DirectoryInfo dir = new DirectoryInfo(dirPath);

            List<string> output = new List<string>();

            FileInfo[] files = null;
            try
            {
                files = dir.GetFiles();
            }
            catch (UnauthorizedAccessException)
            {
                return output;
            }

            foreach (FileInfo file in files)
            {
                if (excludeNames.Contains(file.Name))
                {
                    continue;
                }

                if (excludeExtensions.Contains(file.Extension.ToLower()))
                {
                    continue;
                }

                if (includeExtensions.Count == 0 || includeExtensions.Contains(file.Extension.ToLower()))
                {
                    output.Add(file.FullName);
                }
            }

            DirectoryInfo[] subDirs = dir.GetDirectories();

            foreach (DirectoryInfo subDir in subDirs)
            {
                if (excludeNames.Contains(subDir.Name))
                {
                    continue;
                }

                if (includeDirectories || includeExtensions.Count == 0)
                {
                    output.Add(subDir.FullName);
                }

                List<string> outputSubdir = ProcessDirectory(subDir.FullName, excludeNames, excludeExtensions, includeExtensions, includeDirectories);

                output.AddRange(outputSubdir);
            }

            return output;
        }
    }
}
