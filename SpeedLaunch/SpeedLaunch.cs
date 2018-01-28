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
            _hookID = SetHook(_proc);

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
            UnhookWindowsHookEx(_hookID);
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
            SetForegroundWindow(view.Handle.ToInt32());
            view.BringToFront();
            view.Focus();
        }

        void Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;

            Application.Exit();
        }

        // HOOKS
        //-----------------------------------------------------------------------------
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public static bool LeftAlt = false;

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                var keyPressed = KeyInterop.KeyFromVirtualKey(vkCode);


                if (keyPressed.ToString() == "LeftAlt")
                {
                    LeftAlt = true;
                }
                else
                if (LeftAlt && keyPressed.ToString() == "Space")
                {
                    view.Show();
                    view.Activate();
                    view.WindowState = FormWindowState.Maximized;
                    SetForegroundWindow(view.Handle.ToInt32());
                    view.BringToFront();
                    view.Focus();

                    // move form to active screen
                    Screen s = Screen.FromPoint(new Point(Cursor.Position.X, Cursor.Position.Y));
                    view.WindowState = FormWindowState.Normal;
                    view.Location = new Point(s.WorkingArea.Location.X, s.WorkingArea.Location.Y);
                    view.WindowState = FormWindowState.Maximized;
                }
                else
                {
                    LeftAlt = false;
                }

            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);

    }

}
