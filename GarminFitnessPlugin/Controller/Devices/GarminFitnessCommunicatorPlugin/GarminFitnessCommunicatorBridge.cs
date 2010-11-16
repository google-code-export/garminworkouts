using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Xml;
using System.Windows.Forms;

namespace GarminFitnessPlugin.Controller
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisibleAttribute(true)]
    public class GarminFitnessCommunicatorBridge
    {
        public class ExceptionEventArgs : EventArgs
        {
            public ExceptionEventArgs(string exceptionText)
            {
                m_ExceptionText = exceptionText;
            }

            public string ExceptionText
            {
                get { return m_ExceptionText; }
            }

            private string m_ExceptionText = String.Empty;
        }
        
        public class InitializeCompletedEventArgs : EventArgs
        {
            public InitializeCompletedEventArgs(bool success, string errorText)
            {
                m_Success = success;
                m_ErrorText = errorText;
            }

            public bool Success
            {
                get { return m_Success; }
            }

            public string ErrorText
            {
                get { return m_ErrorText; }
            }

            private bool m_Success = false;
            private string m_ErrorText = String.Empty;
        }

        public class FinishFindDevicesEventArgs : EventArgs
        {
            public FinishFindDevicesEventArgs(string devicesString)
            {
                m_DevicesString = devicesString;
            }

            public string DevicesString
            {
                get { return m_DevicesString; }
            }

            private string m_DevicesString = String.Empty;
        }

        public class TranferCompletedEventArgs : EventArgs
        {
            public TranferCompletedEventArgs(bool success, string dataString)
            {
                m_Success = success;
                m_DataString = dataString;
            }

            public bool Success
            {
                get { return m_Success; }
            }

            public string DataString
            {
                get { return m_DataString; }
            }

            private bool m_Success = false;
            private string m_DataString = String.Empty;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        [ComVisibleAttribute(true)]
        public class CommunicatorFileList : IList<String>
        {
#region IList<string> Members

            public int IndexOf(string item)
            {
                return m_List.IndexOf(item);
            }

            public void Insert(int index, string item)
            {
                m_List.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                m_List.RemoveAt(index);
            }

            public string this[int index]
            {
                get { return m_List[index]; }
                set { m_List[index] = value; }
            }

            public void Add(string item)
            {
                m_List.Add(item);
            }

            public void Clear()
            {
                m_List.Clear();
            }

            public bool Contains(string item)
            {
                return m_List.Contains(item);
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                m_List.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return m_List.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(string item)
            {
                return m_List.Remove(item);
            }

            public IEnumerator<string> GetEnumerator()
            {
                return m_List.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return m_List.GetEnumerator();
            }

#endregion

            private List<String> m_List = new List<String>();
        }
        
        public GarminFitnessCommunicatorBridge()
        {
            m_LocalWebPageLocation = typeof(PluginMain).Assembly.Location;
            m_LocalWebPageLocation = m_LocalWebPageLocation.Substring(0, m_LocalWebPageLocation.LastIndexOf('\\'));
            m_LocalWebPageLocation = m_LocalWebPageLocation.Substring(0, m_LocalWebPageLocation.LastIndexOf('\\'));
            m_LocalWebPageLocation += "\\Communicator\\";

            m_HiddenWebBrowser.ObjectForScripting = this;
            m_HiddenWebBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(OnWebBrowserDocumentCompleted);
            m_HiddenWebBrowser.Navigate(m_LocalWebPageLocation + "GarminFitness.html");
        }

        private void OnWebBrowserDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.LocalPath.Equals(m_LocalWebPageLocation + "GarminFitness.html"))
            {
                m_HiddenWebBrowser.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(OnWebBrowserDocumentCompleted);

                m_ControllerReady = true;
                if (ControllerReady != null)
                {
                    ControllerReady(this, new EventArgs());
                }
            }
        }

#region Web browser callbacks

        public void OnExceptionTriggered(String exception)
        {
            if (ExceptionTriggered != null)
            {
                ExceptionTriggered(this, new ExceptionEventArgs(exception));
            }
        }

        public void OnInitializeCompleted(bool success, string errorText)
        {
            if (InitializeCompleted != null)
            {
                InitializeCompleted(this, new InitializeCompletedEventArgs(success, errorText));
            }
        }

        public void OnStartFindDevices()
        {
        }

        public void OnFinishFindDevices(string devices)
        {
            if (FinishFindDevices != null)
            {
                FinishFindDevices(this, new FinishFindDevicesEventArgs(devices));
            }
        }

        public void OnProgressReadFromDevice(string percentage)
        {
        }

        public void OnFinishReadFromDevice(bool success, string dataString)
        {
            if (ReadFromDeviceCompleted != null)
            {
                ReadFromDeviceCompleted(this, new TranferCompletedEventArgs(success, dataString));
            }
        }

        public void OnFinishReadDirectory(bool success, string directoryString)
        {
            if (ReadDirectoryCompleted != null)
            {
                ReadDirectoryCompleted(this, new TranferCompletedEventArgs(success, directoryString));
            }
        }

        public void OnProgressWriteToDevice(string percentage)
        {
        }

        public void OnFinishWriteToDevice(bool success, string dataString)
        {
            if(WriteToDeviceCompleted != null)
            {
                WriteToDeviceCompleted(this, new TranferCompletedEventArgs(success, dataString));
            }
        }

        public void WriteLog(string log)
        {
            Logger.Instance.LogText(log);
        }

#endregion

        public void Initialize()
        {
            if (m_ControllerReady)
            {
                m_HiddenWebBrowser.Document.InvokeScript("Initialize");
            }
        }

        public void FindDevices()
        {
            m_HiddenWebBrowser.Document.InvokeScript("FindDevices");
        }

        public void SetDeviceNumber(int deviceNumber)
        {
		    m_HiddenWebBrowser.Document.InvokeScript("SetDeviceNumber",
                                                     new object[] { deviceNumber });
	    }

	    public void ReadProfileFromFitnessDevice()
        {
		    m_HiddenWebBrowser.Document.InvokeScript("ReadUserProfileFromFitnessDevice");
	    }

	    public void ReadWorkoutsFromFitnessDevice()
        {
		    m_HiddenWebBrowser.Document.InvokeScript("ReadWorkoutsFromFitnessDevice");
	    }

        public void CancelReadFromDevice()
        {
            m_HiddenWebBrowser.Document.InvokeScript("CancelReadFromDevice");
            m_HiddenWebBrowser.Document.InvokeScript("CancelReadFitnessDirectory");
        }

        public void CancelWriteToDevice()
        {
            m_HiddenWebBrowser.Document.InvokeScript("CancelWriteToDevice");
        }

	    public void WriteProfileToFitnessDevice(string tcxString, string fileName)
        {
		    m_HiddenWebBrowser.Document.InvokeScript("WriteUserProfileToFitnessDevice",
                                                     new object[] { tcxString, fileName });
	    }

        public void WriteWorkoutsToFitnessDevice(string tcxString, string fileName)
        {
            m_HiddenWebBrowser.Document.InvokeScript("WriteWorkoutsToFitnessDevice",
                                                     new object[] { tcxString, fileName });
        }

        public void WriteWorkoutsToFile(List<string> files, string destinationPath)
        {
            try
            {
                CommunicatorFileList filesList = GenerateCommunicatorFileExportList(files, destinationPath);

                string downloadXML = m_HiddenWebBrowser.Document.InvokeScript("BuildMultipleDeviceDownloadsXML",
                                                                              new object[] { filesList }) as string;

                m_HiddenWebBrowser.Document.InvokeScript("DownloadToDevice", new object[] { downloadXML });
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void GetWorkoutFiles()
        {
            m_HiddenWebBrowser.Document.InvokeScript("GetWorkoutFiles");
        }

        public void GetFITWorkoutFiles()
        {
            m_HiddenWebBrowser.Document.InvokeScript("ReadFITDirectory");
        }

        public void GetBinaryFile(string filePath)
        {
            try
            {
                m_HiddenWebBrowser.Document.InvokeScript("GetBinaryFile", new object[] { filePath });
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private CommunicatorFileList GenerateCommunicatorFileExportList(List<string> files, string destinationPath)
        {
            CommunicatorFileList result = new CommunicatorFileList();

            foreach (string filename in files)
            {
                string sourceFile = m_LocalWebPageLocation.Replace('\\', '/');

                sourceFile = sourceFile.Replace(':', '|');
                result.Add("file:///" + sourceFile + "temp/" + filename);
                result.Add(destinationPath.Replace('/', '\\') + "\\" + filename);
            }

            return result;
        }

        public bool IsControllerReady
        {
            get { return m_ControllerReady; }
        }

        public event EventHandler ControllerReady;
        public event EventHandler<ExceptionEventArgs> ExceptionTriggered;
        public event EventHandler<InitializeCompletedEventArgs> InitializeCompleted;
        public event EventHandler<FinishFindDevicesEventArgs> FinishFindDevices;
        public event EventHandler<TranferCompletedEventArgs> WriteToDeviceCompleted;
        public event EventHandler<TranferCompletedEventArgs> ReadFromDeviceCompleted;
        public event EventHandler<TranferCompletedEventArgs> ReadDirectoryCompleted;

        private WebBrowser m_HiddenWebBrowser = new WebBrowser();
        private static String m_LocalWebPageLocation;
        private bool m_ControllerReady = false;
    }
}
