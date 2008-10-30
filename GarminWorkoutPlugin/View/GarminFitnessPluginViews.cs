using ZoneFiveSoftware.Common.Visuals;
using ZoneFiveSoftware.Common.Visuals.Fitness;

namespace GarminFitnessPlugin.View
{
    class GarminFitnessPluginViews : IExtendViews
    {
        #region IExtendViews Members

        public System.Collections.Generic.IList<IView> Views
        {
            get
            {
                return new IView[]
                {
                    new GarminFitnessView()
                };
            }
        }

        #endregion
    }
}
