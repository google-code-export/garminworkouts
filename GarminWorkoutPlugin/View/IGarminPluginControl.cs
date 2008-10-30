using ZoneFiveSoftware.Common.Visuals;

namespace GarminWorkoutPlugin.View
{
    interface IGarminPluginControl
    {
        void RefreshUIFromLogbook();
        void ThemeChanged(ITheme visualTheme);
        void UICultureChanged(System.Globalization.CultureInfo culture);
    }
}
