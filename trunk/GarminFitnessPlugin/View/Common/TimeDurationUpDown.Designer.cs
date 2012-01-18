namespace GarminFitnessPlugin.View
{
    partial class TimeDurationUpDown
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
            this.HoursText = new ZoneFiveSoftware.Common.Visuals.TextBox();
            this.SecondsText = new ZoneFiveSoftware.Common.Visuals.TextBox();
            this.MinutesText = new ZoneFiveSoftware.Common.Visuals.TextBox();
            this.HourMinSeparatorLabel = new System.Windows.Forms.Label();
            this.MinSecSeparatorLabel = new System.Windows.Forms.Label();
            this.UpDownArrows = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.UpDownArrows)).BeginInit();
            this.SuspendLayout();
            // 
            // HoursText
            // 
            this.HoursText.AcceptsReturn = false;
            this.HoursText.AcceptsTab = false;
            this.HoursText.BackColor = System.Drawing.Color.White;
            this.HoursText.BorderColor = System.Drawing.Color.White;
            this.HoursText.ButtonImage = null;
            this.HoursText.Location = new System.Drawing.Point(0, 2);
            this.HoursText.MaxLength = 2;
            this.HoursText.Multiline = false;
            this.HoursText.Name = "HoursText";
            this.HoursText.ReadOnly = false;
            this.HoursText.ReadOnlyColor = System.Drawing.SystemColors.Control;
            this.HoursText.ReadOnlyTextColor = System.Drawing.SystemColors.ControlLight;
            this.HoursText.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.HoursText.Size = new System.Drawing.Size(19, 19);
            this.HoursText.TabIndex = 0;
            this.HoursText.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.HoursText.Enter += new System.EventHandler(this.HoursText_Enter);
            this.HoursText.Leave += new System.EventHandler(this.Control_Leave);
            this.HoursText.Validating += new System.ComponentModel.CancelEventHandler(this.HoursText_Validating);
            // 
            // SecondsText
            // 
            this.SecondsText.AcceptsReturn = false;
            this.SecondsText.AcceptsTab = false;
            this.SecondsText.BackColor = System.Drawing.Color.White;
            this.SecondsText.BorderColor = System.Drawing.Color.White;
            this.SecondsText.ButtonImage = null;
            this.SecondsText.Location = new System.Drawing.Point(42, 2);
            this.SecondsText.MaxLength = 2;
            this.SecondsText.Multiline = false;
            this.SecondsText.Name = "SecondsText";
            this.SecondsText.ReadOnly = false;
            this.SecondsText.ReadOnlyColor = System.Drawing.SystemColors.Control;
            this.SecondsText.ReadOnlyTextColor = System.Drawing.SystemColors.ControlLight;
            this.SecondsText.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.SecondsText.Size = new System.Drawing.Size(19, 19);
            this.SecondsText.TabIndex = 2;
            this.SecondsText.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.SecondsText.Enter += new System.EventHandler(this.SecondsText_Enter);
            this.SecondsText.Leave += new System.EventHandler(this.Control_Leave);
            this.SecondsText.Validating += new System.ComponentModel.CancelEventHandler(this.SecondsText_Validating);
            // 
            // MinutesText
            // 
            this.MinutesText.AcceptsReturn = false;
            this.MinutesText.AcceptsTab = false;
            this.MinutesText.BackColor = System.Drawing.Color.White;
            this.MinutesText.BorderColor = System.Drawing.Color.White;
            this.MinutesText.ButtonImage = null;
            this.MinutesText.Location = new System.Drawing.Point(21, 2);
            this.MinutesText.MaxLength = 2;
            this.MinutesText.Multiline = false;
            this.MinutesText.Name = "MinutesText";
            this.MinutesText.ReadOnly = false;
            this.MinutesText.ReadOnlyColor = System.Drawing.SystemColors.Control;
            this.MinutesText.ReadOnlyTextColor = System.Drawing.SystemColors.ControlLight;
            this.MinutesText.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.MinutesText.Size = new System.Drawing.Size(19, 19);
            this.MinutesText.TabIndex = 1;
            this.MinutesText.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.MinutesText.Enter += new System.EventHandler(this.MinutesText_Enter);
            this.MinutesText.Leave += new System.EventHandler(this.Control_Leave);
            this.MinutesText.Validating += new System.ComponentModel.CancelEventHandler(this.MinutesText_Validating);
            // 
            // HourMinSeparatorLabel
            // 
            this.HourMinSeparatorLabel.BackColor = System.Drawing.Color.Transparent;
            this.HourMinSeparatorLabel.Location = new System.Drawing.Point(15, 4);
            this.HourMinSeparatorLabel.Name = "HourMinSeparatorLabel";
            this.HourMinSeparatorLabel.Size = new System.Drawing.Size(10, 19);
            this.HourMinSeparatorLabel.TabIndex = 0;
            this.HourMinSeparatorLabel.Text = ":";
            // 
            // MinSecSeparatorLabel
            // 
            this.MinSecSeparatorLabel.BackColor = System.Drawing.Color.Transparent;
            this.MinSecSeparatorLabel.Location = new System.Drawing.Point(36, 4);
            this.MinSecSeparatorLabel.Name = "MinSecSeparatorLabel";
            this.MinSecSeparatorLabel.Size = new System.Drawing.Size(10, 19);
            this.MinSecSeparatorLabel.TabIndex = 0;
            this.MinSecSeparatorLabel.Text = ":";
            // 
            // UpDownArrows
            // 
            this.UpDownArrows.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.UpDownArrows.Enabled = false;
            this.UpDownArrows.Location = new System.Drawing.Point(66, 2);
            this.UpDownArrows.Name = "UpDownArrows";
            this.UpDownArrows.Size = new System.Drawing.Size(18, 20);
            this.UpDownArrows.TabIndex = 3;
            this.UpDownArrows.TabStop = false;
            this.UpDownArrows.ValueChanged += new System.EventHandler(this.UpDownArrows_ValueChanged);
            this.UpDownArrows.Leave += new System.EventHandler(this.Control_Leave);
            // 
            // TimeDurationUpDown
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.UpDownArrows);
            this.Controls.Add(this.SecondsText);
            this.Controls.Add(this.MinutesText);
            this.Controls.Add(this.HoursText);
            this.Controls.Add(this.MinSecSeparatorLabel);
            this.Controls.Add(this.HourMinSeparatorLabel);
            this.MaximumSize = new System.Drawing.Size(84, 22);
            this.MinimumSize = new System.Drawing.Size(84, 22);
            this.Name = "TimeDurationUpDown";
            this.Size = new System.Drawing.Size(84, 22);
            ((System.ComponentModel.ISupportInitialize)(this.UpDownArrows)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private ZoneFiveSoftware.Common.Visuals.TextBox HoursText;
        private ZoneFiveSoftware.Common.Visuals.TextBox SecondsText;
        private ZoneFiveSoftware.Common.Visuals.TextBox MinutesText;
        private System.Windows.Forms.Label HourMinSeparatorLabel;
        private System.Windows.Forms.Label MinSecSeparatorLabel;
        private System.Windows.Forms.NumericUpDown UpDownArrows;
    }
}
