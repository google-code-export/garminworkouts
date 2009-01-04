using ZoneFiveSoftware.Common.Visuals;
using ZoneFiveSoftware.Common.Visuals.Fitness;

namespace GarminFitnessPlugin.View
{
    class GarminFitnessPluginSettingsPages : IExtendSettingsPages
    {
        #region IExtendSettingsPages Members

        public System.Collections.Generic.IList<ISettingsPage> SettingsPages
        {
            get
            {
                if (m_SettingPage == null)
                {
                    m_SettingPage = new GarminFitnessSettings();
                }

                return new ISettingsPage[] { m_SettingPage };
            }
        }

        #endregion

        ISettingsPage m_SettingPage = null;
    }
}
