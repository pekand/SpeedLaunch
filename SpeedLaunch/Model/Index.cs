using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedLaunch
{
    public class Index
    {
        public string text = "";
        public string path = "";
        public string action = "";
        public long priority = 0;
        public long runCounter = 0;        
        public Bitmap image = null; // loaded image
        public bool remove = false;
    }
}
