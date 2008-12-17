using ZoneFiveSoftware.Common.Visuals;
using ZoneFiveSoftware.Common.Visuals.Fitness;

namespace GarminFitnessPlugin.View
{
    class GarminFitnessPluginSettingsPages : IExtendSettingsPages
    {
        #region IExtendSettingsPages Members

        public System.Collections.Generic.IList<ISettingsPage> SettingsPages
        {
            get { return new ISettingsPage[] { new GarminFitnessSettings() }; }
        }

        #endregion
    }
}
