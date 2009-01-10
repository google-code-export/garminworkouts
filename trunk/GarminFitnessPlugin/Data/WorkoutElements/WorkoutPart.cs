using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.View;
using ZoneFiveSoftware.Common.Data.Fitness;

namespace GarminFitnessPlugin.Data
{
    class WorkoutPart : IWorkout
    {
        public WorkoutPart(Workout fullWorkout, int partNumber)
        {
            m_FullWorkout = fullWorkout;
            m_PartNumber = partNumber;

            NotesInternal.Value = Name + " " + String.Format(GarminFitnessView.GetLocalizedString("PartNumberingNotesText"),
                                                             m_PartNumber + 1, m_FullWorkout.GetSplitPartsCount());
        }

        public Workout ConvertToWorkout()
        {
            return GarminWorkoutManager.Instance.CreateWorkout(this);
        }

#region IWorkout Members
        public override void Serialize(Stream stream)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public override bool CanAcceptNewStep(int newStepCount, IStep destinationStep)
        {
            // Hard 20 step limit in the parts
            return GetStepCount() + newStepCount <= Constants.MaxStepsPerWorkout;
        }

        public override Workout ConcreteWorkout
        {
            get { return m_FullWorkout; }
        }

        public override IActivityCategory Category
        {
            get { return m_FullWorkout.Category; }
            set { throw new Exception("Cannot assign Category on a WorkoutPart"); }
        }

        public override GarminFitnessString NameInternal
        {
            get
            {
                List<String> names = GarminWorkoutManager.Instance.GetUniqueNameSequence(m_FullWorkout.Name, m_FullWorkout.GetSplitPartsCount());

                return new GarminFitnessString(names[m_PartNumber]);
            }
        }

        public override GarminFitnessString NotesInternal
        {
            get
            {
                if (m_PartNumber == 0)
                {
                    return m_FullWorkout.NotesInternal;
                }
                else
                {
                    return m_OverrideNotes;
                }
            }
        }
        public override DateTime LastExportDate
        {
            get { return ConcreteWorkout.LastExportDate; }
            set { ConcreteWorkout.LastExportDate = value; }
        }

        public override List<GarminFitnessDate> ScheduledDates
        {
            get { return ConcreteWorkout.ScheduledDates; }
        }

        public override System.Collections.Generic.List<IStep> Steps
        {
            get { return m_FullWorkout.GetStepsForPart(m_PartNumber); }
        }

        public override bool AddToDailyViewOnSchedule
        {
            get { return m_FullWorkout.AddToDailyViewOnSchedule; }
            set { throw new Exception("Cannot assign AddToDailyViewOnSchedule on a WorkoutPart");  }
        }
#endregion

#region IXMLSerializable Members
        public override void Deserialize(XmlNode parentNode)
        {
            throw new Exception("Cannot deserialize a WorkoutPart");
        }
#endregion

        Workout m_FullWorkout;
        int m_PartNumber;
        GarminFitnessString m_OverrideNotes = new GarminFitnessString();
    }
}
