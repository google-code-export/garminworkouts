using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    public abstract class IDuration : IPluginSerializable, IXMLSerializable
    {
        protected IDuration(DurationType type, IStep parent)
        {
            Debug.Assert(type != DurationType.DurationTypeCount);
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

        public abstract void FillFITStepMessage(FITMessage message);

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
        }

        public virtual void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            XmlNode elementNode;
            XmlAttribute attribute;

            elementNode = document.CreateElement(nodeName);
            parentNode.AppendChild(elementNode);

            attribute = document.CreateAttribute(Constants.XsiTypeTCXString, Constants.xsins);
            attribute.Value = Constants.DurationTypeTCXString[(int)Type];
            elementNode.Attributes.Append(attribute);
        }

        public virtual void Deserialize(XmlNode parentNode)
        {
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

        public virtual bool ContainsFITOnlyFeatures
        {
            get { return false; }
        }

        public bool ContainsTCXExtensionFeatures
        {
            get { return false; }
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
            [ComboBoxStringProviderAttribute("PowerAboveDurationComboBoxText")]
            [StepDescriptionStringProviderAttribute("PowerAboveDurationDescriptionText")]
            PowerAbove,
            [ComboBoxStringProviderAttribute("PowerBelowDurationComboBoxText")]
            [StepDescriptionStringProviderAttribute("PowerBelowDurationDescriptionText")]
            PowerBelow,
            DurationTypeCount
        }

        public delegate void DurationChangedEventHandler(IDuration modifiedDuration, PropertyChangedEventArgs changedProperty);
        public event DurationChangedEventHandler DurationChanged;

        private DurationType m_Type;
        private IStep m_ParentStep;
    }
}
