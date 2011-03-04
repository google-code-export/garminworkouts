using System;
using System.Collections.Generic;
using System.IO;
using GarminFitnessPublic;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class PublicWorkoutManager : IPublicWorkoutManager
    {
        private PublicWorkoutManager()
        {
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

        IList<IPublicWorkout> IPublicWorkoutManager.Workouts
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

        void IPublicWorkoutManager.ScheduleWorkout(IPublicWorkout workout, DateTime date)
        {
            Workout concreteWorkout = workout as Workout;

            concreteWorkout.ScheduleWorkout(date);
        }

        void IPublicWorkoutManager.SerializeWorkouts(IList<IPublicWorkout> workouts, String directory)
        {
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
                Workout concreteWorkout = currentWorkout as Workout;
                string fileName = Utils.GetWorkoutFilename(concreteWorkout, GarminWorkoutManager.FileFormats.FIT);
                FileStream file = File.Create(directory + "\\" + fileName);

                if (file != null)
                {
                    WorkoutExporter.ExportWorkoutToFIT(concreteWorkout, file, workoutIndex);
                    file.Close();
                }

                ++workoutIndex;
            }
        }

        void IPublicWorkoutManager.DeserializeWorkout(Stream dataStream)
        {
            if (WorkoutImporter.IsFITFileStream(dataStream))
            {
                WorkoutImporter.ImportWorkoutFromFIT(dataStream);
            }
            else
            {
                WorkoutImporter.ImportWorkout(dataStream);
            }
        }

        void IPublicWorkoutManager.OpenWorkoutInView(IPublicWorkout workout)
        {
            PluginMain.GetApplication().ShowView(GUIDs.GarminFitnessView, Constants.BookmarkHeader + workout.Id.ToString());
        }

#endregion

        private static PublicWorkoutManager m_Instance = null;
    }
}
