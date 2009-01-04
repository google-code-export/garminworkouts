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
                if (m_View == null)
                {
                    m_View = new GarminFitnessView();
                }

                return new IView[]
                {
                    m_View
                };
            }
        }

        #endregion

        static IView m_View = null;
    }
}
