using ZoneFiveSoftware.Common.Visuals;
using ZoneFiveSoftware.Common.Visuals.Fitness;
using SportTracksPluginFramework;

namespace GarminFitnessPlugin.View
{
    class GarminFitnessPluginSettingsPages : STFrameworkSettingsPageExtension
    {
        public GarminFitnessPluginSettingsPages()
        {
            RegisterSettingsPage(new GarminFitnessSettings());
        }
    }
}
