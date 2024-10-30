using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace SpeedLaunch
{
    public class Log
    {

 		 public string text = "";
         public void write(string message) {
#if DEBUG
            File.AppendAllText(@"app.log", message+"\n");            
#endif
            text = text + message + "\r\n";
        }
    }
}
