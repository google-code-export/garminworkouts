using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Globalization;
using System.Reflection;
using System.Resources;
using ZoneFiveSoftware.Common.Visuals;
using ZoneFiveSoftware.Common.Visuals.Fitness;
using SportTracksPluginFramework;

namespace GarminFitnessPlugin.View
{
    class GarminFitnessSettings : STFrameworkSettingsPage
    {
#region STFrameworkSettingsPage Members

        public override System.Guid Id
        {
            get { return GUIDs.GarminFitnessSettings; }
        }

        public override IList<ISettingsPage> SubPages
        {
            get { return null; }
        }


        public override Control SettingsPageControl
        {
            get
            {
                if (m_SettingsControl == null)
                {
                    m_SettingsControl = new GarminFitnessSettingsControl();
                }

                return m_SettingsControl;
            }
        }

        public override bool HidePage()
        {
            return true;
        }

        public override string PageName
        {
            get{ return GarminFitnessView.GetLocalizedString("SettingsPageNameText"); }
        }

        public override void ShowPage(string bookmark)
        {
            m_SettingsControl.UICultureChanged(GarminFitnessView.UICulture);
        }

        public override IPageStatus Status
        {
            get { return null; }
        }

        public override void ThemeChanged(ITheme visualTheme)
        {
        }

        public override string Title
        {
            get { return GarminFitnessView.GetLocalizedString("SettingsPageNameText"); }
        }

        public override void UICultureChanged(CultureInfo culture)
        {
            CreatePageControl();
            m_SettingsControl.UICultureChanged(culture);
        }

        public override System.Guid MainPluginId
        {
            get { return GUIDs.PluginMain; }
        }

        public override event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

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
