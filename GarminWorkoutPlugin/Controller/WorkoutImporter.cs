using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminWorkoutPlugin.View;
using GarminWorkoutPlugin.Data;

namespace GarminWorkoutPlugin.Controller
{
    class WorkoutImporter
    {
        public static bool ImportWorkout(Stream importStream)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                byte[] byteContents = new byte[importStream.Length];
                string stringContents;

                importStream.Read(byteContents, 0, (int)importStream.Length);
                stringContents = Encoding.UTF8.GetString(byteContents, 0, (int)importStream.Length);

                // Remove all non-printing characters
                for (int i = 0; i < 32; ++i)
                {
                    char currentCharacter = (char)i;
                    stringContents = stringContents.Replace(currentCharacter.ToString(), "");
                }

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
                                return LoadWorkouts(workoutsList);
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message + "\n" + e.StackTrace);
                return false;
            }
        }

        private static bool LoadWorkouts(XmlNode workoutsList)
        {
            for (int i = 0; i < workoutsList.ChildNodes.Count; ++i)
            {
                XmlNode child = workoutsList.ChildNodes.Item(i);

                if (child.Name == "Workout")
                {
                    Workout newWorkout = WorkoutManager.Instance.CreateWorkout(child);

                    if (newWorkout != null)
                    {
                        GarminWorkoutView currentView = (GarminWorkoutView)PluginMain.GetApplication().ActiveView;
                        SelectCategoryDialog categoryDlg = new SelectCategoryDialog(newWorkout.Name, currentView.UICulture);

                        categoryDlg.ShowDialog();
                        newWorkout.Category = categoryDlg.SelectedCategory;
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
