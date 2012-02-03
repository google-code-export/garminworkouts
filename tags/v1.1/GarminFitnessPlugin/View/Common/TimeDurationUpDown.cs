using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GarminFitnessPlugin.View
{
    public partial class TimeDurationUpDown : UserControl
    {
        public TimeDurationUpDown()
        {
            InitializeComponent();

            UpDownArrows.DecimalPlaces = 0;
            UpDownArrows.Minimum = 0;

            UpdateTextBoxes();
        }

        public UInt16 Hours
        {
            get { return (UInt16)m_Duration.Hours; }
            set { m_Duration = m_Duration.Add(new TimeSpan(value - Hours, 0, 0)); }
        }

        public UInt16 Minutes
        {
            get { return (UInt16)m_Duration.Minutes; }
            set { m_Duration = m_Duration.Add(new TimeSpan(0, value - Minutes, 0)); }
        }

        public UInt16 Seconds
        {
            get { return (UInt16)m_Duration.Seconds; }
            set { m_Duration = m_Duration.Add(new TimeSpan(0, 0, value - Seconds)); }
        }

        public UInt16 SecondsDuration
        {
            get { return (UInt16)m_Duration.TotalSeconds; }
            set
            {
                m_Duration = new TimeSpan(0, 0, value);
                m_SelectedElement = SelectableElements.None;
                UpdateTextBoxes();
            }
        }

        private void HoursText_Enter(object sender, EventArgs e)
        {
            // Save the current value as setting the max on the arrows below can trigger
            //  a value change that will update the value
            UInt16 hours = Hours;

            m_SelectedElement = SelectableElements.Hours;
            UpDownArrows.Enabled = true;

            UpDownArrows.Maximum = 17;
            UpDownArrows.Value = hours;
        }

        private void MinutesText_Enter(object sender, EventArgs e)
        {
            // Save the current value as setting the max on the arrows below can trigger
            //  a value change that will update the value
            UInt16 minutes = Minutes;

            m_SelectedElement = SelectableElements.Minutes;
            UpDownArrows.Enabled = true;

            UpDownArrows.Maximum = Constants.MinutesPerHour - 1;
            UpDownArrows.Value = minutes;
        }

        private void SecondsText_Enter(object sender, EventArgs e)
        {
            // Save the current value as setting the max on the arrows below can trigger
            //  a value change that will update the value
            UInt16 seconds = Seconds;

            m_SelectedElement = SelectableElements.Seconds;
            UpDownArrows.Enabled = true;

            UpDownArrows.Maximum = Constants.SecondsPerMinute - 1;
            UpDownArrows.Value = seconds;
        }

        private void Control_Leave(object sender, EventArgs e)
        {
            if (!ContainsFocus)
            {
                UpDownArrows.Enabled = false;
            }
        }

        private void UpDownArrows_ValueChanged(object sender, EventArgs e)
        {
            switch(m_SelectedElement)
            {
                case SelectableElements.Hours:
                {
                    if (Hours != UpDownArrows.Value)
                    {
                        Hours = (UInt16)UpDownArrows.Value;
                    }
                    break;
                }
                case SelectableElements.Minutes:
                {
                    if (Minutes != UpDownArrows.Value)
                    {
                        Minutes = (UInt16)UpDownArrows.Value;
                    }
                    break;
                }
                case SelectableElements.Seconds:
                {
                    if (Seconds != UpDownArrows.Value)
                    {
                        Seconds = (UInt16)UpDownArrows.Value;
                    }
                    break;
                }
            }

            UpdateTextBoxes();
        }

        private void HoursText_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                UpDownArrows.Value = UInt16.Parse(HoursText.Text);
            }
            catch (Exception)
            {
                HoursText.Text = UpDownArrows.Value.ToString("00");
                System.Media.SystemSounds.Asterisk.Play();
            }
            e.Cancel = false;
        }

        private void MinutesText_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                UpDownArrows.Value = UInt16.Parse(MinutesText.Text);
            }
            catch (Exception)
            {
                MinutesText.Text = UpDownArrows.Value.ToString("00");
                System.Media.SystemSounds.Asterisk.Play();
            }
            e.Cancel = false;
        }

        private void SecondsText_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                UpDownArrows.Value = UInt16.Parse(SecondsText.Text);
            }
            catch (Exception)
            {
                SecondsText.Text = UpDownArrows.Value.ToString("00");
                System.Media.SystemSounds.Asterisk.Play();
            }
            e.Cancel = false;

        }

        private void UpdateTextBoxes()
        {
            HoursText.Text = Hours.ToString("00");
            MinutesText.Text = Minutes.ToString("00");
            SecondsText.Text = Seconds.ToString("00");
        }

        enum SelectableElements
        {
            Hours,
            Minutes,
            Seconds,
            None
        };

        private SelectableElements m_SelectedElement = SelectableElements.None;
        private TimeSpan m_Duration = new TimeSpan(0, 5, 0);
    }
}
