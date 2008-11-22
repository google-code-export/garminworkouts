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
            get { return (UInt16)(Duration / Constants.SecondsPerHour); }
            set { Duration = (UInt16)(Duration + (value - Hours) * Constants.SecondsPerHour); }
        }

        public UInt16 Minutes
        {
            get { return (UInt16)((Duration / Constants.SecondsPerMinute) % Constants.MinutesPerHour); }
            set { Duration = (UInt16)(Duration + (value - Minutes) * Constants.SecondsPerMinute); }
        }

        public UInt16 Seconds
        {
            get { return (UInt16)(Duration % Constants.SecondsPerMinute); }
            set { Duration = (UInt16)(Duration - Seconds + value); }
        }

        public UInt16 Duration
        {
            get { return m_Duration; }
            set
            { 
                m_Duration = value;
                UpdateTextBoxes();
            }
        }

        private void HoursText_Enter(object sender, EventArgs e)
        {
            m_SelectedElement = SelectableElements.Hours;

            UpDownArrows.Maximum = 17;
            UpDownArrows.Value = Hours;
        }

        private void MinutesText_Enter(object sender, EventArgs e)
        {
            m_SelectedElement = SelectableElements.Minutes;

            UpDownArrows.Maximum = Constants.MinutesPerHour - 1;
            UpDownArrows.Value = Minutes;
        }

        private void SecondsText_Enter(object sender, EventArgs e)
        {
            m_SelectedElement = SelectableElements.Seconds;

            UpDownArrows.Maximum = Constants.SecondsPerMinute - 1;
            UpDownArrows.Value = Seconds;
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
        private UInt16 m_Duration = 300;
    }
}
