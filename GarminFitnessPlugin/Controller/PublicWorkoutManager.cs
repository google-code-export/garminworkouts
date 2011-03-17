using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using GarminFitnessPublic;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class PublicWorkoutManager : IPublicWorkoutManager
    {
        private PublicWorkoutManager()
        {
            GarminWorkoutManager.Instance.WorkoutListChanged += new GarminWorkoutManager.WorkoutListChangedEventHandler(OnManagerWorkoutListChanged);
            GarminWorkoutManager.Instance.WorkoutChanged += new GarminWorkoutManager.WorkoutChangedEventHandler(OnManagerWorkoutChanged);
            GarminWorkoutManager.Instance.WorkoutStepChanged += new GarminWorkoutManager.WorkoutStepChangedEventHandler(OnManagerWorkoutStepChanged);
            GarminWorkoutManager.Instance.WorkoutStepDurationChanged += new GarminWorkoutManager.WorkoutStepDurationChangedEventHandler(OnManagerWorkoutStepDurationChanged);
            GarminWorkoutManager.Instance.WorkoutStepRepeatDurationChanged += new GarminWorkoutManager.WorkoutStepRepeatDurationChangedEventHandler(OnManagerWorkoutStepRepeatDurationChanged);
            GarminWorkoutManager.Instance.WorkoutStepTargetChanged += new GarminWorkoutManager.WorkoutStepTargetChangedEventHandler(OnManagerWorkoutStepTargetChanged);
        }

        private void OnManagerWorkoutListChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("WorkoutList"));
            }
        }

        private void OnManagerWorkoutChanged(IWorkout modifiedWorkout, PropertyChangedEventArgs changedProperty)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(modifiedWorkout.ConcreteWorkout as IPublicWorkout, new PropertyChangedEventArgs("Workout"));
            }
        }

        void OnManagerWorkoutStepChanged(IWorkout modifiedWorkout, IStep modifiedStep, PropertyChangedEventArgs changedProperty)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(modifiedWorkout.ConcreteWorkout as IPublicWorkout, new PropertyChangedEventArgs("WorkoutStep"));
            }
        }

        void OnManagerWorkoutStepDurationChanged(IWorkout modifiedWorkout, RegularStep modifiedStep, IDuration modifiedDuration, PropertyChangedEventArgs changedProperty)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(modifiedWorkout.ConcreteWorkout as IPublicWorkout, new PropertyChangedEventArgs("WorkoutStepDuration"));
            }
        }

        void OnManagerWorkoutStepRepeatDurationChanged(IWorkout modifiedWorkout, RepeatStep modifiedStep, IRepeatDuration modifiedDuration, PropertyChangedEventArgs changedProperty)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(modifiedWorkout.ConcreteWorkout as IPublicWorkout, new PropertyChangedEventArgs("WorkoutStepRepeatDuration"));
            }
        }

        void OnManagerWorkoutStepTargetChanged(IWorkout modifiedWorkout, RegularStep modifiedStep, ITarget modifiedDuration, PropertyChangedEventArgs changedProperty)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(modifiedWorkout.ConcreteWorkout as IPublicWorkout, new PropertyChangedEventArgs("WorkoutStepTarget"));
            }
        }

        public static IPublicWorkoutManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new PublicWorkoutManager();
                }

                return m_Instance;
            }
        }

#region IPublicWorkoutManager Members

        public IList<IPublicWorkout> Workouts
        {
            get
            {
                IList<IPublicWorkout> result = new List<IPublicWorkout>();

                foreach (Workout currentWorkout in GarminWorkoutManager.Instance.Workouts)
                {
                    result.Add(currentWorkout);
                }

                return result;
            }
        }

        public void ScheduleWorkout(IPublicWorkout workout, DateTime date)
        {
            Workout concreteWorkout = workout as Workout;

            concreteWorkout.ScheduleWorkout(date);
        }

        public void SerializeWorkouts(IList<IPublicWorkout> workouts, String directory)
        {
            MemoryStream schedulesDataStream = new MemoryStream();
            List<IWorkout> workoutsToExport = new List<IWorkout>();

            ushort workoutIndex = 0;

            // Populate list of workouts to export
            foreach (IPublicWorkout currentWorkout in workouts)
            {
                Workout concreteWorkout = currentWorkout as Workout;

                if (concreteWorkout != null)
                {
                    if (concreteWorkout.GetSplitPartsCount() > 1)
                    {
                        List<WorkoutPart> splitParts = concreteWorkout.SplitInSeperateParts();

                        // Replace the workout by it's parts
                        foreach (WorkoutPart currentPart in splitParts)
                        {
                            workoutsToExport.Add(currentPart);
                        }
                    }
                    else
                    {
                        workoutsToExport.Add(concreteWorkout);
                    }
                }
            }

            foreach (IWorkout currentWorkout in workoutsToExport)
            {
                IWorkout concreteWorkout = currentWorkout as IWorkout;
                string fileName = Utils.GetWorkoutFilename(concreteWorkout, GarminWorkoutManager.FileFormats.FIT);
                FileStream file = File.Create(directory + "\\" + fileName);

                if (file != null)
                {
                    WorkoutExporter.ExportWorkoutToFIT(concreteWorkout, file, workoutIndex, false);
                    concreteWorkout.SerializetoFITSchedule(schedulesDataStream);

                    file.Close();
                }

                ++workoutIndex;
            }

            FileStream schedulesFileStream = File.Create(directory + "\\" + "Schedules.fit");
            WorkoutExporter.ExportSchedulesFITFile(schedulesFileStream, schedulesDataStream, workoutIndex);
            schedulesFileStream.Close();
        }

        public void DeserializeWorkout(Stream dataStream)
        {
            if (WorkoutImporter.IsFITFileStream(dataStream))
            {
                UInt32 workoutId;

                WorkoutImporter.ImportWorkoutFromFIT(dataStream, out workoutId);
            }
            else
            {
                WorkoutImporter.ImportWorkout(dataStream);
            }
        }

        public void OpenWorkoutInView(IPublicWorkout workout)
        {
            PluginMain.GetApplication().ShowView(GUIDs.GarminFitnessView, Constants.BookmarkHeader + workout.Id.ToString());
        }

        public event PropertyChangedEventHandler PropertyChanged;

#endregion

        private static PublicWorkoutManager m_Instance = null;
    }
}
