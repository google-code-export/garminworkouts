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
    public class WorkoutImporter
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

        public static FITFileTypes PeekFITFileType(Stream fileStream)
        {
            if (FITParser.Instance.Init(fileStream))
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

                                if (fileTypeField != null)
                                {
                                    return (FITFileTypes)fileTypeField.GetEnum();
                                }

                                break;
                            }
                            default:
                            {
                                break;
                            }
                        }
                    }
                }
                while (parsedMessage != null);
            }

            return FITFileTypes.Invalid;
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

        public static Workout ImportWorkoutFromFIT(Stream importStream, out UInt32 workoutId)
        {
            workoutId = UInt32.MaxValue;

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
                                        FITMessageField fileId = parsedMessage.GetField((Byte)FITFileIdFieldsIds.ExportDate);

                                        if (fileTypeField == null ||
                                            fileId == null ||
                                            (FITFileTypes)fileTypeField.GetEnum() != FITFileTypes.Workout)
                                        {
                                            Logger.Instance.LogText("Not a workout FIT file");

                                            FITParser.Instance.Close();
                                        }

                                        workoutId = fileId.GetUInt32();
                                        break;
                                    }
                                case FITGlobalMessageIds.Workout:
                                    {
                                        // Make sure we have a valid file header
                                        if (workoutId != UInt32.MaxValue)
                                        {
                                            return ImportWorkoutFromMessage(parsedMessage);
                                        }
                                        else
                                        {
                                            throw new FITParserException("No ID for workout");
                                        }
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
                }
                else
                {
                    Logger.Instance.LogText("FIT parser init failed");
                }
            }
            catch (FITParserException e)
            {
                MessageBox.Show("Error parsing FIT file\n\n" +
                                e.Message + "\n\n" +
                                e.StackTrace);
            }
            catch (Exception e)
            {
                MessageBox.Show("General error on import\n\n" +
                                e.Message + "\n\n" +
                                e.StackTrace);
            }
            finally
            {
                FITParser.Instance.Close();
            }

            return null;
        }

        public static Workout ImportWorkoutFromMessage(FITMessage workoutMessage)
        {
            return ImportWorkoutFromMessage(workoutMessage, null);
        }

        public static Workout ImportWorkoutFromMessage(FITMessage workoutMessage, IActivityCategory category)
        {
            // Peek name
            FITMessageField nameField = workoutMessage.GetField((Byte)FITWorkoutFieldIds.WorkoutName);

            if (nameField != null)
            {
                GarminFitnessView pluginView = PluginMain.GetApplication().ActiveView as GarminFitnessView;
                String workoutName = nameField.GetString();

                if (category == null && pluginView != null)
                {
                    GarminWorkoutControl workoutControl = pluginView.GetCurrentView() as GarminWorkoutControl;

                    if (workoutControl != null)
                    {
                        workoutControl.GetNewWorkoutNameAndCategory(ref workoutName, ref category);
                    }
                }

                return GarminWorkoutManager.Instance.CreateWorkout(workoutName, workoutMessage, category);
            }
            else
            {
                throw new FITParserException("No name for workout");
            }
        }

        public static bool ImportSchedulesFromFIT(Stream importStream, Dictionary<UInt32, Workout> workoutIdMap)
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
                                            (FITFileTypes)fileTypeField.GetEnum() != FITFileTypes.Schedules)
                                        {
                                            Logger.Instance.LogText("Not a schedule FIT file");

                                            FITParser.Instance.Close();
                                            return false;
                                        }

                                        break;
                                    }
                                case FITGlobalMessageIds.WorkoutSchedules:
                                    {
                                        FITMessageField workoutId = parsedMessage.GetField((Byte)FITScheduleFieldIds.WorkoutId);

                                        if (workoutIdMap.ContainsKey(workoutId.GetUInt32()))
                                        {
                                            DateTime scheduleDate = ImportWorkoutScheduleMessage(parsedMessage);

                                            workoutIdMap[workoutId.GetUInt32()].ConcreteWorkout.ScheduleWorkout(scheduleDate);
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

        public static DateTime ImportWorkoutScheduleMessage(FITMessage scheduleMessage)
        {
            // Make sure we have a workout file
            FITMessageField scheduledDate = scheduleMessage.GetField((Byte)FITScheduleFieldIds.ScheduledDate);
            UInt32 secondsSinceReference = scheduledDate.GetUInt32();

            return new DateTime(1989, 12, 31) + new TimeSpan(0, 0, (int)secondsSinceReference);
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
            String FITSchedulesFileName = String.Empty;
            String[] files = Directory.GetFiles(m_ImportDirectory, "*.*", SearchOption.AllDirectories);
            Dictionary<UInt32, Workout> workoutIdMap = new Dictionary<UInt32, Workout>();
            int progress = 0;

            foreach (string filePath in files)
            {
                FileStream file = File.OpenRead(filePath);

                // Check if this is a FIT file or not
                if (WorkoutImporter.IsFITFileStream(file))
                {
                    if (WorkoutImporter.PeekFITFileType(file) == FITFileTypes.Schedules)
                    {
                        // Import schedules last since it refers data contained in the workouts.
                        //  Keep the filename to delay the loading of this file
                        FITSchedulesFileName = filePath;
                    }
                    else
                    {
                        UInt32 workoutId;
                        Workout resultWorkout = WorkoutImporter.ImportWorkoutFromFIT(file, out workoutId);

                        if (resultWorkout != null)
                        {
                            workoutIdMap.Add(workoutId, resultWorkout);
                        }
                    }
                }
                else
                {
                    WorkoutImporter.ImportWorkout(file);
                }

                // Import schedules last since it refers data contained in the workouts
                if (!String.IsNullOrEmpty(FITSchedulesFileName))
                {
                    Stream workoutStream = File.OpenRead(FITSchedulesFileName);

                    WorkoutImporter.ImportSchedulesFromFIT(workoutStream, workoutIdMap);
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
