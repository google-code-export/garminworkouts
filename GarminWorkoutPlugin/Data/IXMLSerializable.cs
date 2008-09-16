using System.IO;
using System.Xml;

namespace GarminWorkoutPlugin.Data
{
    interface IXMLSerializable
    {
        void Serialize(XmlNode parentNode, XmlDocument document);
        bool Deserialize(XmlNode parentNode);
    }
}
