using System;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    class ActivityCategoryWrapper : TreeList.TreeListNode
    {
        public ActivityCategoryWrapper(ActivityCategoryWrapper parent, IActivityCategory element)
            : base(parent, element)
        {
        }

        public String Name
        {
            get { return ((IActivityCategory)Element).Name; }
        }
    }
}
