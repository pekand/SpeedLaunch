using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NCalc;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace SpeedLaunch
{
    public partial class SpeedLaunchView : Form
    {

        SpeedLaunch context = null;

        private List<ListItem> items = new List<ListItem>();

        public int mx = 0;
        public int my = 0;       


// EVENTS
//-----------------------------------------------------------------------------
        public SpeedLaunchView(SpeedLaunch context)
        {
            this.context = context;
            InitializeComponent();
        }

        private void SpeedLaunch_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.ShowInTaskbar = false;
            this.KeyPreview = true;
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

                
                if (item.index != null && item.index.image != null)
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
                items.Clear();
                context.buildIndex();
            }
        }

        private void SpeedLaunch_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void SpeedLaunchView_FormClosing(object sender, FormClosingEventArgs e)
        {
            context.Close();
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

            Regex rMatchCommand = new Regex(@"^\$(.*)$", RegexOptions.IgnoreCase);

            Match matchCommand = rMatchCommand.Match(search);

            if (matchCommand.Success)
            {
                string result = "";
                string command = matchCommand.Groups[1].Captures[0].Value;

                ListItem item = new ListItem();
                item.text = command;
                item.description = "command";                
                item.index = new Index();
                item.index.action = "run_system_command";
                item.index.path = command;
                items.Add(item);
            }

            Regex rMatchExpression = new Regex(@"^=(.*)$", RegexOptions.IgnoreCase);
            
            Match matchExpression = rMatchExpression.Match(search);

            if (matchExpression.Success) {                

                string result = "";
                string exp = matchExpression.Groups[1].Captures[0].Value;
                try
                {
                    if (exp.Trim() != "") { 
                        Expression e = new Expression(exp);
                        result = e.Evaluate().ToString();
                    }
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }

                ListItem item = new ListItem();
                item.text = result;
                item.description = "ncalc";
                item.index = null;
                items.Add(item);
            }

            if (search == "now" || search == "time" || search == "today")
            {

                ListItem item = new ListItem();
                DateTime now = DateTime.Now;
                item.text = now.ToString("yyyy-MM-dd HH:mm:ss, dddd");
                item.description = "date and time";
                item.index = null;
                items.Add(item);
            }

            int itemCount = 30;
            foreach (Index index in context.cache)
            {
                if (index.text.ToLower().Contains(search))
                {

                    ListItem item = new ListItem();
                    item.text = index.text;
                    item.description = index.path;
                    item.index = index;
                    if (index.image == null) {
                        
                        Job.doJob(
                            new DoWorkEventHandler(
                                delegate (object o, DoWorkEventArgs args)
                                {
                                    index.image = Tools.GetImage(index.path);
                                }
                            ),
                            new RunWorkerCompletedEventHandler(
                                delegate (object o, RunWorkerCompletedEventArgs args)
                                {
                                    this.Invalidate();
                                }
                            )
                         );
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
            if (item.index == null) {
                return;
            }

            string action = item.index.action;
            string path = item.index.path;

            if ("open_in_system" == action) {
                this.Hide();
                Tools.OpenPathInSystem(path);
                inputBox.Text = "";
            }

            if ("run_system_command" == action)
            {
                this.Hide();
                Tools.RunCommandAndExit(path);
                inputBox.Text = "";
            }
        }

        public void ShowInfo(string text)
        {
            
            Notification n = new Notification(this);
            n.SetText(text);
        }
    }
}
