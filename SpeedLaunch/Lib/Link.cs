using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shell32;
using static System.Windows.Forms.LinkLabel;

namespace SpeedLaunch
{
    public class Link
    {
        public string path = "";
        public string workdir = "";
        public string arguments = "";

        public static bool isLink(string lnkFilePath)
        {
            return lnkFilePath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) && File.Exists(lnkFilePath);
        }
        public static Link extractLink(string lnkFilePath) {

            if (!isLink(lnkFilePath)) {
                return new Link();
            }

            Shell shell = new Shell();
            Folder folder = shell.NameSpace(System.IO.Path.GetDirectoryName(lnkFilePath));
            FolderItem folderItem = folder.ParseName(System.IO.Path.GetFileName(lnkFilePath));

            if (folderItem != null)
            {
                ShellLinkObject link = (ShellLinkObject)folderItem.GetLink;
                Console.WriteLine("Target: " + link.Path);
                Console.WriteLine("Working Directory: " + link.WorkingDirectory);
                Console.WriteLine("Arguments: " + link.Arguments);
                Console.WriteLine("Description: " + link.Description);

                return new Link()
                {
                    path = link.Path,
                    workdir = link.WorkingDirectory,
                    arguments = link.Arguments,
                };
            }

            return new Link();
        }
    }
}
