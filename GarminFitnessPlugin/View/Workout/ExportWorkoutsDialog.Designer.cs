namespace GarminFitnessPlugin.View
{
    partial class ExportWorkoutsDialog
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
            this.DirectoryTree = new ZoneFiveSoftware.Common.Visuals.DirectoryTree();
            this.ExportAsLabel = new System.Windows.Forms.Label();
            this.ExportButton = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.FileFormatsComboBox = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // DirectoryTree
            // 
            this.DirectoryTree.BackColor = System.Drawing.Color.Transparent;
            this.DirectoryTree.Border = ZoneFiveSoftware.Common.Visuals.ControlBorder.Style.SmallRoundShadow;
            this.DirectoryTree.CheckBoxes = false;
            this.DirectoryTree.DefaultIndent = 15;
            this.DirectoryTree.DefaultRowHeight = -1;
            this.DirectoryTree.Dock = System.Windows.Forms.DockStyle.Top;
            this.DirectoryTree.HeaderRowHeight = 21;
            this.DirectoryTree.Location = new System.Drawing.Point(0, 0);
            this.DirectoryTree.MultiSelect = false;
            this.DirectoryTree.Name = "DirectoryTree";
            this.DirectoryTree.NumHeaderRows = ZoneFiveSoftware.Common.Visuals.TreeList.HeaderRows.None;
            this.DirectoryTree.NumLockedColumns = 0;
            this.DirectoryTree.RowAlternatingColors = false;
            this.DirectoryTree.RowHotlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            this.DirectoryTree.RowHotlightColorText = System.Drawing.SystemColors.HighlightText;
            this.DirectoryTree.RowHotlightMouse = true;
            this.DirectoryTree.RowSelectedColor = System.Drawing.SystemColors.Highlight;
            this.DirectoryTree.RowSelectedColorText = System.Drawing.SystemColors.HighlightText;
            this.DirectoryTree.RowSeparatorLines = false;
            this.DirectoryTree.SelectedPath = "";
            this.DirectoryTree.ShowLines = false;
            this.DirectoryTree.ShowPlusMinus = true;
            this.DirectoryTree.Size = new System.Drawing.Size(331, 253);
            this.DirectoryTree.TabIndex = 0;
            // 
            // ExportAsLabel
            // 
            this.ExportAsLabel.AutoSize = true;
            this.ExportAsLabel.Location = new System.Drawing.Point(3, 6);
            this.ExportAsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.ExportAsLabel.MaximumSize = new System.Drawing.Size(0, 16);
            this.ExportAsLabel.MinimumSize = new System.Drawing.Size(0, 16);
            this.ExportAsLabel.Name = "ExportAsLabel";
            this.ExportAsLabel.Size = new System.Drawing.Size(57, 16);
            this.ExportAsLabel.TabIndex = 1;
            this.ExportAsLabel.Text = "Export as: ";
            this.ExportAsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ExportButton
            // 
            this.ExportButton.Location = new System.Drawing.Point(169, 296);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(75, 23);
            this.ExportButton.TabIndex = 2;
            this.ExportButton.Text = "Export";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Location = new System.Drawing.Point(250, 296);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 3;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
            // 
            // FileFormatsComboBox
            // 
            this.FileFormatsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FileFormatsComboBox.FormattingEnabled = true;
            this.FileFormatsComboBox.Location = new System.Drawing.Point(66, 3);
            this.FileFormatsComboBox.Name = "FileFormatsComboBox";
            this.FileFormatsComboBox.Size = new System.Drawing.Size(186, 21);
            this.FileFormatsComboBox.TabIndex = 4;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.ExportAsLabel);
            this.flowLayoutPanel1.Controls.Add(this.FileFormatsComboBox);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 259);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(331, 31);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // ExportWorkoutsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 326);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.ExportButton);
            this.Controls.Add(this.DirectoryTree);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportWorkoutsDialog";
            this.Text = "Select Folder";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ZoneFiveSoftware.Common.Visuals.DirectoryTree DirectoryTree;
        private System.Windows.Forms.Label ExportAsLabel;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.ComboBox FileFormatsComboBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}