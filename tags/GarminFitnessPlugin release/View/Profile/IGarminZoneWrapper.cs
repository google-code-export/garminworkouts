using System;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    interface IGarminZoneWrapper
    {
        int Index { get; }
        String Name { get; }
        String Low { get; }
        String High { get; }
        void UpdateProfile(GarminCategories category);
    }
}
