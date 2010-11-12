using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Visuals.Fitness;
using SportTracksPluginFramework;
using GarminFitnessPlugin.Data;
using GarminFitnessPlugin.View;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin
{
    class PluginMain : STFrameworkEntryPoint, IPlugin
    {
        public PluginMain() : base(GUIDs.PluginMain)
        {
            STFrameworkEntryPoint.LogbookChanged += new LogbookChangedEventHandler(OnLogbookChanged);
        }

        private void OnLogbookChanged(object sender, ILogbook oldLogbook, ILogbook newLogbook)
        {
            LoadWorkoutsFromLogbook(newLogbook);
        }

#region STFrameworkEntryPoint Members

        public override string Name
        {
            get { return GarminFitnessView.GetLocalizedString("GarminFitnessText"); }
        }

        public override void ReadOptions(XmlDocument xmlDoc, XmlNamespaceManager nsmgr, XmlElement pluginNode)
        {
            m_PluginOptions = pluginNode;
        }

        public override void WriteOptions(XmlDocument xmlDoc, XmlElement pluginNode)
        {
            Options.Instance.Serialize(pluginNode, "", xmlDoc);
        }

        protected override void Initialize()
        {
        }

        public override string DonationLink
        {
            get { return "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=RLEX96JT6JRCQ"; }
        }

        public override Control DonationReminderControl
        {
            get
            {
                if (m_ReminderControl == null)
                {
                    m_ReminderControl = new GarminFitnessDonationReminderControl();
                }

                return m_ReminderControl;
            }
        }

#endregion

        private void LoadWorkoutsFromLogbook(ILogbook logbook)
        {
            // Load data from logbook
            byte[] extensionData = logbook.GetExtensionData(GUIDs.PluginMain);

            if (extensionData != null && extensionData.Length > 0)
            {
                try
                {
                    MemoryStream stream = new MemoryStream(extensionData);
                    byte[] headerBuffer = new byte[Constants.DataHeaderIdString.Length];
                    String headerIdString;
                    DataVersion version = new DataVersion(0);

                    stream.Read(headerBuffer, 0, Constants.DataHeaderIdString.Length);
                    headerIdString = Encoding.UTF8.GetString(headerBuffer);

                    if (headerIdString == Constants.DataHeaderIdString)
                    {
                        Byte versionNumber = (Byte)stream.ReadByte();

                        if (versionNumber <= Constants.CurrentVersion.VersionNumber)
                        {
                            version = new DataVersion(versionNumber);
                        }
                        else
                        {
                            MessageBox.Show(GarminFitnessView.GetLocalizedString("DataTooRecentErrorText"),
                                            GarminFitnessView.GetLocalizedString("ErrorText"),
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);

                            return;
                        }
                    }
                    else
                    {
                        // Deserialize using version 0.  Replace at start since we skipped some data
                        stream.Position = 0;
                    }

                    Options.Instance.Deserialize(stream, version);
                    GarminWorkoutManager.Instance.Deserialize(stream, version);
                    GarminProfileManager.Instance.Deserialize(stream, version);

                    if (m_PluginOptions != null)
                    {
                        Options.Instance.Deserialize(m_PluginOptions);
                        m_PluginOptions = null;
                    }

                    stream.Close();
                }
                catch (Exception e)
                {
                     throw e;
                }
            }
        }

        private GarminFitnessDonationReminderControl m_ReminderControl = null;
        private XmlNode m_PluginOptions;
    }
}
