﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeedLaunch
{
    static class Program
    {

#if DEBUG
        public static bool isDebugMode = true;
#else
        public static bool isDebugMode = false;
#endif

        public static SpeedLaunch SpeedLaunch = new SpeedLaunch();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            
            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CustomApplicationContext());
        }
    }
}