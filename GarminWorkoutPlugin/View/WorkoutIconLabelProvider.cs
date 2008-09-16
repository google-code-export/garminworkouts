using System;
using System.Collections.Generic;
using System.Text;
using ZoneFiveSoftware.Common.Visuals;
using GarminWorkoutPlugin.Data;

namespace GarminWorkoutPlugin.View
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
                    return global::GarminWorkoutPlugin.Properties.Resources.WorkoutDirtyIcon;
                }

                return global::GarminWorkoutPlugin.Properties.Resources.WorkoutIcon;
            }
            else if (element.GetType() == typeof(ActivityCategoryWrapper))
            {
                return global::GarminWorkoutPlugin.Properties.Resources.CategoryIcon;
            }

            return base.GetImage(element, column);
        }
    }
}
