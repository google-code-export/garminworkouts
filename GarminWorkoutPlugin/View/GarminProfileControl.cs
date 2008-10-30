using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    public partial class GarminProfileControl : UserControl, IGarminFitnessPluginControl
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
        }

        public void RefreshUIFromLogbook()
        {
        }

        private ITheme m_CurrentTheme;
    }
}
