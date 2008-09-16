using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;

namespace GarminWorkoutPlugin.Data
{
    abstract class IStep : IPluginSerializable, IXMLSerializable, IDirty
    {
        protected IStep(StepType type, Workout parent)
        {
            m_StepType = type;
            m_ParentWorkout = parent;
        }

        public enum StepType
        {
            Regular = 0,
            Repeat,
            StepTypeCount
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(BitConverter.GetBytes((Int32)Type), 0, sizeof(Int32));
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
        }

        public virtual void Serialize(XmlNode parentNode, XmlDocument document)
        {
            XmlAttribute attribute = document.CreateAttribute("xsi", "type", Constants.xsins);

            attribute.Value = Constants.StepTypeTCXString[(int)Type];
            parentNode.Attributes.Append(attribute);

            XmlNode idNode = document.CreateElement("StepId");
            idNode.AppendChild(document.CreateTextNode(Utils.GetStepExportId(this).ToString()));
            parentNode.AppendChild(idNode);
        }

        public virtual bool Deserialize(XmlNode parentNode)
        {
            return true;
        }

        public virtual byte GetStepCount()
        {
            return 1;
        }

        public abstract IStep Clone();
        public abstract bool ValidateAfterZoneCategoryChanged(IZoneCategory changedCategory);

        public StepType Type
        {
            get { return m_StepType; }
        }

        public Workout ParentWorkout
        {
            get { return m_ParentWorkout; }
        }

        public abstract bool IsDirty
        {
            get;
            set;
        }

        private StepType m_StepType;
        private Workout m_ParentWorkout;
    }
}
