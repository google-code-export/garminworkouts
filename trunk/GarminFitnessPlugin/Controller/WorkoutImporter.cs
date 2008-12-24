using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.View;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
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

                // Akward bug fix : Remove last character if it's a non-printing character
                for (int i = 0; i < 32; ++i)
                {
                    char currentCharacter = (char)i;

                    if (stringContents.EndsWith(currentCharacter.ToString()))
                    {
                        stringContents = stringContents.Substring(0, stringContents.Length - 1);
                        break;
                    }
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
            catch (GarminFitnesXmlDeserializationException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message + "\n\n" + e.ErroneousNode.OuterXml);
                return false;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message + "\n\n" + e.StackTrace);
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
                    IActivityCategory category = null;
                    string name = PeekWorkoutName(child);

                    if (!GarminWorkoutManager.Instance.IsWorkoutNameAvailable(name))
                    {
                        ReplaceRenameDialog dlg = new ReplaceRenameDialog(GarminWorkoutManager.Instance.GetUniqueName(name));

                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Yes)
                        {
                            // Yes = replace, delete the current workout from the list
                            Workout oldWorkout = GarminWorkoutManager.Instance.GetWorkoutWithName(name);

                            category = oldWorkout.Category;
                            GarminWorkoutManager.Instance.RemoveWorkout(oldWorkout);
                        }
                        else
                        {
                            // No = rename
                            name = dlg.NewName;

                            category = PeekWorkoutCategory(child);
                        }
                    }

                    if (category == null)
                    {
                        SelectCategoryDialog categoryDlg = new SelectCategoryDialog(name);

                        categoryDlg.ShowDialog();
                        category = categoryDlg.SelectedCategory;
                    }

                    Workout newWorkout = GarminWorkoutManager.Instance.CreateWorkout(child, category);
                    newWorkout.Name = name;
                    newWorkout.Category = category;

                    return (newWorkout != null);
                }
                else if (child.Name == "Running" ||
                         child.Name == "Biking" ||
                         child.Name == "Other")
                {
                    // This could be a TCX V1 formatting
                    if (child.ChildNodes.Count == 1 &&
                        child.FirstChild.Name == "Folder")
                    {
                        // Still looks valid, keep on
                        XmlNode folderList = child.FirstChild;

                        LoadWorkouts(folderList);
                    }
                }
            }

            return true;
        }

        private static string PeekWorkoutName(XmlNode workoutNode)
        {
            for (int i = 0; i < workoutNode.ChildNodes.Count; ++i)
            {
                XmlNode child = workoutNode.ChildNodes.Item(i);

                if (child.Name == "Name")
                {
                    if (child.ChildNodes.Count == 1 && child.FirstChild.GetType() == typeof(XmlText))
                    {
                        return ((XmlText)child.FirstChild).Value;
                    }
                }
            }

            return String.Empty;
        }

        private static IActivityCategory PeekWorkoutCategory(XmlNode workoutNode)
        {
            IActivityCategory category = null;

            for (int i = 0; i < workoutNode.ChildNodes.Count; ++i)
            {
                XmlNode child = workoutNode.ChildNodes[i];

                if (child.Name == Constants.ExtensionsTCXString)
                {
                    for (int j = 0; j < child.ChildNodes.Count; ++j)
                    {
                        XmlNode currentNode = child.ChildNodes[j];

                        // This condition remains for backsward compatibility with V0 exports
                        if (currentNode.Name == "SportTracksCategory" &&
                            currentNode.FirstChild.ChildNodes.Count == 1 &&
                            currentNode.FirstChild.GetType() == typeof(XmlText))
                        {
                            XmlText categoryNode = (XmlText)child.FirstChild.FirstChild;

                            category = Utils.FindCategoryByID(categoryNode.Value);
                        }
                        else if (currentNode.Name == "SportTracksExtensions")
                        {
                            for (int k = 0; k < currentNode.ChildNodes.Count; ++k)
                            {
                                XmlNode currentExtension = currentNode.ChildNodes[k];

                                if (currentExtension.Name == "SportTracksCategory")
                                {
                                    if (currentExtension.ChildNodes.Count == 1 &&
                                        currentExtension.FirstChild.GetType() == typeof(XmlText))
                                    {
                                        XmlText categoryNode = (XmlText)currentExtension.FirstChild;

                                        category = Utils.FindCategoryByID(categoryNode.Value);
                                        //break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return category;
        }
    }
}
