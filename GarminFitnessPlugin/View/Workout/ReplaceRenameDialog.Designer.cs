namespace GarminFitnessPlugin.View
{
    partial class ReplaceRenameDialog
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
            this.ReplaceRenameIntroLabel = new System.Windows.Forms.Label();
            this.ReplacePanel = new System.Windows.Forms.Panel();
            this.ReplaceExplanationLabel = new System.Windows.Forms.Label();
            this.ReplaceLabel = new System.Windows.Forms.Label();
            this.ReplaceArrowPictureBox = new System.Windows.Forms.PictureBox();
            this.RenamePanel = new System.Windows.Forms.Panel();
            this.NewNameTextBox = new GarminFitnessPlugin.View.ExtendedTextBox();
            this.NewNameLabel = new System.Windows.Forms.Label();
            this.RenameExplanationLabel = new System.Windows.Forms.Label();
            this.RenameLabel = new System.Windows.Forms.Label();
            this.RenameArrowPictureBox = new System.Windows.Forms.PictureBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.ReplacePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ReplaceArrowPictureBox)).BeginInit();
            this.RenamePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RenameArrowPictureBox)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ReplaceRenameIntroLabel
            // 
            this.ReplaceRenameIntroLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ReplaceRenameIntroLabel.Location = new System.Drawing.Point(13, 13);
            this.ReplaceRenameIntroLabel.Name = "ReplaceRenameIntroLabel";
            this.ReplaceRenameIntroLabel.Size = new System.Drawing.Size(311, 61);
            this.ReplaceRenameIntroLabel.TabIndex = 0;
            this.ReplaceRenameIntroLabel.Text = "There is already a workout present that has the same name.  You can either replac" +
                "e the current workout or rename the new one.";
            // 
            // ReplacePanel
            // 
            this.ReplacePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ReplacePanel.Controls.Add(this.ReplaceExplanationLabel);
            this.ReplacePanel.Controls.Add(this.ReplaceLabel);
            this.ReplacePanel.Controls.Add(this.ReplaceArrowPictureBox);
            this.ReplacePanel.Location = new System.Drawing.Point(16, 78);
            this.ReplacePanel.Name = "ReplacePanel";
            this.ReplacePanel.Size = new System.Drawing.Size(308, 82);
            this.ReplacePanel.TabIndex = 1;
            this.ReplacePanel.Click += new System.EventHandler(this.ReplacePanel_Click);
            // 
            // ReplaceExplanationLabel
            // 
            this.ReplaceExplanationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ReplaceExplanationLabel.Location = new System.Drawing.Point(4, 31);
            this.ReplaceExplanationLabel.Name = "ReplaceExplanationLabel";
            this.ReplaceExplanationLabel.Size = new System.Drawing.Size(301, 42);
            this.ReplaceExplanationLabel.TabIndex = 2;
            this.ReplaceExplanationLabel.Text = "Selecting this option will overwrite the current workout that has the same name.";
            // 
            // ReplaceLabel
            // 
            this.ReplaceLabel.AutoSize = true;
            this.ReplaceLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReplaceLabel.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.ReplaceLabel.Location = new System.Drawing.Point(30, 4);
            this.ReplaceLabel.Name = "ReplaceLabel";
            this.ReplaceLabel.Size = new System.Drawing.Size(68, 20);
            this.ReplaceLabel.TabIndex = 1;
            this.ReplaceLabel.Text = "Replace";
            // 
            // ReplaceArrowPictureBox
            // 
            this.ReplaceArrowPictureBox.Image = global::GarminFitnessPlugin.Resources.Resources.Arrow;
            this.ReplaceArrowPictureBox.InitialImage = null;
            this.ReplaceArrowPictureBox.Location = new System.Drawing.Point(4, 4);
            this.ReplaceArrowPictureBox.Name = "ReplaceArrowPictureBox";
            this.ReplaceArrowPictureBox.Size = new System.Drawing.Size(20, 20);
            this.ReplaceArrowPictureBox.TabIndex = 0;
            this.ReplaceArrowPictureBox.TabStop = false;
            // 
            // RenamePanel
            // 
            this.RenamePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.RenamePanel.Controls.Add(this.flowLayoutPanel1);
            this.RenamePanel.Controls.Add(this.RenameExplanationLabel);
            this.RenamePanel.Controls.Add(this.RenameLabel);
            this.RenamePanel.Controls.Add(this.RenameArrowPictureBox);
            this.RenamePanel.Location = new System.Drawing.Point(16, 166);
            this.RenamePanel.Name = "RenamePanel";
            this.RenamePanel.Size = new System.Drawing.Size(308, 106);
            this.RenamePanel.TabIndex = 1;
            this.RenamePanel.Click += new System.EventHandler(this.RenamePanel_Click);
            // 
            // NewNameTextBox
            // 
            this.NewNameTextBox.AcceptsReturn = false;
            this.NewNameTextBox.AcceptsTab = false;
            this.NewNameTextBox.BackColor = System.Drawing.Color.White;
            this.NewNameTextBox.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(123)))), ((int)(((byte)(114)))), ((int)(((byte)(108)))));
            this.NewNameTextBox.ButtonImage = null;
            this.NewNameTextBox.Location = new System.Drawing.Point(70, 3);
            this.NewNameTextBox.MaxLength = 15;
            this.NewNameTextBox.Multiline = false;
            this.NewNameTextBox.Name = "NewNameTextBox";
            this.NewNameTextBox.ReadOnly = false;
            this.NewNameTextBox.ReadOnlyColor = System.Drawing.SystemColors.Control;
            this.NewNameTextBox.ReadOnlyTextColor = System.Drawing.SystemColors.ControlLight;
            this.NewNameTextBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.NewNameTextBox.Size = new System.Drawing.Size(181, 20);
            this.NewNameTextBox.TabIndex = 4;
            this.NewNameTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            // 
            // NewNameLabel
            // 
            this.NewNameLabel.AutoSize = true;
            this.NewNameLabel.Location = new System.Drawing.Point(3, 6);
            this.NewNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.NewNameLabel.Name = "NewNameLabel";
            this.NewNameLabel.Size = new System.Drawing.Size(61, 13);
            this.NewNameLabel.TabIndex = 3;
            this.NewNameLabel.Text = "New name:";
            // 
            // RenameExplanationLabel
            // 
            this.RenameExplanationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.RenameExplanationLabel.Location = new System.Drawing.Point(4, 31);
            this.RenameExplanationLabel.Name = "RenameExplanationLabel";
            this.RenameExplanationLabel.Size = new System.Drawing.Size(301, 42);
            this.RenameExplanationLabel.TabIndex = 2;
            this.RenameExplanationLabel.Text = "Selecting this option will use the following name for the workout.";
            // 
            // RenameLabel
            // 
            this.RenameLabel.AutoSize = true;
            this.RenameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RenameLabel.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.RenameLabel.Location = new System.Drawing.Point(30, 4);
            this.RenameLabel.Name = "RenameLabel";
            this.RenameLabel.Size = new System.Drawing.Size(70, 20);
            this.RenameLabel.TabIndex = 1;
            this.RenameLabel.Text = "Rename";
            // 
            // RenameArrowPictureBox
            // 
            this.RenameArrowPictureBox.Image = global::GarminFitnessPlugin.Resources.Resources.Arrow;
            this.RenameArrowPictureBox.InitialImage = null;
            this.RenameArrowPictureBox.Location = new System.Drawing.Point(4, 4);
            this.RenameArrowPictureBox.Name = "RenameArrowPictureBox";
            this.RenameArrowPictureBox.Size = new System.Drawing.Size(20, 20);
            this.RenameArrowPictureBox.TabIndex = 0;
            this.RenameArrowPictureBox.TabStop = false;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.NewNameLabel);
            this.flowLayoutPanel1.Controls.Add(this.NewNameTextBox);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 76);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(302, 24);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // ReplaceRenameDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(336, 286);
            this.Controls.Add(this.RenamePanel);
            this.Controls.Add(this.ReplacePanel);
            this.Controls.Add(this.ReplaceRenameIntroLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ReplaceRenameDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Replace or rename?";
            this.ReplacePanel.ResumeLayout(false);
            this.ReplacePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ReplaceArrowPictureBox)).EndInit();
            this.RenamePanel.ResumeLayout(false);
            this.RenamePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RenameArrowPictureBox)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label ReplaceRenameIntroLabel;
        private System.Windows.Forms.Panel ReplacePanel;
        private System.Windows.Forms.Label ReplaceExplanationLabel;
        private System.Windows.Forms.Label ReplaceLabel;
        private System.Windows.Forms.PictureBox ReplaceArrowPictureBox;
        private System.Windows.Forms.Panel RenamePanel;
        private System.Windows.Forms.Label NewNameLabel;
        private System.Windows.Forms.Label RenameExplanationLabel;
        private System.Windows.Forms.Label RenameLabel;
        private System.Windows.Forms.PictureBox RenameArrowPictureBox;
        private GarminFitnessPlugin.View.ExtendedTextBox NewNameTextBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}