namespace GarminFitnessPlugin.View
{
    partial class SetupWizardCompletedControl
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ExplanationLabel = new System.Windows.Forms.Label();
            this.FinishLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::GarminFitnessPlugin.Properties.Resources.GarminLogo;
            this.pictureBox1.InitialImage = global::GarminFitnessPlugin.Properties.Resources.GarminLogo;
            this.pictureBox1.Location = new System.Drawing.Point(3, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(184, 70);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // ExplanationLabel
            // 
            this.ExplanationLabel.Location = new System.Drawing.Point(193, 6);
            this.ExplanationLabel.Name = "ExplanationLabel";
            this.ExplanationLabel.Size = new System.Drawing.Size(404, 70);
            this.ExplanationLabel.TabIndex = 2;
            this.ExplanationLabel.Text = "You have completed the setup wizard successfully.  The plugin is now available fr" +
                "om the \"Views\" menu.  Note that all the settings are always available in the Gar" +
                "min fitness Plugin \"Settings\" page.";
            // 
            // FinishLabel
            // 
            this.FinishLabel.AutoSize = true;
            this.FinishLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FinishLabel.Location = new System.Drawing.Point(3, 100);
            this.FinishLabel.Name = "FinishLabel";
            this.FinishLabel.Size = new System.Drawing.Size(295, 20);
            this.FinishLabel.TabIndex = 4;
            this.FinishLabel.Text = "To close the wizard click on \"Finish\"";
            // 
            // SetupWizardCompletedControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.FinishLabel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.ExplanationLabel);
            this.Name = "SetupWizardCompletedControl";
            this.Size = new System.Drawing.Size(600, 132);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label ExplanationLabel;
        private System.Windows.Forms.Label FinishLabel;
    }
}
