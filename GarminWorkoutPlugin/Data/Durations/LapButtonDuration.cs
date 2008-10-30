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

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IDuration), stream, version);
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            base.Serialize(parentNode, document);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            return base.Deserialize(parentNode);
        }
    }
}
