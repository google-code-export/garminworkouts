using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.View;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class ProfileImporter
    {
        public interface AsyncImportDelegate
        {
            void OnAsyncImportCompleted(bool success);
            void OnProgressChanged(int progressPercent);
        }

        public static bool ImportProfile(Stream importStream)
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
                        for(int j = 0; j < database.ChildNodes.Count; ++j)
                        {
                            if (database.ChildNodes[j].Name == Constants.ExtensionsTCXString)
                            {
                                XmlNode extensionsNode = database.ChildNodes[j];

                                if (extensionsNode.ChildNodes.Count == 1 && extensionsNode.FirstChild.Name == Constants.ProfileTCXString)
                                {
                                    GarminProfileManager.Instance.Deserialize(extensionsNode.FirstChild);
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (GarminFitnessXmlDeserializationException e)
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

        public static bool ImportProfileFromFIT(Stream importStream)
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
                                        // Make sure we have a profile file (settings or sport)
                                        FITMessageField fileTypeField = parsedMessage.GetField((Byte)FITFileIdFieldsIds.FileType);

                                        if (fileTypeField != null &&
                                            ((FITFileTypes)fileTypeField.GetEnum() == FITFileTypes.Settings ||
                                             (FITFileTypes)fileTypeField.GetEnum() == FITFileTypes.Sport))
                                        {
                                            GarminProfileManager.Instance.UserProfile.DeserializeFromFIT(importStream);

                                            FITParser.Instance.Close();
                                            return true;
                                        }
                                        else
                                        {
                                            Logger.Instance.LogText("Not a profile FIT file");
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
                    return false;
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

                Debug.Assert(filePath.EndsWith(FITConstants.FITFileDescriptor, StringComparison.OrdinalIgnoreCase));

                ProfileImporter.ImportProfileFromFIT(file);

                importerThread.ReportProgress(++progress);
            }
        }

        private static AsyncImportDelegate m_AsyncCompletedDelegate = null;
        private static String m_ImportDirectory = null;
    }
}
