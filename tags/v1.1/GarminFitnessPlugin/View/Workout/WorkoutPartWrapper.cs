using System;
using System.Collections.Generic;
using System.Text;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    class WorkoutPartWrapper : TreeList.TreeListNode
    {
        public WorkoutPartWrapper(WorkoutWrapper parent, WorkoutPart element)
            : base(parent, element)
        {
        }

        public String Name
        {
            get { return ((WorkoutPart)Element).Name; }
        }
    }
}
