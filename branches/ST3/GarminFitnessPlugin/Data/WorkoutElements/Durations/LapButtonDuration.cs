using System;
using System.IO;
using System.Xml;

namespace GarminFitnessPlugin.Data
{
    class LapButtonDuration : IDuration
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

        public override void SerializetoFIT(Stream stream)
        {
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

        public override void Serialize(GarXFaceNet._Workout._Step step)
        {
            step.SetDurationType(GarXFaceNet._Workout._Step.DurationTypes.Open);
        }

        public override void Deserialize(GarXFaceNet._Workout._Step step)
        {
        }
    }
}
