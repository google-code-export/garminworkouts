using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;
using System.ComponentModel;

namespace GarminFitnessPlugin.Data
{
    abstract class ITarget : IPluginSerializable, IXMLSerializable, IDirty
    {
        protected ITarget(TargetType type, IStep parent)
        {
            Debug.Assert(type != TargetType.TargetTypeCount);
            m_Type = type;
            m_ParentStep = parent;
        }

        protected ITarget(Stream stream, DataVersion version, IStep parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(BitConverter.GetBytes((Int32)Type), 0, sizeof(Int32));
        }

        public abstract void SerializetoFIT(FITMessage message);

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
        }

        public virtual void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            XmlNode elementNode;
            XmlAttribute attribute;

            elementNode = document.CreateElement(nodeName);
            parentNode.AppendChild(elementNode);

            attribute = document.CreateAttribute(Constants.XsiTypeTCXString, Constants.xsins);
            attribute.Value = Constants.TargetTypeTCXString[(int)Type];
            elementNode.Attributes.Append(attribute);
        }

        public virtual void Deserialize(XmlNode parentNode)
        {
        }

        public abstract void HandleTargetOverride(XmlNode extensionNode);

        public abstract void Serialize(GarXFaceNet._Workout._Step step);
        public abstract void Deserialize(GarXFaceNet._Workout._Step step);

        public TargetType Type
        {
            get { return m_Type; }
        }

        protected void TriggerTargetChangedEvent(PropertyChangedEventArgs args)
        {
            if (TargetChanged != null)
            {
                TargetChanged(this, args);
            }
        }

        public IStep ParentStep
        {
            get { return m_ParentStep; }
        }

        public abstract bool IsDirty
        {
            get;
            set;
        }

        public virtual bool ContainsFITOnlyFeatures
        {
            get { return false; }
        }

        public enum TargetType
        {
            [ComboBoxStringProviderAttribute("NullTargetComboBoxText")]
            Null = 0,
            [ComboBoxStringProviderAttribute("SpeedTargetComboBoxText")]
            Speed,
            [ComboBoxStringProviderAttribute("CadenceTargetComboBoxText")]
            Cadence,
            [ComboBoxStringProviderAttribute("HeartRateTargetComboBoxText")]
            HeartRate,
            [ComboBoxStringProviderAttribute("PowerTargetComboBoxText")]
            Power,
            TargetTypeCount
        }

        public delegate void TargetChangedEventHandler(ITarget modifiedTarget, PropertyChangedEventArgs changedProperty);
        public event TargetChangedEventHandler TargetChanged;

        private TargetType m_Type;
        private IStep m_ParentStep;
    }
}
