using System;
using System.ComponentModel;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class CaloriesDuration : IDuration
    {
        public CaloriesDuration(IStep parent)
            : base(DurationType.Calories, parent)
        {
            CaloriesToSpend = 100;
        }

        public CaloriesDuration(UInt16 caloriesToSpend, IStep parent)
            : this(parent)
        {
            CaloriesToSpend = caloriesToSpend;
        }

        public CaloriesDuration(Stream stream, DataVersion version, IStep parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            m_CaloriesToSpend.Serialize(stream);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IDuration), stream, version);

            m_CaloriesToSpend.Deserialize(stream, version);
        }

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField durationType = message.GetExistingOrAddField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField durationValue = message.GetExistingOrAddField((Byte)FITWorkoutStepFieldIds.DurationValue);

            durationType.SetEnum((Byte)FITWorkoutStepDurationTypes.Calories);
            durationValue.SetUInt32((UInt32)CaloriesToSpend);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            base.Serialize(parentNode, nodeName, document);

            // This node was added by our parent...
            parentNode = parentNode.LastChild;

            m_CaloriesToSpend.Serialize(parentNode, "Calories", document);
        }

        public override void Deserialize(XmlNode parentNode)
                {
            base.Deserialize(parentNode);

            if(parentNode.ChildNodes.Count != 1 || parentNode.FirstChild.Name != "Calories")
                    {
                throw new GarminFitnessXmlDeserializationException("Missing information in calories duration XML node", parentNode);
            }

            m_CaloriesToSpend.Deserialize(parentNode.FirstChild);
        }

        public override void Serialize(GarXFaceNet._Workout._Step step)
        {
            step.SetDurationType(GarXFaceNet._Workout._Step.DurationTypes.CaloriesBurned);
            step.SetDurationValue(CaloriesToSpend);
        }

        public override void Deserialize(GarXFaceNet._Workout._Step step)
        {
            CaloriesToSpend = (UInt16)step.GetDurationValue();
        }

        public UInt16 CaloriesToSpend
        {
            get { return m_CaloriesToSpend; }
            set
            {
                if (CaloriesToSpend != value)
                {
                    m_CaloriesToSpend.Value = value;

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("CaloriesToSpend"));
                }
            }
        }

        private GarminFitnessUInt16Range m_CaloriesToSpend = new GarminFitnessUInt16Range(0, Constants.MinCalories, Constants.MaxCalories);
    }
}
