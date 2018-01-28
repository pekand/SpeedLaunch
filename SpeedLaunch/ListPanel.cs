using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedLaunch
{
    public partial class ListPanel : Component
    {
        public List<string> items = new List<string>();

        public ListPanel()
        {
            InitializeComponent();
        }

        public ListPanel(IContainer container)
        {
            container.Add(this);

            InitializeComponent();

        }
    }
}
