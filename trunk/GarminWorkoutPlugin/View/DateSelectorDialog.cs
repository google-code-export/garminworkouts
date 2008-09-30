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

            Calendar.SelectedDate = DateTime.Today;
        }

        public DateTime SelectedDate
        {
            get { return Calendar.SelectedDate; }
        }
        
        private void Calendar_DoubleClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private ResourceManager m_ResourceManager = new ResourceManager("GarminWorkoutPlugin.Resources.StringResources",
                                                                        Assembly.GetExecutingAssembly());
    }
}