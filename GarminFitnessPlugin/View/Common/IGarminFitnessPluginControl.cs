using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    interface IGarminFitnessPluginControl
    {
        void RefreshCalendar();
        void RefreshUIFromLogbook();
        void ThemeChanged(ITheme visualTheme);
        void UICultureChanged(System.Globalization.CultureInfo culture);
    }
}
