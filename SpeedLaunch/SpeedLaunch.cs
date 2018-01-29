using SpeedLaunch.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Input;

namespace SpeedLaunch
{
    public class SpeedLaunch : ApplicationContext
    {
        public static SpeedLaunchView view = null;

        private NotifyIcon trayIcon;
       
        public List<Command> commands = new List<Command>();
        public List<Index> cache = new List<Index>();

        public SpeedLaunch()
        {
            Hook.registerHook(() => {
                view.Show();
                view.Activate();
                view.WindowState = FormWindowState.Maximized;
                Hook.SetForegroundWindow(view.Handle.ToInt32());
                view.BringToFront();
                view.Focus();

                // move form to active screen
                Screen s = Screen.FromPoint(new Point(Cursor.Position.X, Cursor.Position.Y));
                view.WindowState = FormWindowState.Normal;
                view.Location = new Point(s.WorkingArea.Location.X, s.WorkingArea.Location.Y);
                view.WindowState = FormWindowState.Maximized;
            });

            loadConfigurationFile();
            buildIndex();

            trayIcon = new NotifyIcon()
            {
                Icon = Resources.SpeedLaunch,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Exit", Exit)                
                }),
                Visible = true,                
            };

            trayIcon.MouseDoubleClick += new MouseEventHandler(this.notifyIcon_Click);

            view = new SpeedLaunchView(this);
        }

        public void Close()
        {
            view = null;
            Hook.unregisterHook();
            Application.Exit();            
        }
        
        // CONFIGURATION
        //-----------------------------------------------------------------------------

        public void loadConfigurationFile()
        {
            string confidDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string speedlaunchConfigDirectory = Path.Combine(confidDirectory, "SpeedLaunch");

            if (!Directory.Exists(speedlaunchConfigDirectory))
            {
                Directory.CreateDirectory(speedlaunchConfigDirectory);
            }

            string configFileName = "config.xml";

            if (Program.isDebugMode)
            {
                configFileName = "config.debug.xml";
            }

            string speedlaunchConfigFile = Path.Combine(speedlaunchConfigDirectory, configFileName);

            if (!File.Exists(speedlaunchConfigFile) || Program.isDebugMode)
            {

                string config = @"<config>
<commands>
    <command>
        <name>common_start_menu</name>
        <type>scan_directory_for_files</type>
        <priority>1</priority>
        <path>%COMMON_START_MENU%</path>
        <extensions>lnk</extensions>
        <keywords></keywords>
        <action>open_in_system</action>
    </command>
    <command>
        <name>user_start_menu</name>
        <type>scan_directory_for_files</type>
        <priority>1</priority>
        <path>%START_MENU%</path>
        <extensions>lnk</extensions>
        <keywords></keywords>
        <action>open_in_system</action>
    </command>
    <command>
        <name>diagrams</name>
        <type>scan_directory_for_files</type>
        <priority>1</priority>
        <path>o:\Diagrams\</path>
        <extensions>diagram</extensions>
        <keywords></keywords>
        <action>open_in_system</action>
    </command>
    <command>
        <name>scripts</name>
        <type>scan_directory_for_files</type>
        <priority>1</priority>
        <path>o:\Scripts\</path>
        <extensions>bat exe ps1 py pyw</extensions>
        <keywords></keywords>
        <action>open_in_system</action>
    </command>
</commands>
</config>";
                File.WriteAllText(speedlaunchConfigFile, config);
            }

            XmlReaderSettings xws = new XmlReaderSettings
            {
                CheckCharacters = false
            };

            string xml = File.ReadAllText(speedlaunchConfigFile);

            try
            {
                using (XmlReader xr = XmlReader.Create(new StringReader(xml), xws))
                {

                    XElement root = XElement.Load(xr);
                    foreach (XElement element in root.Elements())
                    {
                        if (element.HasElements)
                        {

                            if (element.Name.ToString() == "commands") // [options] [config]
                            {
                                this.LoadInnerXmlCommands(element);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void LoadInnerXmlCommands(XElement commandsElement)
        {
            foreach (XElement command in commandsElement.Descendants())
            {

                if (command.Name.ToString() == "command")
                {
                    Command c = new Command();

                    foreach (XElement el in command.Descendants())
                    {
                        try
                        {

                            if (el.Name.ToString() == "name")
                            {
                                c.name = el.Value;
                            }

                            if (el.Name.ToString() == "type")
                            {
                                c.type = el.Value;
                            }

                            if (el.Name.ToString() == "priority")
                            {
                                c.priority = Int64.Parse(el.Value);
                            }

                            if (el.Name.ToString() == "path")
                            {
                                c.path = el.Value;
                            }

                            if (el.Name.ToString() == "extensions")
                            {
                                c.extensions = el.Value;
                            }

                            if (el.Name.ToString() == "keywords")
                            {
                                c.keywords = el.Value;
                            }

                            if (el.Name.ToString() == "action")
                            {
                                c.action = el.Value;
                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    commands.Add(c);
                }
            }
        }

        // INDEX
        //-----------------------------------------------------------------------------
        public void buildIndex()
        {
            cache.Clear();

            foreach (Command command in commands)
            {
                if (command.type == "scan_directory_for_files")
                {
                    List<string> extensions = command.extensions.Split(' ').ToList().ConvertAll(d => d.ToLower());

                    string path = command.path;

                    path = path.Replace("%USER_PROFILE%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                    path = path.Replace("%COMMON_START_MENU%", Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));
                    path = path.Replace("%START_MENU%", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));


                    if (Directory.Exists(path))
                    {
                        ProcessDirectory(
                            path,
                            (string filePath) => {
                                string extension = Path.GetExtension(filePath);

                                if (extensions.Contains(extension.ToLower().TrimStart('.')))
                                {
                                    Index i = new Index();
                                    i.text = Path.GetFileName(filePath);
                                    i.path = filePath;
                                    i.action = command.action;
                                    i.priority = command.priority;
                                    cache.Add(i);
                                }

                                return false;
                            },
                            (string directoryPath) => {
                                return false;
                            }
                        );

                    }
                }
            }

            cache = cache.OrderBy(o => o.priority).ToList();
        }

        public delegate bool CallBack(string path);

        public static void ProcessDirectory(string path, CallBack f = null, CallBack d = null, int level = 10)
        {
            if (level == 0)
            {
                return;
            }

            string[] fileEntries = Directory.GetFiles(path);
            foreach (string fileName in fileEntries)
            {
                if (f != null)
                {
                    bool cancel = f(fileName);
                    if (cancel)
                    {
                        return;
                    }
                }
            }


            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(path);
            foreach (string subdirectory in subdirectoryEntries)
            {

                bool cancel = d(subdirectory);
                if (cancel)
                {
                    return;
                }

                ProcessDirectory(subdirectory, f, d, --level);
            }
        }

        // NOTIFICATION ICON
        //-----------------------------------------------------------------------------

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            Screen s = Screen.FromPoint(new Point(Cursor.Position.X, Cursor.Position.Y));
            view.Location = s.WorkingArea.Location;
            view.Show();
            view.Activate();
            view.WindowState = FormWindowState.Maximized;
            Hook.SetForegroundWindow(view.Handle.ToInt32());
            view.BringToFront();
            view.Focus();
        }

        void Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;

            Application.Exit();
        }

        

    }

}
