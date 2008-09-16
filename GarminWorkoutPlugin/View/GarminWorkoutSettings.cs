using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using ZoneFiveSoftware.Common.Visuals;
using ZoneFiveSoftware.Common.Visuals.Fitness;

namespace GarminWorkoutPlugin.View
{
    class GarminWorkoutSettings : ISettingsPage
    {
        #region ISettingsPage Members

        public System.Guid Id
        {
            get { return GUIDs.GarminWorkoutSettings; }
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
                m_SettingsControl = new GarminWorkoutSettingsControl();
            }

            return m_SettingsControl;
        }

        public bool HidePage()
        {
            return true;
        }

        public string PageName
        {
            get { return m_ResourceManager.GetString("SettingsPageNameText", m_CurrentCulture); }
        }

        public void ShowPage(string bookmark)
        {
            m_SettingsControl.UICultureChanged(m_CurrentCulture);
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
            get { return m_ResourceManager.GetString("SettingsPageNameText", m_CurrentCulture); }
        }

        public void UICultureChanged(CultureInfo culture)
        {
            m_CurrentCulture = culture;

            CreatePageControl();
            m_SettingsControl.UICultureChanged(culture);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        public GarminWorkoutSettings()
        {
            PropertyChanged = new PropertyChangedEventHandler(OnPropertyChanged);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private GarminWorkoutSettingsControl m_SettingsControl = null;
        private ResourceManager m_ResourceManager = new ResourceManager("GarminWorkoutPlugin.Resources.StringResources",
                                                                        Assembly.GetExecutingAssembly());
        private CultureInfo m_CurrentCulture;
    }
}
