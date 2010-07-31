using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GarminFitnessPlugin.Data
{
    class WorkoutLinkStep : IStep
    {
        public WorkoutLinkStep(Workout parent, Workout linkedWorkout)
            : base(StepType.Link, parent)
        {
            m_LinkedWorkout = linkedWorkout;
        }
        public WorkoutLinkStep(Stream stream, DataVersion version, Workout parent)
            : base(StepType.Link, parent)
        {
            Deserialize(stream, version);
        }

#region IStep members

        public override uint Serialize(GarXFaceNet._Workout workout, uint stepIndex)
        {
            return m_LinkedWorkout.Steps.Serialize(workout, stepIndex);
        }

        public override void Deserialize(GarXFaceNet._Workout workout, uint stepIndex)
        {
            throw new NotImplementedException();
        }

        public override IStep Clone()
        {
            return new WorkoutLinkStep(ParentConcreteWorkout, m_LinkedWorkout);
        }

        public override bool ValidateAfterZoneCategoryChanged(ZoneFiveSoftware.Common.Data.Fitness.IZoneCategory changedCategory)
        {
            return false;
        }

        public override bool IsDirty
        {
            get { return false; }
            set { }
        }

#endregion

        public Workout LinkedWorkout
        {
            get { return m_LinkedWorkout; }
        }

        private Workout m_LinkedWorkout = null;
    }
}
