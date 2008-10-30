namespace GarminFitnessPlugin.View
{
    partial class DateSelectorDialog
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
            this.Calendar = new SingleSelectionCalendar();
            this.SelectDateLabel = new System.Windows.Forms.Label();
            this.OKButton = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Calendar
            // 
            this.Calendar.BackColor = System.Drawing.Color.Transparent;
            this.Calendar.Border = ZoneFiveSoftware.Common.Visuals.ControlBorder.Style.RoundShadow;
            this.Calendar.Location = new System.Drawing.Point(12, 29);
            this.Calendar.MarkedColor = System.Drawing.SystemColors.Control;
            this.Calendar.MarkedColorText = System.Drawing.Color.Black;
            this.Calendar.MonthTitleColor = System.Drawing.SystemColors.Control;
            this.Calendar.MonthTitleColorText = System.Drawing.Color.Black;
            this.Calendar.Name = "Calendar";
            this.Calendar.OneMonthOnly = false;
            this.Calendar.SelectedColor = System.Drawing.SystemColors.Control;
            this.Calendar.SelectedColorText = System.Drawing.Color.Black;
            this.Calendar.SelectMode = ZoneFiveSoftware.Common.Visuals.Calendar.SelectModeType.Calendar;
            this.Calendar.Size = new System.Drawing.Size(158, 384);
            this.Calendar.StartOfWeek = System.DayOfWeek.Monday;
            this.Calendar.TabIndex = 0;
            this.Calendar.DoubleClick += new System.EventHandler(this.Calendar_DoubleClick);
            // 
            // SelectDateLabel
            // 
            this.SelectDateLabel.AutoSize = true;
            this.SelectDateLabel.Location = new System.Drawing.Point(13, 13);
            this.SelectDateLabel.Name = "SelectDateLabel";
            this.SelectDateLabel.Size = new System.Drawing.Size(76, 13);
            this.SelectDateLabel.TabIndex = 1;
            this.SelectDateLabel.Text = "Select a date :";
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(12, 419);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 2;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(95, 419);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 3;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // DateSelectorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(182, 451);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.SelectDateLabel);
            this.Controls.Add(this.Calendar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "DateSelectorDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select a date";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SingleSelectionCalendar Calendar;
        private System.Windows.Forms.Label SelectDateLabel;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button Cancel_Button;
    }
}