using System.Collections.Generic;
using ZoneFiveSoftware.Common.Visuals;
using ZoneFiveSoftware.Common.Visuals.Fitness;
using SportTracksPluginFramework;

namespace GarminFitnessPlugin.View
{
    class GarminFitnessPluginViews : STFrameworkViewExtension
    {
        public GarminFitnessPluginViews()
        {
            RegisterView(new GarminFitnessView());
        }
    }
}
