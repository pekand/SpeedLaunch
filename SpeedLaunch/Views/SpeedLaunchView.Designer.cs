namespace SpeedLaunch
{
    partial class SpeedLaunchView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>        
        private void InitializeComponent() // SPEEDLAUNCH_VIEW_INITIALIZECOMPONENT
        {
            this.inputBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // inputBox
            // 
            this.inputBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.inputBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.inputBox.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.inputBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.inputBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.inputBox.Location = new System.Drawing.Point(58, 87);
            this.inputBox.Name = "inputBox";
            this.inputBox.Size = new System.Drawing.Size(541, 73);
            this.inputBox.TabIndex = 0;
            this.inputBox.TextChanged += new System.EventHandler(this.inputBox_TextChanged);
            this.inputBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.inputBox_KeyDown);
            // 
            // SpeedLaunchView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(1008, 588);
            this.Controls.Add(this.inputBox);
            this.DoubleBuffered = true;
            this.Name = "SpeedLaunchView";
            this.Text = "SppedLaunch";
            this.Activated += new System.EventHandler(this.SpeedLaunch_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SpeedLaunchView_FormClosing);
            this.Load += new System.EventHandler(this.SpeedLaunch_Load);
            this.Shown += new System.EventHandler(this.SpeedLaunch_Shown);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.SpeedLaunch_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SpeedLaunch_KeyDown);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.SpeedLaunch_MouseClick);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SpeedLaunch_MouseMove);
            this.Resize += new System.EventHandler(this.SpeedLaunch_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox inputBox;
    }
}

