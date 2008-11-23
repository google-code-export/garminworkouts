using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace GarminFitnessPlugin.Data
{
    class NullTarget : ITarget
    {
        public NullTarget(IStep parent)
            : base(TargetType.Null, parent)
        {
        }

        public NullTarget(Stream stream, DataVersion version, IStep parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);
        }

        public override void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(ITarget), stream, version);
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            base.Serialize(parentNode, document);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            return base.Deserialize(parentNode);
        }

        public override void HandleTargetOverride(XmlNode extensionNode)
        {
        }

        public override bool IsDirty
        {
            get { return false; }
            set { }
        }
    }
}
