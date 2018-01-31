using System;
using System.Drawing;
using System.Windows.Forms;
using System.Timers;

namespace SpeedLaunch
{
    
    public class Notification : Control
    {
        delegate void SetTextCallback(string text);

        EventHandler resizeEvent = null;
        PaintEventHandler paintEvent = null;
        private System.Timers.Timer timer = null;

        int ticks = 50;

        public Notification(Form form)
        {
            this.Parent = form;
            form.Controls.Add(this);
            InitializeComponent();
            this.Parent.Invalidate();
        }

        private void InitializeComponent()
        {
            resizeEvent = new EventHandler(this.Form1_Resize);
            paintEvent = new PaintEventHandler(this.Form1_Paint);

            this.Parent.Resize += resizeEvent;
            this.Parent.Paint += paintEvent;

            this.SuspendLayout();
            // 
            // panel1
            // 
            this.Location = new System.Drawing.Point(-10, -10);
            this.Name = "DispleyInfo";
            this.Size = new System.Drawing.Size(10, 10);
            this.TabIndex = 0;
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(10, 10);
            this.ResumeLayout(false);

        }

        public void RemoveComponent()
        {
            this.Parent.Resize -= resizeEvent;
            this.Parent.Paint -= paintEvent;
            this.Parent.Invalidate();

            this.Parent.Controls.Remove(this);
        }

        public void SetText(string text)
        {
            this.timer = new System.Timers.Timer(100);
            this.timer.Elapsed += OnTimedEvent;
            this.timer.Start();

            if (this.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
                (Parent as Form).Invalidate();
            }
            else
            {
                this.Text = text;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            Control control = (Control)sender;
        }

        int opacity = 255;

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Font drawFont = new Font("Arial", 20);
            SolidBrush drawBrush = new SolidBrush(Color.FromArgb(opacity, Color.Black));

            Size textSize = TextRenderer.MeasureText(this.Text, drawFont);

            int Left = this.Parent.Width / 2 - textSize.Width / 2;
            int Top = this.Parent.Height / 2 - textSize.Height / 2;

            PointF drawPoint = new PointF(Left, Top);

            e.Graphics.DrawString(this.Text, drawFont, drawBrush, drawPoint);
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            ticks = ticks - 1;

            if (ticks < 20)
            {
                opacity = opacity - 20;
                if (opacity < 0) opacity = 0;

                this.Parent.Invalidate();
            }

            if (ticks == 0) {
                this.timer.Stop();
                this.RemoveComponent();
            }
        }


    }
}
