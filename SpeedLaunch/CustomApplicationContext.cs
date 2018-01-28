using SpeedLaunch.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeedLaunch
{
    public class CustomApplicationContext : ApplicationContext
    {
        private NotifyIcon trayIcon;

        public CustomApplicationContext()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = Resources.SpeedLaunch,
                ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Exit", Exit)
            }),
                Visible = true
            };
        }

        void Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;

            Application.Exit();
        }

    }

}
