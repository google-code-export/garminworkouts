using System;
using System.Collections.Generic;
using System.Text;
using GarminFitnessPlugin.Data;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.View
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

        public String AutoSplitPart
        {
            get
            {
                IStep step = (IStep)Element;

                if (step.ParentConcreteWorkout.GetSplitPartsCount() == 1)
                {
                    return string.Empty;
                }
                else
                {
                    string result = (step.SplitPartInWorkout + 1).ToString();

                    if (step.ForceSplitOnStep)
                    {
                        result = "*" + result;
                    }

                    return result;
                }
            }
        }
    }
}
