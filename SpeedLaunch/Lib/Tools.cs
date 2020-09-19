using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SpeedLaunch
{
    public class Tools
    {
        public static void OpenPathInSystem(string path)
        {
            if (File.Exists(path))       // OPEN FILE
            {
                try
                {
                    System.Diagnostics.Process.Start(path);
                }
                catch (Exception ex) {
                    Program.log.write(ex.Message);
                }
            }
            else if (Directory.Exists(path))  // OPEN DIRECTORY
            {
                try
                {
                    System.Diagnostics.Process.Start(path);
                }
                catch (Exception ex) {
                    Program.log.write(ex.Message);
                }
            }
        }

        [DllImport("shell32.dll")]
        static extern IntPtr ExtractAssociatedIcon(IntPtr hInst, StringBuilder lpIconPath, out ushort lpiIcon);

        public static Icon GetIconOldSchool(string fileName)
        {
            ushort uicon;
            StringBuilder strB = new StringBuilder(fileName);
            IntPtr handle = ExtractAssociatedIcon(IntPtr.Zero, strB, out uicon);
            Icon ico = Icon.FromHandle(handle);

            return ico;
        }

        public static Bitmap GetImage(string file)
        {
            try
            {
                Icon ico = Icon.ExtractAssociatedIcon(file);
                return ico.ToBitmap();
            }
            catch (Exception ex)
            {
                Program.log.write(ex.Message);

                try
                {
                    Icon ico2 = GetIconOldSchool(file);
                    return ico2.ToBitmap();
                }
                catch (Exception ex2)
                {
                    Program.log.write(ex2.Message);
                }
            }

            return null;
        }

        public static void RunCommandAndExit(string cmd, string parameters = "")
        {

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
            };

            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + "\"" + cmd + ((parameters != "") ? " " + parameters : "") + "\"";

            process.StartInfo = startInfo;
            process.Start();
        }

        public static void RunCmdAndPreventCloseCommandPromp(string cmd, string parameters = "")
        {

            string strCmdText;
            strCmdText = "/K " + cmd;
            System.Diagnostics.Process.Start("CMD.exe", strCmdText);
        }
    }
}
