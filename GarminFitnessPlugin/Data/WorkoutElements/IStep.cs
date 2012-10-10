using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    abstract class IStep : IPluginSerializable, IXMLSerializable, IDirty
    {
        protected IStep(StepType type, Workout parent)
        {
            m_StepType = type;
            ParentConcreteWorkout = parent;
        }

        public enum StepType
        {
            Regular = 0,
            Repeat,
            Link,
            StepTypeCount
        }

        public override void Serialize(Stream stream)
        {
            // Type
            stream.Write(BitConverter.GetBytes((Int32)Type), 0, sizeof(Int32));

            m_Notes.Serialize(stream);

            m_ForceSplit.Serialize(stream);
        }

        public virtual void SerializetoFIT(Stream stream)
        {
            FITMessage message = new FITMessage(FITGlobalMessageIds.WorkoutStep);
            FITMessageField stepId = new FITMessageField((Byte)FITWorkoutStepFieldIds.MessageIndex);
            FITMessageField durationType = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField durationValue = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationValue);
            FITMessageField intensity = new FITMessageField((Byte)FITWorkoutStepFieldIds.Intensity);
            FITMessageField stepName = new FITMessageField((Byte)FITWorkoutStepFieldIds.StepName);
            FITMessageField targetHigh = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetCustomValueHigh);
            FITMessageField targetLow = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetCustomValueLow);
            FITMessageField targetType = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetType);
            FITMessageField targetValue = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetValue);

            // Fill in invalid data
            durationType.SetEnum((Byte)0xFF);
            durationValue.SetUInt32(0xFFFFFFFF);
            intensity.SetEnum((Byte)0xFF);
            stepName.SetString("", 16);
            targetHigh.SetUInt32(0xFFFFFFFF);
            targetLow.SetUInt32(0xFFFFFFFF);
            targetType.SetEnum((Byte)0xFF);
            targetValue.SetUInt32(0xFFFFFFFF);

            stepId.SetUInt16((UInt16)(ParentWorkout.GetStepExportId(this) - 1));

            message.AddField(stepName);
            message.AddField(durationValue);
            message.AddField(targetValue);
            message.AddField(targetLow);
            message.AddField(targetHigh);
            message.AddField(stepId);
            message.AddField(durationType);
            message.AddField(targetType);
            message.AddField(intensity);

            FillFITStepMessage(message);

            message.Serialize(stream);
        }

        public abstract void DeserializeFromFIT(FITMessage stepMessage);

        public abstract void FillFITStepMessage(FITMessage message);

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
        }

        public void Deserialize_V6(Stream stream, DataVersion version)
        {
            m_Notes.Deserialize(stream, version);
        }

        public void Deserialize_V12(Stream stream, DataVersion version)
        {
            Deserialize_V6(stream, version);

            m_ForceSplit.Deserialize(stream, version);
        }

        public virtual void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            XmlAttribute attribute = document.CreateAttribute(Constants.XsiTypeTCXString, Constants.xsins);
            XmlNode childNode = document.CreateElement(nodeName);
            int stepExportId = ParentWorkout.GetStepExportId(this);

            parentNode.AppendChild(childNode);
            attribute.Value = Constants.StepTypeTCXString[(int)Type];
            childNode.Attributes.Append(attribute);

            XmlNode idNode = document.CreateElement("StepId");
            idNode.AppendChild(document.CreateTextNode(stepExportId.ToString()));
            childNode.AppendChild(idNode);

            XmlNode valueNode;
            XmlNode extensionNode;

            extensionNode = document.CreateElement("StepNotes");
            valueNode = document.CreateElement("StepId");
            valueNode.AppendChild(document.CreateTextNode(stepExportId.ToString()));
            extensionNode.AppendChild(valueNode);
            m_Notes.Serialize(extensionNode, "Notes", document);

            ParentWorkout.AddSportTracksExtension(extensionNode);
        }

        public virtual void Deserialize(XmlNode parentNode)
        {
        }

        public abstract UInt32 Serialize(GarXFaceNet._Workout workout, UInt32 stepIndex);
        public abstract void Deserialize(GarXFaceNet._Workout workout, UInt32 stepIndex);

        public virtual UInt16 StepCount
        {
            get
            {
                return 1;
            }
        }

        public abstract IStep Clone();
        public abstract bool ValidateAfterZoneCategoryChanged(IZoneCategory changedCategory);

        protected void TriggerStepChanged(PropertyChangedEventArgs args)
        {
            if (StepChanged != null)
            {
                StepChanged(this, args);
            }
        }

        public StepType Type
        {
            get { return m_StepType; }
        }

        public IWorkout ParentWorkout
        {
            get
            {
                if (ParentConcreteWorkout.GetSplitPartsCount() == 1)
                {
                    return ParentConcreteWorkout;
                }
                else
                {
                    UInt16 partIndex = (UInt16)(ParentConcreteWorkout.GetStepSplitPart(this) - 1);

                    return GarminWorkoutManager.Instance.CreateWorkoutPart(ParentConcreteWorkout, partIndex);
                }
            }
        }

        public virtual Workout ParentConcreteWorkout
        {
            get { return m_ParentWorkout; }
            set
            {
                m_ParentWorkout = value;
            }
        }

        public string Notes
        {
            get { return m_Notes; }
            set
            {
                if(m_Notes != value)
                {
                    m_Notes.Value = value;
                
                    if (StepChanged != null)
                    {
                        StepChanged(this, new PropertyChangedEventArgs("Notes"));
                    }
                }
            }
        }

        public bool ForceSplitOnStep
        {
            get
            {
                if (ParentConcreteWorkout.Steps.GetTopMostRepeatForStep(this) == null)
                {
                    return m_ForceSplit;
                }

                return false;
            }
            set 
            {
                if(ForceSplitOnStep != value)
                {
                    m_ForceSplit.Value = value;
                
                    if (StepChanged != null)
                    {
                        StepChanged(this, new PropertyChangedEventArgs("ForceSplitOnStep"));
                    }
                }
            }
        }

        public UInt16 SplitPartInWorkout
        {
            get { return ParentConcreteWorkout.GetStepSplitPart(this); }
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

        public virtual bool ContainsTCXExtensionFeatures
        {
            get { return false; }
        }

        public delegate void StepChangedEventHandler(IStep modifiedStep, PropertyChangedEventArgs changedProperty);
        public event StepChangedEventHandler StepChanged;

        private StepType m_StepType;
        private Workout m_ParentWorkout;
        private GarminFitnessString m_Notes = new GarminFitnessString();
        private GarminFitnessBool m_ForceSplit = new GarminFitnessBool(false);
   }
}
