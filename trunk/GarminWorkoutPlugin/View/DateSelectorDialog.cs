using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    public partial class DateSelectorDialog : Form
    {
        public DateSelectorDialog(CultureInfo UICulture, ITheme UITheme)
        {
            InitializeComponent();

            this.Text = GarminFitnessView.ResourceManager.GetString("SelectDateText", UICulture);
            SelectDateLabel.Text = GarminFitnessView.ResourceManager.GetString("SelectDateText", UICulture) + " :";
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
    }
}