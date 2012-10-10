using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using GarminFitnessPlugin.Controller;

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

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField targetType = message.GetExistingOrAddField((Byte)FITWorkoutStepFieldIds.TargetType);

            targetType.SetEnum((Byte)FITWorkoutStepTargetTypes.NoTarget);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(ITarget), stream, version);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            base.Serialize(parentNode, nodeName, document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);
        }

        public override void HandleTargetOverride(XmlNode extensionNode)
        {
        }

        public override void Serialize(GarXFaceNet._Workout._Step step)
        {
            step.SetTargetType(2);
        }

        public override void Deserialize(GarXFaceNet._Workout._Step step)
        {
        }

        public override bool IsDirty
        {
            get { return false; }
            set { }
        }
    }
}
