using System;
using System.IO;
using System.Windows.Forms;

namespace GarminFitnessPlugin.Controller
{
    class Logger
    {
        private Logger()
        {
            String logPath = typeof(PluginMain).Assembly.Location;
            logPath = logPath.Substring(0, logPath.LastIndexOf('\\'));

            m_LogFile = File.CreateText(logPath + "\\GF_Log.txt");
            m_LogFile.AutoFlush = true;

            //MessageBox.Show((m_LogFile.BaseStream as FileStream).Name);
        }

        ~Logger()
        {
            m_LogFile = null;
        }

        public static Logger Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new Logger();
                }

                return m_Instance;
            }
        }

        public void LogText(String textToLog)
        {
            if (m_LogFile != null &&
                m_LogFile.BaseStream != null &&
                Options.Instance.EnableDebugLog)
            {
                m_LogFile.WriteLine(String.Format("{0} : {1}", DateTime.Now.ToLongTimeString(), textToLog));
            }
        }

        private static Logger m_Instance = null;

        private StreamWriter m_LogFile = null;
    }
}
