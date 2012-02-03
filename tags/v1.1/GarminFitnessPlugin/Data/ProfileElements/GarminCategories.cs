using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    enum GarminCategories
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
