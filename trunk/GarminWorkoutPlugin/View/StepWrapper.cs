using System;
using System.Collections.Generic;
using System.Text;
using GarminWorkoutPlugin.Data;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminWorkoutPlugin.View
{
    class StepWrapper : TreeList.TreeListNode
    {
        public StepWrapper(StepWrapper parent, IStep element)
            : base(parent, element)
        {
        }

        public String DisplayString
        {
            get { return StepDescriptionStringFormatter.FormatStepDescription((IStep)Element); }
        }
    }
}
