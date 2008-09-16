using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;

namespace GarminWorkoutPlugin.Data
{
    class WorkoutImporter
    {
        public static bool ImportWorkout(Stream importStream, IActivityCategory category)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                byte[] byteContents = new byte[importStream.Length];
                string stringContents;

                importStream.Read(byteContents, 0, (int)importStream.Length);
                stringContents = Encoding.UTF8.GetString(byteContents, 0, (int)importStream.Length);
                stringContents = stringContents.Replace("\r\n", "");
                document.LoadXml(stringContents);

                for (int i = 0; i < document.ChildNodes.Count; ++i)
                {
                    XmlNode database = document.ChildNodes.Item(i);

                    if (database.Name == "TrainingCenterDatabase")
                    {
                        for (int j = 0; j < database.ChildNodes.Count; ++j)
                        {
                            XmlNode workoutsList = database.ChildNodes.Item(j);

                            if (workoutsList.Name == "Workouts")
                            {
                                return LoadWorkouts(workoutsList, category);
                            }
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool LoadWorkouts(XmlNode workoutsList, IActivityCategory category)
        {
            for (int i = 0; i < workoutsList.ChildNodes.Count; ++i)
            {
                XmlNode child = workoutsList.ChildNodes.Item(i);

                if (child.Name == "Workout")
                {
                    Workout newWorkout = WorkoutManager.Instance.CreateWorkout(child);

                    if (newWorkout != null)
                    {
                        newWorkout.Category = category;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
