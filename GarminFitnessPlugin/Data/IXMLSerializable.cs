using System;
using System.Xml;

namespace GarminFitnessPlugin.Data
{
    public interface IXMLSerializable
    {
        void Serialize(XmlNode parentNode, String nodeName, XmlDocument document);
        void Deserialize(XmlNode parentNode);
    }
}
