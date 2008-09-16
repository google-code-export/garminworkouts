using ZoneFiveSoftware.Common.Visuals;
using ZoneFiveSoftware.Common.Visuals.Fitness;

namespace GarminWorkoutPlugin.View
{
    class GarminWorkoutPluginViews : IExtendViews
    {
        #region IExtendViews Members

        public System.Collections.Generic.IList<IView> Views
        {
            get
            {
                return new IView[]
                {
                    new GarminWorkoutView()
                };
            }
        }

        #endregion
    }
}
