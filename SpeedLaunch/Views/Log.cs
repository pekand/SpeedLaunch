using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace SpeedLaunch.Views
{
    class Log
    {
        public string text = "";
        public void write(string message) {
            text = text + message + "\r\n";
            Program.console.UpdateText(text);
        }
    }
}
