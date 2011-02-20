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
                if (m_Instance != null)
                {
                    m_Instance = new PublicWorkoutManager();
                }

                return m_Instance;
            }
        }

#region IPublicWorkoutManager Members

        public IList<IPublicWorkout> PublicWorkouts
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
            ushort workoutIndex = 0;

            foreach (IPublicWorkout currentWorkout in workouts)
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

        public void DeserializeWorkout(Stream dataStream)
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

        public void OpenWorkoutInView(IPublicWorkout workout)
        {
            PluginMain.GetApplication().ShowView(GUIDs.GarminFitnessView, Constants.BookmarkHeader + workout.PublicId.ToString());
        }

#endregion

        private static PublicWorkoutManager m_Instance = null;
    }
}
