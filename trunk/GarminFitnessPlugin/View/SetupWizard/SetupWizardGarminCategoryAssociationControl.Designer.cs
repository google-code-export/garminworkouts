namespace GarminFitnessPlugin.View
{
    partial class SetupWizardGarminCategoryAssociationControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupWizardGarminCategoryAssociationControl));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ActivityCategoryList = new GarminFitnessPlugin.View.AutoExpandTreeList();
            this.CategorySelectionPanel = new System.Windows.Forms.Panel();
            this.GarminCategoriesPanel = new System.Windows.Forms.Panel();
            this.OtherRadioButton = new System.Windows.Forms.RadioButton();
            this.CyclingRadioButton = new System.Windows.Forms.RadioButton();
            this.RunningRadioButton = new System.Windows.Forms.RadioButton();
            this.CustomCategoryRadioButton = new System.Windows.Forms.RadioButton();
            this.ParentCategoryRadioButton = new System.Windows.Forms.RadioButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ExplanationLabel = new System.Windows.Forms.Label();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.CategorySelectionPanel.SuspendLayout();
            this.GarminCategoriesPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(3, 89);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ActivityCategoryList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.CategorySelectionPanel);
            this.splitContainer1.Size = new System.Drawing.Size(513, 155);
            this.splitContainer1.SplitterDistance = 307;
            this.splitContainer1.TabIndex = 6;
            // 
            // ActivityCategoryList
            // 
            this.ActivityCategoryList.AllowDrop = true;
            this.ActivityCategoryList.BackColor = System.Drawing.SystemColors.Window;
            this.ActivityCategoryList.Border = ZoneFiveSoftware.Common.Visuals.ControlBorder.Style.SmallRoundShadow;
            this.ActivityCategoryList.CheckBoxes = false;
            this.ActivityCategoryList.DefaultIndent = 15;
            this.ActivityCategoryList.DefaultRowHeight = -1;
            this.ActivityCategoryList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ActivityCategoryList.DragAutoScrollSize = ((byte)(20));
            this.ActivityCategoryList.HeaderRowHeight = 21;
            this.ActivityCategoryList.Location = new System.Drawing.Point(0, 0);
            this.ActivityCategoryList.MultiSelect = false;
            this.ActivityCategoryList.Name = "ActivityCategoryList";
            this.ActivityCategoryList.NumHeaderRows = ZoneFiveSoftware.Common.Visuals.TreeList.HeaderRows.None;
            this.ActivityCategoryList.NumLockedColumns = 0;
            this.ActivityCategoryList.RowAlternatingColors = true;
            this.ActivityCategoryList.RowHotlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            this.ActivityCategoryList.RowHotlightColorText = System.Drawing.SystemColors.HighlightText;
            this.ActivityCategoryList.RowHotlightMouse = false;
            this.ActivityCategoryList.RowSelectedColor = System.Drawing.SystemColors.Highlight;
            this.ActivityCategoryList.RowSelectedColorText = System.Drawing.SystemColors.HighlightText;
            this.ActivityCategoryList.RowSeparatorLines = true;
            this.ActivityCategoryList.ShowLines = false;
            this.ActivityCategoryList.ShowPlusMinus = false;
            this.ActivityCategoryList.Size = new System.Drawing.Size(307, 155);
            this.ActivityCategoryList.TabIndex = 5;
            this.ActivityCategoryList.TabStop = false;
            this.ActivityCategoryList.SelectedChanged += new System.EventHandler(this.ActivityCategoryList_SelectedChanged);
            // 
            // CategorySelectionPanel
            // 
            this.CategorySelectionPanel.Controls.Add(this.GarminCategoriesPanel);
            this.CategorySelectionPanel.Controls.Add(this.CustomCategoryRadioButton);
            this.CategorySelectionPanel.Controls.Add(this.ParentCategoryRadioButton);
            this.CategorySelectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CategorySelectionPanel.Enabled = false;
            this.CategorySelectionPanel.Location = new System.Drawing.Point(0, 0);
            this.CategorySelectionPanel.Name = "CategorySelectionPanel";
            this.CategorySelectionPanel.Size = new System.Drawing.Size(202, 155);
            this.CategorySelectionPanel.TabIndex = 0;
            // 
            // GarminCategoriesPanel
            // 
            this.GarminCategoriesPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.GarminCategoriesPanel.Controls.Add(this.OtherRadioButton);
            this.GarminCategoriesPanel.Controls.Add(this.CyclingRadioButton);
            this.GarminCategoriesPanel.Controls.Add(this.RunningRadioButton);
            this.GarminCategoriesPanel.Location = new System.Drawing.Point(12, 62);
            this.GarminCategoriesPanel.Name = "GarminCategoriesPanel";
            this.GarminCategoriesPanel.Size = new System.Drawing.Size(187, 90);
            this.GarminCategoriesPanel.TabIndex = 2;
            // 
            // OtherRadioButton
            // 
            this.OtherRadioButton.AutoSize = true;
            this.OtherRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OtherRadioButton.Location = new System.Drawing.Point(3, 64);
            this.OtherRadioButton.Name = "OtherRadioButton";
            this.OtherRadioButton.Size = new System.Drawing.Size(72, 24);
            this.OtherRadioButton.TabIndex = 0;
            this.OtherRadioButton.Text = "Other";
            this.OtherRadioButton.UseVisualStyleBackColor = true;
            this.OtherRadioButton.CheckedChanged += new System.EventHandler(this.OtherRadioButton_CheckedChanged);
            // 
            // CyclingRadioButton
            // 
            this.CyclingRadioButton.AutoSize = true;
            this.CyclingRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CyclingRadioButton.Location = new System.Drawing.Point(3, 34);
            this.CyclingRadioButton.Name = "CyclingRadioButton";
            this.CyclingRadioButton.Size = new System.Drawing.Size(76, 24);
            this.CyclingRadioButton.TabIndex = 0;
            this.CyclingRadioButton.Text = "Biking";
            this.CyclingRadioButton.UseVisualStyleBackColor = true;
            this.CyclingRadioButton.CheckedChanged += new System.EventHandler(this.CyclingRadioButton_CheckedChanged);
            // 
            // RunningRadioButton
            // 
            this.RunningRadioButton.AutoSize = true;
            this.RunningRadioButton.Checked = true;
            this.RunningRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RunningRadioButton.Location = new System.Drawing.Point(3, 4);
            this.RunningRadioButton.Name = "RunningRadioButton";
            this.RunningRadioButton.Size = new System.Drawing.Size(94, 24);
            this.RunningRadioButton.TabIndex = 0;
            this.RunningRadioButton.TabStop = true;
            this.RunningRadioButton.Text = "Running";
            this.RunningRadioButton.UseVisualStyleBackColor = true;
            this.RunningRadioButton.CheckedChanged += new System.EventHandler(this.RunningRadioButton_CheckedChanged);
            // 
            // CustomCategoryRadioButton
            // 
            this.CustomCategoryRadioButton.AutoSize = true;
            this.CustomCategoryRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CustomCategoryRadioButton.Location = new System.Drawing.Point(4, 34);
            this.CustomCategoryRadioButton.Name = "CustomCategoryRadioButton";
            this.CustomCategoryRadioButton.Size = new System.Drawing.Size(196, 24);
            this.CustomCategoryRadioButton.TabIndex = 1;
            this.CustomCategoryRadioButton.Text = "Use custom category";
            this.CustomCategoryRadioButton.UseVisualStyleBackColor = true;
            this.CustomCategoryRadioButton.CheckedChanged += new System.EventHandler(this.CustomCategoryRadioButton_CheckedChanged);
            // 
            // ParentCategoryRadioButton
            // 
            this.ParentCategoryRadioButton.AutoSize = true;
            this.ParentCategoryRadioButton.Checked = true;
            this.ParentCategoryRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ParentCategoryRadioButton.Location = new System.Drawing.Point(4, 4);
            this.ParentCategoryRadioButton.Name = "ParentCategoryRadioButton";
            this.ParentCategoryRadioButton.Size = new System.Drawing.Size(190, 24);
            this.ParentCategoryRadioButton.TabIndex = 0;
            this.ParentCategoryRadioButton.TabStop = true;
            this.ParentCategoryRadioButton.Text = "Use parent category";
            this.ParentCategoryRadioButton.UseVisualStyleBackColor = true;
            this.ParentCategoryRadioButton.CheckedChanged += new System.EventHandler(this.ParentCategoryRadioButton_CheckedChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::GarminFitnessPlugin.Properties.Resources.GarminLogo;
            this.pictureBox1.InitialImage = global::GarminFitnessPlugin.Properties.Resources.GarminLogo;
            this.pictureBox1.Location = new System.Drawing.Point(3, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(184, 70);
            this.pictureBox1.TabIndex = 10;
            this.pictureBox1.TabStop = false;
            // 
            // ExplanationLabel
            // 
            this.ExplanationLabel.Location = new System.Drawing.Point(193, 6);
            this.ExplanationLabel.Name = "ExplanationLabel";
            this.ExplanationLabel.Size = new System.Drawing.Size(323, 70);
            this.ExplanationLabel.TabIndex = 9;
            this.ExplanationLabel.Text = resources.GetString("ExplanationLabel.Text");
            // 
            // SetupWizardGarminCategoryAssociationControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.ExplanationLabel);
            this.Name = "SetupWizardGarminCategoryAssociationControl";
            this.Size = new System.Drawing.Size(520, 251);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.CategorySelectionPanel.ResumeLayout(false);
            this.CategorySelectionPanel.PerformLayout();
            this.GarminCategoriesPanel.ResumeLayout(false);
            this.GarminCategoriesPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private AutoExpandTreeList ActivityCategoryList;
        private System.Windows.Forms.Panel CategorySelectionPanel;
        private System.Windows.Forms.Panel GarminCategoriesPanel;
        private System.Windows.Forms.RadioButton OtherRadioButton;
        private System.Windows.Forms.RadioButton CyclingRadioButton;
        private System.Windows.Forms.RadioButton RunningRadioButton;
        private System.Windows.Forms.RadioButton CustomCategoryRadioButton;
        private System.Windows.Forms.RadioButton ParentCategoryRadioButton;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label ExplanationLabel;
    }
}
