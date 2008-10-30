using System.IO;
using System.Xml;

namespace GarminWorkoutPlugin.Data
{
    class GarminExtendedActivityProfile : GarminActivityProfile
    {
        public GarminExtendedActivityProfile(GarminCategories category) :
            base(category)
        {
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);
        }

        public new void Deserialize_V8(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(GarminActivityProfile), stream, version);


        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            return false;
        }
    }
}
