using System.IO;
using System.Xml;

namespace GarminFitnessPlugin.Data
{
    class GarminActivityProfile : IPluginSerializable, IXMLSerializable
    {
        public GarminActivityProfile(GarminCategories category)
        {
            m_Category = category;
        }

        public override void Serialize(Stream stream)
        {
        }

        public void Deserialize_V8(Stream stream, DataVersion version)
        {
        }

        public virtual void Serialize(XmlNode parentNode, XmlDocument document)
        {
        }

        public virtual bool Deserialize(XmlNode parentNode)
        {
            return false;
        }

        private GarminCategories m_Category;
    }
}
