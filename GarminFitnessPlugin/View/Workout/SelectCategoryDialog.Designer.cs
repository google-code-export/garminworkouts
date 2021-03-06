namespace GarminFitnessPlugin.View
{
    partial class SelectCategoryDialog
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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.SelectCategoryLabel = new System.Windows.Forms.Label();
            this.ActivityCategoryList = new GarminFitnessPlugin.View.AutoExpandTreeList();
            this.UseCategoryForAllCheckBox = new System.Windows.Forms.CheckBox();
            this.OkButton = new System.Windows.Forms.Button();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.UseCategoryForAllCheckBox);
            this.splitContainer.Panel2.Controls.Add(this.OkButton);
            this.splitContainer.Size = new System.Drawing.Size(294, 430);
            this.splitContainer.SplitterDistance = 401;
            this.splitContainer.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.SelectCategoryLabel);
            this.splitContainer1.Panel1MinSize = 20;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ActivityCategoryList);
            this.splitContainer1.Size = new System.Drawing.Size(294, 401);
            this.splitContainer1.SplitterDistance = 39;
            this.splitContainer1.TabIndex = 2;
            // 
            // SelectCategoryLabel
            // 
            this.SelectCategoryLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectCategoryLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SelectCategoryLabel.Location = new System.Drawing.Point(0, 0);
            this.SelectCategoryLabel.Name = "SelectCategoryLabel";
            this.SelectCategoryLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.SelectCategoryLabel.Size = new System.Drawing.Size(294, 39);
            this.SelectCategoryLabel.TabIndex = 1;
            this.SelectCategoryLabel.Text = "Select category for workout ";
            // 
            // ActivityCategoryList
            // 
            this.ActivityCategoryList.BackColor = System.Drawing.Color.Transparent;
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
            this.ActivityCategoryList.NumHeaderRows = ZoneFiveSoftware.Common.Visuals.TreeList.HeaderRows.Auto;
            this.ActivityCategoryList.NumLockedColumns = 0;
            this.ActivityCategoryList.RowAlternatingColors = true;
            this.ActivityCategoryList.RowHotlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            this.ActivityCategoryList.RowHotlightColorText = System.Drawing.SystemColors.HighlightText;
            this.ActivityCategoryList.RowHotlightMouse = true;
            this.ActivityCategoryList.RowSelectedColor = System.Drawing.SystemColors.Highlight;
            this.ActivityCategoryList.RowSelectedColorText = System.Drawing.SystemColors.HighlightText;
            this.ActivityCategoryList.RowSeparatorLines = true;
            this.ActivityCategoryList.ShowLines = false;
            this.ActivityCategoryList.ShowPlusMinus = false;
            this.ActivityCategoryList.Size = new System.Drawing.Size(294, 358);
            this.ActivityCategoryList.TabIndex = 0;
            this.ActivityCategoryList.DoubleClick += new System.EventHandler(this.ActivityCategoryList_DoubleClick);
            // 
            // UseCategoryForAllCheckBox
            // 
            this.UseCategoryForAllCheckBox.AutoSize = true;
            this.UseCategoryForAllCheckBox.Location = new System.Drawing.Point(3, 2);
            this.UseCategoryForAllCheckBox.Name = "UseCategoryForAllCheckBox";
            this.UseCategoryForAllCheckBox.Size = new System.Drawing.Size(163, 17);
            this.UseCategoryForAllCheckBox.TabIndex = 1;
            this.UseCategoryForAllCheckBox.Text = "Use category for all workouts";
            this.UseCategoryForAllCheckBox.UseVisualStyleBackColor = true;
            this.UseCategoryForAllCheckBox.CheckedChanged += new System.EventHandler(this.UseCategoryForAllCheckBox_CheckedChanged);
            // 
            // OkButton
            // 
            this.OkButton.Location = new System.Drawing.Point(219, 2);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 23);
            this.OkButton.TabIndex = 0;
            this.OkButton.Text = "OK";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // SelectCategoryDialog
            // 
            this.AcceptButton = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 430);
            this.Controls.Add(this.splitContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(300, 456);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 456);
            this.Name = "SelectCategoryDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select Category";
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            this.splitContainer.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GarminFitnessPlugin.View.AutoExpandTreeList ActivityCategoryList;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Label SelectCategoryLabel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.CheckBox UseCategoryForAllCheckBox;
    }
}