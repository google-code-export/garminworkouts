using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminWorkoutPlugin.View
{
    public partial class GarminProfileControl : UserControl, IGarminPluginControl
    {
        public GarminProfileControl()
        {
            InitializeComponent();
        }

        public void ThemeChanged(ITheme visualTheme)
        {
            m_CurrentTheme = visualTheme;
        }

        public void UICultureChanged(System.Globalization.CultureInfo culture)
        {
            m_CurrentCulture = culture;
        }

        public void RefreshUIFromLogbook()
        {
        }

        private ResourceManager m_ResourceManager = new ResourceManager("GarminWorkoutPlugin.Resources.StringResources",
                                                                Assembly.GetExecutingAssembly());
        private CultureInfo m_CurrentCulture;
        private ITheme m_CurrentTheme;
    }
}
