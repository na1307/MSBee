// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;

namespace CSWindowsApplication
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class Form1 : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button button1;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public Form1()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
        }
        public void ClickAndSleep()
        {
            button1.PerformClick();
            Thread.Sleep(2000);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(120, 112);
            this.button1.Name = "button1";
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Form1 localForm = new Form1();
            localForm.Show();
            localForm.ClickAndSleep();
            localForm.Close();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            string filename = Environment.GetEnvironmentVariable("SystemRoot") + @"\clock.avi";
            QuartzTypeLib.FilgraphManager graphManager =
                new QuartzTypeLib.FilgraphManager();

            // QueryInterface for the IMediaControl interface:
            QuartzTypeLib.IMediaControl mc =
                (QuartzTypeLib.IMediaControl)graphManager;

            // Call some methods on a COM interface 
            // Pass in file to RenderFile method on COM object. 
            mc.RenderFile(filename);

            // Show file. 
            mc.Run();

        }
    }
}
