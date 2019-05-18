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
using System.ComponentModel;

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

            Hook.registerHook(this.showSpeedLaunch);

            view = new SpeedLaunchView(this);
        }

        public void showSpeedLaunch() {            
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
            commands.Clear();

            // get AppData directory
            string confidDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // get speedlanch directory in AppData 
            string speedlaunchConfigDirectory = Path.Combine(confidDirectory, "SpeedLaunch");

            // create speedlaunch config directory if not exists
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

            // create default config file if not exits
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
</commands>
</config>";
                File.WriteAllText(speedlaunchConfigFile, config);
            }

            XmlReaderSettings xws = new XmlReaderSettings
            {
                CheckCharacters = false
            };

            // load config file
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
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                            MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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

            Job.doJob(
                new DoWorkEventHandler(
                    delegate (object o, DoWorkEventArgs args)
                    {
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
                                                i.text = Path.GetFileNameWithoutExtension(filePath);
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

                        cache = cache.OrderBy(x => x.priority).ToList();
                    }
                ),
             new RunWorkerCompletedEventHandler(
                    delegate (object o, RunWorkerCompletedEventArgs args)
                    {
                        // complete
                    }
                )
             );

            
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
            this.showSpeedLaunch();            
        }

        void Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;

            this.Close();
        }

        
    }

}
