using System;
using System.Collections.Generic;
using System.Text;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    class WorkoutWrapper : TreeList.TreeListNode
    {
        public WorkoutWrapper(ActivityCategoryWrapper parent, Workout element)
            : base(parent, element)
        {
        }

        public String Name
        {
            get { return ((Workout)Element).Name; }
        }
    }
}
