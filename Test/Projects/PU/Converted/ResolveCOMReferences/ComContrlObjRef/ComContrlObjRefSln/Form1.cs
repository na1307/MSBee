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
        private AxMSACAL.AxCalendar axCalendar1;
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

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Form1));
            this.axCalendar1 = new AxMSACAL.AxCalendar();
            ((System.ComponentModel.ISupportInitialize)(this.axCalendar1)).BeginInit();
            this.SuspendLayout();
            // 
            // axCalendar1
            // 
            this.axCalendar1.Enabled = true;
            this.axCalendar1.Location = new System.Drawing.Point(0, 8);
            this.axCalendar1.Name = "axCalendar1";
            this.axCalendar1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axCalendar1.OcxState")));
            this.axCalendar1.Size = new System.Drawing.Size(288, 256);
            this.axCalendar1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.axCalendar1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.axCalendar1)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new Form1());
        }
    }
}
