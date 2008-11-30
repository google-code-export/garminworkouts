namespace GarminFitnessPlugin.View
{
    partial class SetupWizardSportTracksZonesControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupWizardSportTracksZonesControl));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ExplanationLabel = new System.Windows.Forms.Label();
            this.CadenceZoneComboBox = new System.Windows.Forms.ComboBox();
            this.PowerZoneComboBox = new System.Windows.Forms.ComboBox();
            this.PowerZoneSelectionLabel = new System.Windows.Forms.Label();
            this.CadenceZoneSelectionLabel = new System.Windows.Forms.Label();
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
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // ExplanationLabel
            // 
            this.ExplanationLabel.Location = new System.Drawing.Point(193, 6);
            this.ExplanationLabel.Name = "ExplanationLabel";
            this.ExplanationLabel.Size = new System.Drawing.Size(404, 82);
            this.ExplanationLabel.TabIndex = 4;
            this.ExplanationLabel.Text = resources.GetString("ExplanationLabel.Text");
            // 
            // CadenceZoneComboBox
            // 
            this.CadenceZoneComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CadenceZoneComboBox.FormattingEnabled = true;
            this.CadenceZoneComboBox.Location = new System.Drawing.Point(303, 123);
            this.CadenceZoneComboBox.Name = "CadenceZoneComboBox";
            this.CadenceZoneComboBox.Size = new System.Drawing.Size(294, 21);
            this.CadenceZoneComboBox.TabIndex = 8;
            this.CadenceZoneComboBox.SelectedIndexChanged += new System.EventHandler(this.CadenceZoneComboBox_SelectedIndexChanged);
            // 
            // PowerZoneComboBox
            // 
            this.PowerZoneComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PowerZoneComboBox.FormattingEnabled = true;
            this.PowerZoneComboBox.Location = new System.Drawing.Point(303, 189);
            this.PowerZoneComboBox.Name = "PowerZoneComboBox";
            this.PowerZoneComboBox.Size = new System.Drawing.Size(294, 21);
            this.PowerZoneComboBox.TabIndex = 9;
            this.PowerZoneComboBox.SelectedIndexChanged += new System.EventHandler(this.PowerZoneComboBox_SelectedIndexChanged);
            // 
            // PowerZoneSelectionLabel
            // 
            this.PowerZoneSelectionLabel.AutoSize = true;
            this.PowerZoneSelectionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PowerZoneSelectionLabel.Location = new System.Drawing.Point(3, 166);
            this.PowerZoneSelectionLabel.Name = "PowerZoneSelectionLabel";
            this.PowerZoneSelectionLabel.Size = new System.Drawing.Size(313, 20);
            this.PowerZoneSelectionLabel.TabIndex = 6;
            this.PowerZoneSelectionLabel.Text = "Use this power zone for my workouts :";
            // 
            // CadenceZoneSelectionLabel
            // 
            this.CadenceZoneSelectionLabel.AutoSize = true;
            this.CadenceZoneSelectionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CadenceZoneSelectionLabel.Location = new System.Drawing.Point(3, 100);
            this.CadenceZoneSelectionLabel.Name = "CadenceZoneSelectionLabel";
            this.CadenceZoneSelectionLabel.Size = new System.Drawing.Size(333, 20);
            this.CadenceZoneSelectionLabel.TabIndex = 7;
            this.CadenceZoneSelectionLabel.Text = "Use this cadence zone for my workouts :";
            // 
            // SetupWizardSportTracksZonesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.CadenceZoneComboBox);
            this.Controls.Add(this.PowerZoneComboBox);
            this.Controls.Add(this.PowerZoneSelectionLabel);
            this.Controls.Add(this.CadenceZoneSelectionLabel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.ExplanationLabel);
            this.Name = "SetupWizardSportTracksZonesControl";
            this.Size = new System.Drawing.Size(600, 226);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label ExplanationLabel;
        private System.Windows.Forms.ComboBox CadenceZoneComboBox;
        private System.Windows.Forms.ComboBox PowerZoneComboBox;
        private System.Windows.Forms.Label PowerZoneSelectionLabel;
        private System.Windows.Forms.Label CadenceZoneSelectionLabel;
    }
}
