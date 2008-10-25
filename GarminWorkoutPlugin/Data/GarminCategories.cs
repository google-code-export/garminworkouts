using GarminWorkoutPlugin.Controller;

namespace GarminWorkoutPlugin.Data
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
