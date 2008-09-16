using ZoneFiveSoftware.Common.Visuals;
using ZoneFiveSoftware.Common.Visuals.Fitness;

namespace GarminWorkoutPlugin.View
{
    class GarminWorkoutPluginSettingsPages : IExtendSettingsPages
    {
        #region IExtendSettingsPages Members

        public System.Collections.Generic.IList<ISettingsPage> SettingsPages
        {
            get { return new ISettingsPage[] { new GarminWorkoutSettings() }; }
        }

        #endregion
    }
}
