using System;
using System.Collections.Generic;
using System.Text;
using ZoneFiveSoftware.Common.Visuals;
using System.Drawing;
using GarminWorkoutPlugin.Data;

namespace GarminWorkoutPlugin.View
{
    class WorkoutRowDataRenderer : TreeList.DefaultRowDataRenderer
    {
        public WorkoutRowDataRenderer(TreeList list)
            : base(list)
        {
        }

        protected override FontStyle GetCellFontStyle(object element, TreeList.Column column)
        {
            if (element.GetType() == typeof(WorkoutWrapper))
            {
                return FontStyle.Italic;
            }

            return base.GetCellFontStyle(element, column);
        }
    }
}
