using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
                catch (Exception ex) { MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
            }
            else if (Directory.Exists(path))  // OPEN DIRECTORY
            {
                try
                {
                    System.Diagnostics.Process.Start(path);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
            }
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
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
