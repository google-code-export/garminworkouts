namespace GarminFitnessPlugin.View
{
    partial class DonationReminder
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
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DonationReminder));
            this.RemindLaterButton = new System.Windows.Forms.Button();
            this.StopRemindingButton = new System.Windows.Forms.Button();
            this.ExplanationLabel = new System.Windows.Forms.Label();
            this.DonateLinkLabel = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // RemindLaterButton
            // 
            this.RemindLaterButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.RemindLaterButton.Location = new System.Drawing.Point(75, 251);
            this.RemindLaterButton.Name = "RemindLaterButton";
            this.RemindLaterButton.Size = new System.Drawing.Size(137, 23);
            this.RemindLaterButton.TabIndex = 0;
            this.RemindLaterButton.Text = "Remind me later";
            this.RemindLaterButton.UseVisualStyleBackColor = true;
            // 
            // StopRemindingButton
            // 
            this.StopRemindingButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.StopRemindingButton.Location = new System.Drawing.Point(218, 251);
            this.StopRemindingButton.Name = "StopRemindingButton";
            this.StopRemindingButton.Size = new System.Drawing.Size(137, 23);
            this.StopRemindingButton.TabIndex = 1;
            this.StopRemindingButton.Text = "I don\'t want to donate";
            this.StopRemindingButton.UseVisualStyleBackColor = true;
            // 
            // ExplanationLabel
            // 
            this.ExplanationLabel.Location = new System.Drawing.Point(12, 9);
            this.ExplanationLabel.Name = "ExplanationLabel";
            this.ExplanationLabel.Size = new System.Drawing.Size(406, 174);
            this.ExplanationLabel.TabIndex = 2;
            this.ExplanationLabel.Text = resources.GetString("ExplanationLabel.Text");
            // 
            // DonateLinkLabel
            // 
            this.DonateLinkLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DonateLinkLabel.Image = global::GarminFitnessPlugin.Resources.Resources.DonateImage;
            this.DonateLinkLabel.Location = new System.Drawing.Point(164, 193);
            this.DonateLinkLabel.Name = "DonateLinkLabel";
            this.DonateLinkLabel.Size = new System.Drawing.Size(102, 45);
            this.DonateLinkLabel.TabIndex = 3;
            this.DonateLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DonateLinkLabel.Click += new System.EventHandler(this.DonateLinkLabel_Click);
            // 
            // DonationReminder
            // 
            this.AcceptButton = this.RemindLaterButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.StopRemindingButton;
            this.ClientSize = new System.Drawing.Size(430, 286);
            this.Controls.Add(this.DonateLinkLabel);
            this.Controls.Add(this.ExplanationLabel);
            this.Controls.Add(this.StopRemindingButton);
            this.Controls.Add(this.RemindLaterButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "DonationReminder";
            this.Text = "Garmin Fitness Plugin Donation";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button RemindLaterButton;
        private System.Windows.Forms.Button StopRemindingButton;
        private System.Windows.Forms.Label ExplanationLabel;
        private System.Windows.Forms.LinkLabel DonateLinkLabel;
    }
}