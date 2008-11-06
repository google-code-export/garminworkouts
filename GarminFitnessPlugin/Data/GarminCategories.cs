using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    enum GarminCategories
    {
        [GarminCategoryStringProviderAttribute("RunningText")]
        Running = 0,
        [GarminCategoryStringProviderAttribute("BikingText")]
        Cycling,
        [GarminCategoryStringProviderAttribute("OtherText")]
        Other,
        GarminCategoriesCount
    }
}
