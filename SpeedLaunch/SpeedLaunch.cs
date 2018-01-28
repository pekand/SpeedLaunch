using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace SpeedLaunch
{
    public partial class SpeedLaunch : Form
    {

        public int mx = 0;
        public int my = 0;

        List<ListItem> items = new List<ListItem>();
        List<Command> commands = new List<Command>();
        List<Index> cache = new List<Index>();

        

// EVENTS
//-----------------------------------------------------------------------------
        public SpeedLaunch()
        {

            InitializeComponent();

            if (!Program.isDebugMode) {
                this.Hide();
            }


            _hookID = SetHook(_proc);
        }

        private void SpeedLaunch_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.ShowInTaskbar = false;
            this.KeyPreview = true;

            loadConfigurationFile();
            buildIndex();
        }

        private void SpeedLaunch_Activated(object sender, EventArgs e)
        {
            inputBox.Focus();
        }

        private void SpeedLaunch_Shown(object sender, EventArgs e)
        {
            inputBox.Focus();
        }

        private void SpeedLaunch_Resize(object sender, EventArgs e)
        {
            inputBox.Width = (int)(this.Width * 0.5);

            inputBox.Left = (this.Width - inputBox.Width) / 2;


        }

        private void SpeedLaunch_Paint(object sender, PaintEventArgs e)
        {

            Font textFont = new System.Drawing.Font("Arial", 16);
            Font descriptionFont = new System.Drawing.Font("Arial", 8);
            Pen blackPen = new Pen(Color.Black, 1);
            SolidBrush itemBrush = new SolidBrush(Color.DarkGray);
            SolidBrush itemSelectedBrush = new SolidBrush(Color.Gray);
            SolidBrush iconBrush = new SolidBrush(Color.LightGreen);

            foreach (ListItem item in items)
            {

                int il = item.left;
                int it = item.top;
                int iw = item.width;
                int ih = item.heighth;

                Rectangle itemrec = new Rectangle(il, it, iw, ih);

                e.Graphics.FillRectangle(itemBrush, itemrec);

                if (item.selected)
                {
                    e.Graphics.FillRectangle(itemSelectedBrush, itemrec);
                }

                
                if (item.index.image != null)
                {
                    e.Graphics.DrawImage(item.index.image, new Rectangle(il + 10, it + 10, 50, 50));
                }
                else {
                    e.Graphics.FillRectangle(iconBrush, new Rectangle(il + 10, it + 10, 50, 50));
                }


                e.Graphics.DrawString(item.text, textFont, Brushes.Black, il + 70, it + 10);
                e.Graphics.DrawString(item.description, descriptionFont, Brushes.Black, il + 70, it + 30);
            }


        }

        private void SpeedLaunch_MouseMove(object sender, MouseEventArgs e)
        {
            mx = e.X;
            my = e.Y;

            foreach (ListItem item in items)
            {

                if (item.left <= mx && mx <= item.left + item.width && item.top <= my && my <= item.top + item.heighth)
                {
                    selectItem(item);
                    Invalidate();
                    return;
                }
            }
        }

        private void SpeedLaunch_MouseClick(object sender, MouseEventArgs e)
        {
            mx = e.X;
            my = e.Y;

            foreach (ListItem item in items)
            {

                if (item.left <= mx && mx <= item.left + item.width && item.top <= my && my <= item.top + item.heighth)
                {
                    selectItem(item);
                    doItem(item);
                    return;
                }
            }
        }

        private void SpeedLaunch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                buildIndex();
            }
        }

        private void SpeedLaunch_FormClosed(object sender, FormClosedEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
        }

