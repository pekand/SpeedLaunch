using SpeedLaunch.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeedLaunch
{
    static class Program
    {
        public static Log log = new Log();
        public static Console console = new Console();

        // IS_DEBUG_MODE
#if DEBUG
        public static bool isDebugMode = true;
#else
        public static bool isDebugMode = false;
#endif


        public static SpeedLaunch speedLaunch = new SpeedLaunch();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(speedLaunch);
        }
    }
}
