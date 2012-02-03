using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    abstract class IRepeatDuration : IPluginSerializable, IXMLSerializable
    {
        protected IRepeatDuration(RepeatDurationType type, RepeatStep parent)
        {
            Debug.Assert(type != RepeatDurationType.RepeatDurationTypeCount);
            m_Type = type;
            m_ParentStep = parent;
        }

        protected IRepeatDuration(Stream stream, DataVersion version, RepeatStep parent)
        {
            m_ParentStep = parent;

            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(BitConverter.GetBytes((Int32)Type), 0, sizeof(Int32));
        }

        public abstract void Serialize(XmlNode parentNode, String nodeName, XmlDocument document);
        public abstract void Deserialize(XmlNode parentNode);

        public abstract void FillFITStepMessage(FITMessage message);

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
        }

        protected void TriggerDurationChangedEvent(PropertyChangedEventArgs args)
        {
            if (DurationChanged != null)
            {
                DurationChanged(this, args);
            }
        }

        public RepeatDurationType Type
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

        public enum RepeatDurationType
        {
            [ComboBoxStringProviderAttribute("RepeatCountComboBoxText")]
            [StepDescriptionStringProviderAttribute("RepeatStepDescriptionText")]
            RepeatCount = 0,
            [ComboBoxStringProviderAttribute("DistanceDurationComboBoxText")]
            [StepDescriptionStringProviderAttribute("RepeatUntilDistanceDurationDescriptionText")]
            RepeatUntilDistance,
            [ComboBoxStringProviderAttribute("TimeDurationComboBoxText")]
            [StepDescriptionStringProviderAttribute("RepeatUntilTimeDurationDescriptionText")]
            RepeatUntilTime,
            [ComboBoxStringProviderAttribute("HeartRateAboveDurationComboBoxText")]
            [StepDescriptionStringProviderAttribute("RepeatUntilHeartRateAboveDurationDescriptionText")]
            RepeatUntilHeartRateAbove,
            [ComboBoxStringProviderAttribute("HeartRateBelowDurationComboBoxText")]
            [StepDescriptionStringProviderAttribute("RepeatUntilHeartRateBelowDurationDescriptionText")]
            RepeatUntilHeartRateBelow,
            [ComboBoxStringProviderAttribute("CaloriesDurationComboBoxText")]
            [StepDescriptionStringProviderAttribute("RepeatUntilCaloriesDurationDescriptionText")]
            RepeatUntilCalories,
            [ComboBoxStringProviderAttribute("PowerAboveDurationComboBoxText")]
            [StepDescriptionStringProviderAttribute("RepeatUntilPowerAboveDurationDescriptionText")]
            RepeatUntilPowerAbove,
            [ComboBoxStringProviderAttribute("PowerBelowDurationComboBoxText")]
            [StepDescriptionStringProviderAttribute("RepeatUntilPowerBelowDurationDescriptionText")]
            RepeatUntilPowerBelow,
            RepeatDurationTypeCount
        }

        public delegate void DurationChangedEventHandler(IRepeatDuration modifiedDuration, PropertyChangedEventArgs changedProperty);
        public event DurationChangedEventHandler DurationChanged;

        private RepeatDurationType m_Type;
        private IStep m_ParentStep;
    }
}
