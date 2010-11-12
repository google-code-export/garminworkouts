namespace GarminFitnessPlugin.View
{
    partial class SetupWizardWelcomeControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupWizardWelcomeControl));
            this.WelcomeLabel = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ContinueLabel = new System.Windows.Forms.Label();
            this.FinishLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // WelcomeLabel
            // 
            this.WelcomeLabel.Location = new System.Drawing.Point(194, 6);
            this.WelcomeLabel.Name = "WelcomeLabel";
            this.WelcomeLabel.Size = new System.Drawing.Size(403, 70);
            this.WelcomeLabel.TabIndex = 0;
            this.WelcomeLabel.Text = resources.GetString("WelcomeLabel.Text");
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::GarminFitnessPlugin.Resources.Resources.GarminLogo;
            this.pictureBox1.InitialImage = global::GarminFitnessPlugin.Resources.Resources.GarminLogo;
            this.pictureBox1.Location = new System.Drawing.Point(4, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(184, 70);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // ContinueLabel
            // 
            this.ContinueLabel.AutoSize = true;
            this.ContinueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ContinueLabel.Location = new System.Drawing.Point(3, 100);
            this.ContinueLabel.Name = "ContinueLabel";
            this.ContinueLabel.Size = new System.Drawing.Size(293, 20);
            this.ContinueLabel.TabIndex = 2;
            this.ContinueLabel.Text = "To set up the plugin, click on \"Next\"";
            // 
            // FinishLabel
            // 
            this.FinishLabel.AutoSize = true;
            this.FinishLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FinishLabel.Location = new System.Drawing.Point(3, 140);
            this.FinishLabel.Name = "FinishLabel";
            this.FinishLabel.Size = new System.Drawing.Size(346, 20);
            this.FinishLabel.TabIndex = 2;
            this.FinishLabel.Text = "To set up the plugin later, click on \"Finish\"";
            this.FinishLabel.Visible = false;
            // 
            // SetupWizardWelcomeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.FinishLabel);
            this.Controls.Add(this.ContinueLabel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.WelcomeLabel);
            this.Name = "SetupWizardWelcomeControl";
            this.Size = new System.Drawing.Size(600, 179);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label WelcomeLabel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label ContinueLabel;
        private System.Windows.Forms.Label FinishLabel;
    }
}
