namespace GarminWorkoutPlugin.View
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
            this.SelectCategoryLabel = new System.Windows.Forms.Label();
            this.ActivityCategoryList = new GarminWorkoutPlugin.View.AutoExpandTreeList();
            this.OkButton = new System.Windows.Forms.Button();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
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
            this.splitContainer.Panel1.Controls.Add(this.SelectCategoryLabel);
            this.splitContainer.Panel1.Controls.Add(this.ActivityCategoryList);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.OkButton);
            this.splitContainer.Size = new System.Drawing.Size(294, 430);
            this.splitContainer.SplitterDistance = 401;
            this.splitContainer.TabIndex = 1;
            // 
            // SelectCategoryLabel
            // 
            this.SelectCategoryLabel.AutoSize = true;
            this.SelectCategoryLabel.Location = new System.Drawing.Point(3, 9);
            this.SelectCategoryLabel.Name = "SelectCategoryLabel";
            this.SelectCategoryLabel.Size = new System.Drawing.Size(140, 13);
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
            this.ActivityCategoryList.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ActivityCategoryList.HeaderRowHeight = 21;
            this.ActivityCategoryList.Location = new System.Drawing.Point(0, 25);
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
            this.ActivityCategoryList.Size = new System.Drawing.Size(294, 376);
            this.ActivityCategoryList.TabIndex = 0;
            this.ActivityCategoryList.DoubleClick += new System.EventHandler(this.ActivityCategoryList_DoubleClick);
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
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Category";
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GarminWorkoutPlugin.View.AutoExpandTreeList ActivityCategoryList;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Label SelectCategoryLabel;
    }
}