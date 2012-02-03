namespace GarminFitnessPlugin
{
    partial class GarminFitnessDonationReminderControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GarminFitnessDonationReminderControl));
            this.ExplanationLabel = new System.Windows.Forms.Label();
            this.GarminLogoPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.GarminLogoPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // ExplanationLabel
            // 
            this.ExplanationLabel.Location = new System.Drawing.Point(3, 95);
            this.ExplanationLabel.Name = "ExplanationLabel";
            this.ExplanationLabel.Size = new System.Drawing.Size(416, 306);
            this.ExplanationLabel.TabIndex = 3;
            this.ExplanationLabel.Text = resources.GetString("ExplanationLabel.Text");
            // 
            // GarminLogoPictureBox
            // 
            this.GarminLogoPictureBox.Image = global::GarminFitnessPlugin.Resources.Resources.GarminLogo;
            this.GarminLogoPictureBox.Location = new System.Drawing.Point(6, 4);
            this.GarminLogoPictureBox.Name = "GarminLogoPictureBox";
            this.GarminLogoPictureBox.Size = new System.Drawing.Size(185, 72);
            this.GarminLogoPictureBox.TabIndex = 4;
            this.GarminLogoPictureBox.TabStop = false;
            // 
            // GarminFitnessDonationReminderControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.GarminLogoPictureBox);
            this.Controls.Add(this.ExplanationLabel);
            this.Name = "GarminFitnessDonationReminderControl";
            ((System.ComponentModel.ISupportInitialize)(this.GarminLogoPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label ExplanationLabel;
        private System.Windows.Forms.PictureBox GarminLogoPictureBox;
    }
}
