using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    public enum GarminCategories
    {
        [GarminCategoryStringProviderAttribute("RunningText")]
        Running = 0,
        [GarminCategoryStringProviderAttribute("BikingText")]
        Biking,
        [GarminCategoryStringProviderAttribute("OtherText")]
        Other,
        GarminCategoriesCount
    }
}
