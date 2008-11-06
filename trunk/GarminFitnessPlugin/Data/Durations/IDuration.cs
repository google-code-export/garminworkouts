using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    abstract class IDuration : IPluginSerializable, IXMLSerializable
    {
        protected IDuration(DurationType type, IStep parent)
        {
            Trace.Assert(type != DurationType.DurationTypeCount);
            m_Type = type;
            m_ParentStep = parent;
        }

        protected IDuration(Stream stream, DataVersion version, IStep parent)
        {
            m_ParentStep = parent;

            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(BitConverter.GetBytes((Int32)Type), 0, sizeof(Int32));
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
        }

        public virtual void Serialize(XmlNode parentNode, XmlDocument document)
        {
            XmlAttribute attribute = document.CreateAttribute("xsi", "type", Constants.xsins);

            attribute.Value = Constants.DurationTypeTCXString[(int)Type];
            parentNode.Attributes.Append(attribute);
        }

        public virtual bool Deserialize(XmlNode parentNode)
        {
            return true;
        }

        protected void TriggerDurationChangedEvent(PropertyChangedEventArgs args)
        {
            if (DurationChanged != null)
            {
                DurationChanged(this, args);
            }
        }

        public DurationType Type
        {
            get { return m_Type; }
        }

        public IStep ParentStep
        {
            get { return m_ParentStep; }
        }

        public enum DurationType
        {
            [ComboBoxStringProviderAttribute("LapButtonDurationComboBoxText")]
            [StepDescriptionStringProviderAttribute("LapButtonDurationDescriptionText")]
            LapButton = 0,
            [ComboBoxStringProviderAttribute("DistanceDurationComboBoxText")]
            [StepDescriptionStringProviderAttribute("DistanceDurationDescriptionText")]
            Distance,
            [ComboBoxStringProviderAttribute("TimeDurationComboBoxText")]
            [StepDescriptionStringProviderAttribute("TimeDurationDescriptionText")]
            Time,
            [ComboBoxStringProviderAttribute("HeartRateAboveDurationComboBoxText")]
            [StepDescriptionStringProviderAttribute("HeartRateAboveDurationDescriptionText")]
            HeartRateAbove,
            [ComboBoxStringProviderAttribute("HeartRateBelowDurationComboBoxText")]
            [StepDescriptionStringProviderAttribute("HeartRateBelowDurationDescriptionText")]
            HeartRateBelow,
            [ComboBoxStringProviderAttribute("CaloriesDurationComboBoxText")]
            [StepDescriptionStringProviderAttribute("CaloriesDurationDescriptionText")]
            Calories,
            DurationTypeCount
        }

        public delegate void DurationChangedEventHandler(IDuration modifiedDuration, PropertyChangedEventArgs changedProperty);
        public event DurationChangedEventHandler DurationChanged;

        private DurationType m_Type;
        private IStep m_ParentStep;
    }
}
