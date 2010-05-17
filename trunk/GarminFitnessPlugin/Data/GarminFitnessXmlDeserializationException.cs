using System;
using System.Xml;

namespace GarminFitnessPlugin.Data
{
    class GarminFitnessXmlDeserializationException : Exception
    {
        public GarminFitnessXmlDeserializationException(String errorMessage, XmlNode node)
            : base(errorMessage)
        {
            m_ErroneousNode = node;
        }

        public XmlNode ErroneousNode
        {
            get { return m_ErroneousNode; }
        }

        XmlNode m_ErroneousNode;
    }
}
