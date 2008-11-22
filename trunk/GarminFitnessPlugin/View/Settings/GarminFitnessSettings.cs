using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using ZoneFiveSoftware.Common.Visuals;
using ZoneFiveSoftware.Common.Visuals.Fitness;

namespace GarminFitnessPlugin.View
{
    class GarminFitnessSettings : ISettingsPage
    {
        #region ISettingsPage Members

        public System.Guid Id
        {
            get { return GUIDs.GarminFitnessSettings; }
        }

        public System.Collections.Generic.IList<ISettingsPage> SubPages
        {
            get { return null; }
        }

        #endregion

        #region IDialogPage Members

        public System.Windows.Forms.Control CreatePageControl()
        {
            if (m_SettingsControl == null)
            {
                m_SettingsControl = new GarminFitnessSettingsControl();
            }

            return m_SettingsControl;
        }

        public bool HidePage()
        {
            return true;
        }

        public string PageName
        {
            get{ return GarminFitnessView.ResourceManager.GetString("SettingsPageNameText", GarminFitnessView.UICulture); }
        }

        public void ShowPage(string bookmark)
        {
            m_SettingsControl.UICultureChanged(GarminFitnessView.UICulture);
        }

        public IPageStatus Status
        {
            get { return null; }
        }

        public void ThemeChanged(ITheme visualTheme)
        {
        }

        public string Title
        {
            get { return GarminFitnessView.ResourceManager.GetString("SettingsPageNameText", GarminFitnessView.UICulture); }
        }

        public void UICultureChanged(CultureInfo culture)
        {
            CreatePageControl();
            m_SettingsControl.UICultureChanged(culture);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        public GarminFitnessSettings()
        {
            PropertyChanged = new PropertyChangedEventHandler(OnPropertyChanged);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private GarminFitnessSettingsControl m_SettingsControl = null;
    }
}
