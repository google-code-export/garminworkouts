using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.View;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.SportTracks.Device.GarminAPI.Xml;

namespace GarminFitnessPlugin.Data
{
    class WorkoutPart : IWorkout
    {
        public WorkoutPart(Workout fullWorkout, int partNumber)
        {
            m_FullWorkout = fullWorkout;
            m_PartNumber = partNumber;
        }

        public Workout ConvertToWorkout()
        {
            return GarminWorkoutManager.Instance.CreateWorkout(this);
        }

        public bool IsValid()
        {
            Workout parent = GarminWorkoutManager.Instance.GetWorkout(ConcreteWorkout.Name);

            return parent != null && m_PartNumber < parent.GetSplitPartsCount();
        }

#region IWorkout Members
        public override void Serialize(Stream stream)
        {
            throw new System.Exception("There is no need to serialize a WorkoutPart");
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
                List<String> names = GarminWorkoutManager.Instance.GetReservedNamesForWorkout(ConcreteWorkout);

                return new GarminFitnessString(names[m_PartNumber]);
            }
        }

        public override GarminFitnessGuid IdInternal
        {
            get
            {
                return ConcreteWorkout.IdInternal;
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
                    return new GarminFitnessString(ConcreteWorkout.Name + " " +
                                                   String.Format(GarminFitnessView.GetLocalizedString("PartNumberingNotesText"),
                                                                 m_PartNumber + 1, m_FullWorkout.GetSplitPartsCount()));
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

        public override WorkoutStepsList Steps
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

        public override void Deserialize(GarXFaceNet._Workout workout)
        {
            throw new Exception("Cannot deserialize a WorkoutPart");
        }

        public override void DeserializeOccurances(GarXFaceNet._WorkoutOccuranceList occuranceList)
        {
            throw new Exception("Cannot deserialize a WorkoutPart");
        }

        Workout m_FullWorkout;
        int m_PartNumber;
    }
}
