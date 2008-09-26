using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminWorkoutPlugin.View
{
    public partial class DateSelectorDialog : Form
    {
        public DateSelectorDialog(CultureInfo UICulture, ITheme UITheme)
        {
            InitializeComponent();

            this.Text = m_ResourceManager.GetString("SelectDateText", UICulture);
            SelectDateLabel.Text = m_ResourceManager.GetString("SelectDateText", UICulture) + " :";
            Calendar.ThemeChanged(UITheme);

            m_SelectedDate = DateTime.Today;
            Calendar.AddMarkedDate(m_SelectedDate);
        }

        public DateTime SelectedDate
        {
            get { return m_SelectedDate; }
        }

        private void Calendar_DayClick(object sender, ZoneFiveSoftware.Common.Visuals.Calendar.DayClickEventArgs e)
        {
            m_SelectedDate = e.DateClicked;

            Calendar.RemoveAllMarkedDates();
            Calendar.AddMarkedDate(m_SelectedDate);
        }

        private DateTime m_SelectedDate;
        private ResourceManager m_ResourceManager = new ResourceManager("GarminWorkoutPlugin.Resources.StringResources",
                                                                        Assembly.GetExecutingAssembly());
    }
}