// CONFIGURATION
//-----------------------------------------------------------------------------
        public void loadConfigurationFile()
        {
            string confidDirectory =  Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string speedlaunchConfigDirectory = Path.Combine(confidDirectory, "SpeedLaunch");

            if (!Directory.Exists(confidDirectory))
            {
                Directory.CreateDirectory(speedlaunchConfigDirectory);
            }

            string configFileName = "config.xml";

            if (Program.isDebugMode) {
                configFileName = "config.debug.xml";
            }

            string speedlaunchConfigFile = Path.Combine(confidDirectory, configFileName);



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
            items.Clear();
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


                    if (Directory.Exists(path)) {
                        ProcessDirectory(
                            path, 
                            (string filePath) => {
                                string extension = Path.GetExtension(filePath);

                                if (extensions.Contains(extension.ToLower().TrimStart('.'))) {
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
            if (level == 0) {
                return;
            }

            string[] fileEntries = Directory.GetFiles(path);
            foreach (string fileName in fileEntries)
            {
                if (f != null)
                {
                    bool cancel = f(fileName);
                    if (cancel) {
                        return;
                    }
                }
            }


            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(path);
            foreach (string subdirectory in subdirectoryEntries) {

                bool cancel = d(subdirectory);
                if (cancel)
                {
                    return;
                }

                ProcessDirectory(subdirectory, f, d, --level);
            }
        }

// INPUTBOX
//-----------------------------------------------------------------------------
        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Hide();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }

            if (e.KeyCode == Keys.Enter)
            {
                if (inputBox.Text.Trim().ToLower() == ".close")
                {
                    Application.Exit();
                }
                else if (inputBox.Text.Trim().ToLower() == ".exit")
                {
                    Application.Exit();
                }
                else if (inputBox.Text.Trim().ToLower() == ".quit")
                {
                    Application.Exit();
                }
                else {
                    ListItem item = getSelectedItem();
                    if (item != null) {
                        doItem(item);
                    }
                }

                this.Hide();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }

            if (e.KeyCode == Keys.Down)
            {
                selectNextItem();
            }

            if (e.KeyCode == Keys.Up)
            {
                selectPrevItem();
            }

        }

        private void inputBox_TextChanged(object sender, EventArgs e)
        {
            string search = inputBox.Text.Trim().ToLower();
            filterItems(search);
        }

        public void filterItems(string search)
        {
            items.Clear();

            int itemCount = 30;
            foreach (Index index in cache)
            {
                if (index.text.ToLower().Contains(search))
                {

                    ListItem item = new ListItem();
                    item.text = index.text;
                    item.description = index.path;
                    item.index = index;
                    if (index.image == null) {
                        index.image = GetImage(index.path);
                    }                                       
                    items.Add(item);

                    itemCount--;
                }

                if (itemCount == 0)
                {
                    break;
                }
            }

            if (items.Count > 0)
            {
                selectItem(items[0]);
                setItemPositions();
            }

            Invalidate();
        }

        public void setItemPositions()
        {
            int w = this.Width;
            int h = this.Height;

            int l = 100;
            int t = 200;

            int p = 10; //padding

            int i = 0;
            foreach (ListItem item in items)
            {

                int il = l;
                int it = t + i * 70 + p * i;
                int iw = w - 200;
                int ih = 70;

                item.left = il;
                item.top = it;
                item.width = iw;
                item.heighth = ih;

                i++;
            }

            this.Invalidate();
        }

// ITEMS
//-----------------------------------------------------------------------------

        public ListItem getSelectedItem()
        {
            foreach (ListItem item in items)
            {
                if (item.selected) { 
                    return item;
                }
            }

            if (items.Count > 0)
            {
                return items[0];
            }

            return null;
        }

        public void selectItem(ListItem item)
        {
            unselectItems();
            item.selected = true;
        }

        public void selectNextItem()
        {
            bool next = false;
            for (int i =0; i< items.Count; i++) {
                if (items[i].selected) {
                    next = true;
                    continue;
                }

                if (next) {
                    selectItem(items[i]);
                    break;
                }
            }
            this.Invalidate();
        }

        public void selectPrevItem()
        {
            ListItem prev = null;
            for (int i = 0; i < items.Count; i++)
            {
                
                if (items[i].selected)
                {
                    if (prev != null) {
                        selectItem(prev);
                    }
                    break;
                }

                prev = items[i];
            }
            this.Invalidate();
        }

        public void unselectItems()
        {
            foreach (ListItem it in items)
            {
                it.selected = false;
            }
        }

        public void doItem(ListItem item)
        {
            string action = item.index.action;
            string path = item.index.path;

            if ("open_in_system" == action) {
                this.Hide();
                OpenPathInSystem(path);
                inputBox.Text = "";
            }
        }

// NOTIFICATION ICON
//-----------------------------------------------------------------------------

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            Screen s = Screen.FromPoint(new Point(Cursor.Position.X, Cursor.Position.Y));
            this.Location = s.WorkingArea.Location;
            this.Show();
            this.Activate();
            this.WindowState = FormWindowState.Maximized;
            SetForegroundWindow(Handle.ToInt32());
            this.BringToFront();
            this.Focus();
        }

// SYSTEM TOOLS
//-----------------------------------------------------------------------------
        public static void OpenPathInSystem(string path)
        {
            if (File.Exists(path))       // OPEN FILE
            {
                try
                {
                    System.Diagnostics.Process.Start(path);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            else if (Directory.Exists(path))  // OPEN DIRECTORY
            {
                try
                {
                    System.Diagnostics.Process.Start(path);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        public static Bitmap GetImage(string file)
        {
            try
            {
                Icon ico = Icon.ExtractAssociatedIcon(file);
                return ico.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return null;
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
                    Program.SpeedLaunch.Show();
                    Program.SpeedLaunch.Activate();
                    Program.SpeedLaunch.WindowState = FormWindowState.Maximized;
                    SetForegroundWindow(Program.SpeedLaunch.Handle.ToInt32());
                    Program.SpeedLaunch.BringToFront();
                    Program.SpeedLaunch.Focus();

                    // move form to active screen
                    Screen s = Screen.FromPoint(new Point(Cursor.Position.X, Cursor.Position.Y));
                    Program.SpeedLaunch.WindowState = FormWindowState.Normal;
                    Program.SpeedLaunch.Location = new Point(s.WorkingArea.Location.X, s.WorkingArea.Location.Y);
                    Program.SpeedLaunch.WindowState = FormWindowState.Maximized;
                }
                else {
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

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }
    }
}
