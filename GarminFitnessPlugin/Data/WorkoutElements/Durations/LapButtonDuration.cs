using System;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    public class LapButtonDuration : IDuration
    {
        public LapButtonDuration(IStep parent)
            : base(DurationType.LapButton, parent)
        {
        }

        public LapButtonDuration(Stream stream, DataVersion version, IStep parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);
        }

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField durationType = message.GetExistingOrAddField((Byte)FITWorkoutStepFieldIds.DurationType);

            durationType.SetEnum((Byte)FITWorkoutStepDurationTypes.Open);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IDuration), stream, version);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            base.Serialize(parentNode, nodeName, document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);
        }
    }
}
