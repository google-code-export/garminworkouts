using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.View;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class WorkoutImporter
    {
        public interface AsyncImportDelegate
        {
            void OnAsyncImportCompleted(bool success);
            void OnProgressChanged(int progressPercent);
        }

        public static bool IsFITFileStream(Stream fileStream)
        {
            fileStream.Seek(8, SeekOrigin.Begin);
            Byte[] buffer = new Byte[4];
            fileStream.Read(buffer, 0, 4);
            String FITMarker = Encoding.UTF8.GetString(buffer, 0, 4);
            fileStream.Seek(0, SeekOrigin.Begin);

            return FITMarker.Equals(FITConstants.FITFileDescriptor);
        }

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
            catch (GarminFitnessXmlDeserializationException e)
            {
                MessageBox.Show(e.Message + "\n\n" + e.ErroneousNode.OuterXml);
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n\n" + e.StackTrace);
                return false;
            }
        }

        public static bool ImportWorkout(GarXFaceNet._Workout workout, GarXFaceNet._WorkoutOccuranceList occuranceList)
        {
            GarminFitnessView pluginView = PluginMain.GetApplication().ActiveView as GarminFitnessView;
            GarminWorkoutControl workoutControl = pluginView.GetCurrentView() as GarminWorkoutControl;
            IActivityCategory category = null;
            String workoutName = workout.GetName();

            workoutControl.GetNewWorkoutNameAndCategory(ref workoutName, ref category);

            Workout newWorkout = GarminWorkoutManager.Instance.CreateWorkout(category);

            if (newWorkout == null)
            {
                return false;
            }

            newWorkout.Deserialize(workout);
            newWorkout.DeserializeOccurances(occuranceList);

            return true;
        }

        public static bool ImportWorkoutFromFIT(Stream importStream)
        {
            try
            {
                Logger.Instance.LogText("Init parser");

                if (FITParser.Instance.Init(importStream))
                {
                    FITMessage parsedMessage;

                    do
                    {
                        parsedMessage = FITParser.Instance.ReadNextMessage();

                        if (parsedMessage != null)
                        {
                            Logger.Instance.LogText(String.Format("FIT parsed message type={0:0}", (int)parsedMessage.GlobalMessageType));

                            switch (parsedMessage.GlobalMessageType)
                            {
                                case FITGlobalMessageIds.FileId:
                                    {
                                        // Make sure we have a workout file
                                        FITMessageField fileTypeField = parsedMessage.GetField((Byte)FITFileIdFieldsIds.FileType);

                                        if (fileTypeField == null ||
                                            (FITFileTypes)fileTypeField.GetEnum() != FITFileTypes.Workout)
                                        {
                                            Logger.Instance.LogText("Not a workout FIT file");

                                            FITParser.Instance.Close();
                                            return false;
                                        }

                                        break;
                                    }
                                case FITGlobalMessageIds.Workout:
                                    {
                                        // Peek name
                                        FITMessageField nameField = parsedMessage.GetField((Byte)FITWorkoutFieldIds.WorkoutName);

                                        if (nameField != null)
                                        {
                                            GarminFitnessView pluginView = PluginMain.GetApplication().ActiveView as GarminFitnessView;
                                            GarminWorkoutControl workoutControl = pluginView.GetCurrentView() as GarminWorkoutControl;
                                            IActivityCategory category = null;
                                            String workoutName = nameField.GetString();

                                            workoutControl.GetNewWorkoutNameAndCategory(ref workoutName, ref category);

                                            Workout newWorkout = GarminWorkoutManager.Instance.CreateWorkout(workoutName, parsedMessage, category);
                                        }
                                        else
                                        {
                                            throw new FITParserException("No name for workout");
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        // Nothing to do, unsupported message
                                        break;
                                    }
                            }
                        }
                    }
                    while (parsedMessage != null);

                    FITParser.Instance.Close();

                    return true;
                }
                else
                {
                    Logger.Instance.LogText("FIT parser init failed");

                    return false;
                }
            }
            catch (FITParserException e)
            {
                MessageBox.Show("Error parsing FIT file\n\n" +
                                e.Message + "\n\n" +
                                e.StackTrace);
                FITParser.Instance.Close();
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show("General error on import\n\n" +
                                e.Message + "\n\n" +
                                e.StackTrace);
                FITParser.Instance.Close();
                return false;
            }
        }

        public static bool AsyncImportDirectory(string directory, AsyncImportDelegate completedDelegate)
        {
            BackgroundWorker importerThread = new BackgroundWorker();

            Debug.Assert(m_AsyncCompletedDelegate == null);

            if (completedDelegate != null)
            {
                m_AsyncCompletedDelegate = completedDelegate;
                m_ImportDirectory = directory;

                importerThread.DoWork += new DoWorkEventHandler(importerThread_DoWork);
                importerThread.WorkerReportsProgress = true;
                importerThread.ProgressChanged += new ProgressChangedEventHandler(importerThread_ProgressChanged);
                importerThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(importerThread_RunWorkerCompleted);
                importerThread.WorkerSupportsCancellation = true;

                importerThread.RunWorkerAsync();

                return true;
            }

            return false;
        }

        private static void importerThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            m_AsyncCompletedDelegate.OnAsyncImportCompleted(true);
            m_AsyncCompletedDelegate = null;
        }

        private static void importerThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            m_AsyncCompletedDelegate.OnProgressChanged(e.ProgressPercentage);
        }

        private static void importerThread_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker importerThread = sender as BackgroundWorker;
            String[] files = Directory.GetFiles(m_ImportDirectory, "*.*", SearchOption.AllDirectories);
            int progress = 0;

            foreach (string filePath in files)
            {
                FileStream file = File.OpenRead(filePath);

                // Check if this is a FIT file or not
                if (WorkoutImporter.IsFITFileStream(file))
                {
                    WorkoutImporter.ImportWorkoutFromFIT(file);
                }
                else
                {
                    WorkoutImporter.ImportWorkout(file);
                }

                importerThread.ReportProgress(++progress);
            }
        }

        private static bool LoadWorkouts(XmlNode workoutsList)
        {
            for (int i = 0; i < workoutsList.ChildNodes.Count; ++i)
            {
                XmlNode child = workoutsList.ChildNodes.Item(i);

                if (child.Name == "Workout")
                {
                    GarminFitnessView pluginView = PluginMain.GetApplication().ActiveView as GarminFitnessView;
                    GarminWorkoutControl workoutControl = pluginView.GetCurrentView() as GarminWorkoutControl;
                    IActivityCategory category = PeekWorkoutCategory(child);
                    String workoutName = PeekWorkoutName(child);

                    workoutControl.GetNewWorkoutNameAndCategory(ref workoutName, ref category);

                    Workout newWorkout = GarminWorkoutManager.Instance.CreateWorkout(child, category);

                    if (newWorkout == null)
                    {
                        return false;
                    }

                    newWorkout.Name = workoutName;
                    newWorkout.Category = category;
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

                        return LoadWorkouts(folderList);
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
                            break;
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
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return category;
        }

        private static AsyncImportDelegate m_AsyncCompletedDelegate = null;
        private static String m_ImportDirectory = null;
    }
}
