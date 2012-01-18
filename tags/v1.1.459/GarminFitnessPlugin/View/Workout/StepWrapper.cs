using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using GarminFitnessPlugin.Data;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.View
{
    class StepWrapper : TreeList.TreeListNode
    {
        public StepWrapper(StepWrapper parent, IStep element, Workout baseWorkout, bool isWorkoutLinkChild)
            : base(parent, element)
        {
            Debug.Assert(baseWorkout != null);

            m_BaseWorkout = baseWorkout;
            m_IsWorkoutLinkChild = isWorkoutLinkChild;
        }

        public String DisplayString
        {
            get { return StepDescriptionStringFormatter.FormatStepDescription((IStep)Element); }
        }

        public String AutoSplitPart
        {
            get
            {
                string result = String.Empty;
                IStep step = (IStep)Element;

                if (!(step is WorkoutLinkStep) &&
                    m_BaseWorkout.GetSplitPartsCount() != 1)
                {
                    result = m_BaseWorkout.GetStepSplitPart(step).ToString();
                }

                if (step.ForceSplitOnStep)
                {
                    result = "*" + result;
                }

                return result;
            }
        }

        public bool IsWorkoutLinkChild
        {
            get { return m_IsWorkoutLinkChild; }
        }

        private Workout m_BaseWorkout = null;
        private bool m_IsWorkoutLinkChild = false;
    }
}
