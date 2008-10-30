namespace GarminFitnessPlugin.View
{
    partial class GarminProfileControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ProfileTabs = new System.Windows.Forms.TabControl();
            this.RunningPage = new System.Windows.Forms.TabPage();
            this.BikingPage = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.ProfileTabs.SuspendLayout();
            this.SuspendLayout();
            // 
            // ProfileTabs
            // 
            this.ProfileTabs.Controls.Add(this.RunningPage);
            this.ProfileTabs.Controls.Add(this.BikingPage);
            this.ProfileTabs.Controls.Add(this.tabPage3);
            this.ProfileTabs.Location = new System.Drawing.Point(0, 4);
            this.ProfileTabs.Name = "ProfileTabs";
            this.ProfileTabs.SelectedIndex = 0;
            this.ProfileTabs.Size = new System.Drawing.Size(516, 312);
            this.ProfileTabs.TabIndex = 0;
            // 
            // RunningPage
            // 
            this.RunningPage.Location = new System.Drawing.Point(4, 22);
            this.RunningPage.Name = "RunningPage";
            this.RunningPage.Padding = new System.Windows.Forms.Padding(3);
            this.RunningPage.Size = new System.Drawing.Size(508, 286);
            this.RunningPage.TabIndex = 0;
            this.RunningPage.Text = "Running";
            this.RunningPage.UseVisualStyleBackColor = true;
            // 
            // BikingPage
            // 
            this.BikingPage.Location = new System.Drawing.Point(4, 22);
            this.BikingPage.Name = "BikingPage";
            this.BikingPage.Padding = new System.Windows.Forms.Padding(3);
            this.BikingPage.Size = new System.Drawing.Size(508, 286);
            this.BikingPage.TabIndex = 1;
            this.BikingPage.Text = "tabPage2";
            this.BikingPage.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(511, 286);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // GarminProfileControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ProfileTabs);
            this.Name = "GarminProfileControl";
            this.Size = new System.Drawing.Size(519, 319);
            this.ProfileTabs.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl ProfileTabs;
        private System.Windows.Forms.TabPage RunningPage;
        private System.Windows.Forms.TabPage BikingPage;
        private System.Windows.Forms.TabPage tabPage3;

    }
}
