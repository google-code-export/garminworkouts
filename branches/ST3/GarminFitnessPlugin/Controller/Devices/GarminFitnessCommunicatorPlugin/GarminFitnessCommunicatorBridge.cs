using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
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
        
        public GarminFitnessCommunicatorBridge()
        {
            m_LocalWebPageLocation = typeof(PluginMain).Assembly.Location;
            m_LocalWebPageLocation = m_LocalWebPageLocation.Substring(0, m_LocalWebPageLocation.LastIndexOf('\\'));
            m_LocalWebPageLocation = m_LocalWebPageLocation.Substring(0, m_LocalWebPageLocation.LastIndexOf('\\'));
            m_LocalWebPageLocation += "\\Communicator\\GarminFitness.html";

            m_HiddenWebBrowser.ObjectForScripting = this;
            m_HiddenWebBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(OnWebBrowserDocumentCompleted);
            m_HiddenWebBrowser.Navigate(m_LocalWebPageLocation);
        }

        private void OnWebBrowserDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.LocalPath.Equals(m_LocalWebPageLocation))
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

        public void OnProgressReadFromDevice(int percentage)
        {
        }


        public void OnFinishReadFromDevice(bool success, string dataString)
        {
            if (ReadFromDeviceCompleted != null)
            {
                ReadFromDeviceCompleted(this, new TranferCompletedEventArgs(success, dataString));
            }
        }

        public void OnProgressWriteToDevice(int percentage)
        {
        }

        public void OnFinishWriteToDevice(bool success, string dataString)
        {
            if(WriteToDeviceCompleted != null)
            {
                WriteToDeviceCompleted(this, new TranferCompletedEventArgs(success, dataString));
            }
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

        private WebBrowser m_HiddenWebBrowser = new WebBrowser();
        private static String m_LocalWebPageLocation;
        private bool m_ControllerReady = false;
    }
}
