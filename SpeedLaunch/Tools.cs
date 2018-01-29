using System;
using System.Drawing;
using System.IO;
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
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            else if (Directory.Exists(path))  // OPEN DIRECTORY
            {
                try
                {
                    System.Diagnostics.Process.Start(path);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
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
                MessageBox.Show(ex.Message);
            }

            return null;
        }

    }
}
