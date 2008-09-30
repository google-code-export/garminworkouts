using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminWorkoutPlugin.View
{
    [DefaultEvent("SelectionChanged")]
    public partial class SingleSelectionCalendar : Calendar
    {
        public SingleSelectionCalendar()
        {
            InitializeComponent();

            this.DayClick += new DayClickHandler(SingleSelectionCalendar_DayClick);

            AddMarkedDateStyle(m_SelectedDate, Calendar.MarkerStyle.Normal);
        }

        void SingleSelectionCalendar_DayClick(object sender, Calendar.DayClickEventArgs e)
        {
            SelectedDate = e.DateClicked;
        }

        public DateTime SelectedDate
        {
            get { return m_SelectedDate; }
            set
            {
                if (SelectionChanged != null && m_SelectedDate != value)
                {
                    SelectionChanged(this, value);
                }

                RemoveMarkedDateStyle(m_SelectedDate, Calendar.MarkerStyle.Normal);
                m_SelectedDate = value;
                AddMarkedDateStyle(m_SelectedDate, Calendar.MarkerStyle.Normal);
            }
        }
        
        public delegate void SelectionChangedEvenHandler(object sender, DateTime newSelection);
        public event SelectionChangedEvenHandler SelectionChanged;

        private DateTime m_SelectedDate = DateTime.Today;
    }
}
