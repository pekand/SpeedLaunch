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
        private string configFileName = "config.xml";

        private string indexFileName = "index.xml";

        public static SpeedLaunchView view = null;

        public bool indexIsRebuilded = false;

        private NotifyIcon trayIcon;
       
        // LIST_OF_COMMANDS
        public List<Command> commands = new List<Command>();
        
        // LIST_OF_COMMANDS_INCEX_CACHE
        public List<Index> cache = new List<Index>();

        // SPEEDLAUNCH_CONSTRUCTOR
        public SpeedLaunch()
        {
            loadConfigurationFile();

            if (!this.loadIndexFile()) {
                this.buildIndex();
            }
            
            //CREATE_TRY_ICON
            trayIcon = new NotifyIcon()
            {
                Icon = Resources.SpeedLaunch,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Exit", Exit)                
                }),
                Visible = true,                
            };

            trayIcon.MouseDoubleClick += new MouseEventHandler(this.notifyIcon_Click);

            //REGISTER_KEYBOARD_HOOK
            Hook.registerHook(this.showSpeedLaunch);

            //CREATE_SPEEDLAUNCH_VIEW
            view = new SpeedLaunchView(this);
        }

        // LOAD_CONFIGURATION_FILE
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

            if (Program.isDebugMode)
            {
                this.configFileName = "config.debug.xml";
            }

            string speedlaunchConfigFile = Path.Combine(speedlaunchConfigDirectory, configFileName);

            // CREATE_DEFAULT_CONFIG_FILE
            if (!File.Exists(speedlaunchConfigFile))
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
                Program.log.write(ex.Message);
            }
        }

        // LOAD_COMMAND_CONFIG_FROM_CONFIG_FILE
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
                            Program.log.write(ex.Message);
                        }
                    }
                    commands.Add(c);
                }
            }
        }

        // BUILD_INDEX_FROM_COMMANDS
        public void buildIndex()
        {
            cache.Clear();

            this.indexIsRebuilded = true;

            //BUILD_INDEX_WORKER
            Job.doJob(
                new DoWorkEventHandler(
                    delegate (object o, DoWorkEventArgs args)
                    {
                        foreach (Command command in commands)
                        {
                            // COMMAND_SCAN_DIRECTORY_FOR_FILES
                            if (command.type == "scan_directory_for_files")
                            {
                                List<string> extensions = command.extensions.Split(' ').ToList().ConvertAll(d => d.ToLower());

                                string path = command.path;

                                // COMMAND_SCAN_DIRECTORY_FOR_FILES_PATH_VARIABLES
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

                ProcessDirectory(subdirectory, f, d, level-1);
            }
        }

        // LOAD_CONFIGURATION_FILE
        public bool loadIndexFile()
        {
            cache.Clear();

            // get AppData directory
            string indexFileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // get speedlanch directory in AppData 
            string speedlaunchIndexFileDirectory = Path.Combine(indexFileDirectory, "SpeedLaunch");

            // create speedlaunch config directory if not exists
            if (!Directory.Exists(speedlaunchIndexFileDirectory))
            {
                Directory.CreateDirectory(speedlaunchIndexFileDirectory);
            }

            if (Program.isDebugMode)
            {
                this.indexFileName = "index.debug.xml";
            }

            string indexFile = Path.Combine(speedlaunchIndexFileDirectory, indexFileName);

            if (!File.Exists(indexFile))
            {
                return false;
            }

            XmlReaderSettings xws = new XmlReaderSettings
            {
                CheckCharacters = false
            };

            // load config file
            string xml = File.ReadAllText(indexFile);

            try
            {
                using (XmlReader xr = XmlReader.Create(new StringReader(xml), xws))
                {

                    XElement root = XElement.Load(xr);
                    foreach (XElement element in root.Elements())
                    {
                        if (element.HasElements)
                        {

                            if (element.Name.ToString() == "items") // [options] [config]
                            {
                                this.LoadInnerXmlIndexies(element);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.log.write(ex.Message);
            }

            return true;
        }

        // LOAD_INDEX_CACHE_FILE
        public void LoadInnerXmlIndexies(XElement commandsElement)
        {
            foreach (XElement command in commandsElement.Descendants())
            {

                if (command.Name.ToString() == "item")
                {
                    Index c = new Index();

                    foreach (XElement el in command.Descendants())
                    {
                        try
                        {

                            if (el.Name.ToString() == "text")
                            {
                                c.text = el.Value;
                            }

                            if (el.Name.ToString() == "path")
                            {
                                c.path = el.Value;
                            }

                            if (el.Name.ToString() == "action")
                            {
                                c.action = el.Value;
                            }

                            if (el.Name.ToString() == "priority")
                            {
                                c.priority = Int64.Parse(el.Value);
                            }

                            if (el.Name.ToString() == "runCounter")
                            {
                                c.runCounter = Int64.Parse(el.Value);
                            }
                        }
                        catch (Exception ex)
                        {
                            Program.log.write(ex.Message);
                        }
                    }
                    cache.Add(c);
                }
            }
        }

        // SAVE_INDEX_CACHE_FILE
        public bool saveIndexFile()
        {

            // get AppData directory
            string indexFileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // get speedlanch directory in AppData 
            string speedlaunchIndexFileDirectory = Path.Combine(indexFileDirectory, "SpeedLaunch");

            // create speedlaunch config directory if not exists
            if (!Directory.Exists(speedlaunchIndexFileDirectory))
            {
                Directory.CreateDirectory(speedlaunchIndexFileDirectory);
            }

            if (Program.isDebugMode)
            {
                this.indexFileName = "index.debug.xml";
            }

            string speedlaunchConfigFile = Path.Combine(speedlaunchIndexFileDirectory, indexFileName);

            StringBuilder sb = new StringBuilder();
            XmlWriterSettings xws = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                CheckCharacters = false,
                Indent = true
            };


            XElement root = new XElement("index");

            XElement items = new XElement("items");

            foreach (Index index in this.cache) // filter items from cache
            {
                XElement item = new XElement("item");
                item.Add(new XElement("text", index.text));
                item.Add(new XElement("path", index.path));
                item.Add(new XElement("action", index.action));
                item.Add(new XElement("priority", index.priority));
                item.Add(new XElement("runCounter", index.runCounter));
                items.Add(item);
            }

            root.Add(items);

            try
            {
               
                using (XmlWriter xw = XmlWriter.Create(sb, xws))
                {
                    root.WriteTo(xw);
                }

            }
            catch (Exception ex)
            {
                Program.log.write(ex.Message);
            }


            try
            {
                    System.IO.StreamWriter file = new System.IO.StreamWriter(speedlaunchConfigFile);
                    file.Write(sb.ToString());
                    file.Close();

            }
            catch (System.IO.IOException ex)
            {
                Program.log.write(ex.Message);
            }
            catch (Exception ex)
            {
                Program.log.write(ex.Message);
            }


            return true;
        }

        //SHOW_SPEEDLAUNCH
        private void notifyIcon_Click(object sender, EventArgs e)
        {
            this.showSpeedLaunch();            
        }

        // SHOW_SPEED_LAUNCH
        public void showSpeedLaunch()
        {
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

        //CLOSE_SPEED_LAUNCH
        void Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;

            this.Close();
        }

        public void Close()
        {
            view = null;
            Hook.unregisterHook();

            if (this.indexIsRebuilded) {
                this.saveIndexFile();
            }

            Application.Exit();
        }
    }

}
