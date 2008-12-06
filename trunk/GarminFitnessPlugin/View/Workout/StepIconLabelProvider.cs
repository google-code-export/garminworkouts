using System;
using System.Collections.Generic;
using System.Text;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    class StepIconLabelProvider : TreeList.DefaultLabelProvider
    {
        public override System.Drawing.Image GetImage(object element, TreeList.Column column)
        {
            if (element.GetType() == typeof(StepWrapper))
            {
                StepWrapper wrapper = (StepWrapper)element;

                if (((IStep)wrapper.Element).IsDirty)
                {
                    return global::GarminFitnessPlugin.Resources.Resources.DirtyWarning;
                }
            }

            return base.GetImage(element, column);
        }
    }
}
