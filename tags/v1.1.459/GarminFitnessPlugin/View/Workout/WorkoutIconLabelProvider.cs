using System;
using System.Collections.Generic;
using System.Text;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    class WorkoutIconLabelProvider : TreeList.DefaultLabelProvider
    {
        public WorkoutIconLabelProvider()
        {
        }

        public override System.Drawing.Image GetImage(object element, TreeList.Column column)
        {
            if (element.GetType() == typeof(WorkoutWrapper))
            {
                WorkoutWrapper wrapper = (WorkoutWrapper)element;

                if (((Workout)wrapper.Element).IsDirty)
                {
                    return global::GarminFitnessPlugin.Resources.Resources.WorkoutDirtyIcon;
                }
                else if (((Workout)wrapper.Element).ContainsFITOnlyFeatures)
                {
                    return global::GarminFitnessPlugin.Resources.Resources.FITWorkoutIcon;
                }

                return global::GarminFitnessPlugin.Resources.Resources.WorkoutIcon;
            }
            if (element.GetType() == typeof(WorkoutPartWrapper))
            {
                return global::GarminFitnessPlugin.Resources.Resources.WorkoutPartIcon;
            }
            else if (element.GetType() == typeof(ActivityCategoryWrapper))
            {
                return global::GarminFitnessPlugin.Resources.Resources.CategoryIcon;
            }

            return base.GetImage(element, column);
        }
    }
}
