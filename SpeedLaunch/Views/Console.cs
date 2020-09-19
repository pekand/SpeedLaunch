using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeedLaunch
{
    public partial class Console : Form
    {
        public Console()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public void UpdateText(string text) {
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate () { UpdateText(text); });
                return;
            }

            textBox1.Text = text;
        }

        private void Console_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
