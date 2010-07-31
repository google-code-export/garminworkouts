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
            m_ParentWorkout = parent;
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

            attribute.Value = Constants.StepTypeTCXString[(int)Type];
            parentNode.Attributes.Append(attribute);

            XmlNode idNode = document.CreateElement("StepId");
            idNode.AppendChild(document.CreateTextNode(ParentWorkout.GetStepExportId(this).ToString()));
            parentNode.AppendChild(idNode);

            // Extension
            XmlNode valueNode;
            XmlNode extensionNode;

            extensionNode = document.CreateElement("StepNotes");
            valueNode = document.CreateElement("StepId");
            valueNode.AppendChild(document.CreateTextNode(ParentWorkout.GetStepExportId(this).ToString()));
            extensionNode.AppendChild(valueNode);
            m_Notes.Serialize(extensionNode, "Notes", document);

            ParentConcreteWorkout.AddSportTracksExtension(extensionNode);
        }

        public virtual void Deserialize(XmlNode parentNode)
        {
        }

        public abstract UInt32 Serialize(GarXFaceNet._Workout workout, UInt32 stepIndex);
        public abstract void Deserialize(GarXFaceNet._Workout workout, UInt32 stepIndex);

        public virtual byte GetStepCount()
        {
            return 1;
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

        protected void TriggerDurationChangedEvent(IDuration duration, PropertyChangedEventArgs args)
        {
            Debug.Assert(Type == StepType.Regular);

            if (DurationChanged != null)
            {
                DurationChanged((RegularStep)this, duration, args);
            }
        }

        protected void TriggerTargetChangedEvent(ITarget target, PropertyChangedEventArgs args)
        {
            Debug.Assert(Type == StepType.Regular);

            if (TargetChanged != null)
            {
                TargetChanged((RegularStep)this, target, args);
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
                    UInt16 partIndex = ParentConcreteWorkout.GetStepSplitPart(this);

                    return GarminWorkoutManager.Instance.CreateWorkoutPart(ParentConcreteWorkout, partIndex);
                }
            }
        }

        public Workout ParentConcreteWorkout
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
                if (ParentConcreteWorkout.GetTopMostRepeatForStep(this) == null)
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

        public delegate void StepChangedEventHandler(IStep modifiedStep, PropertyChangedEventArgs changedProperty);
        public event StepChangedEventHandler StepChanged;

        public delegate void StepDurationChangedEventHandler(RegularStep modifiedStep, IDuration modifiedDuration, PropertyChangedEventArgs changedProperty);
        public event StepDurationChangedEventHandler DurationChanged;

        public delegate void StepTargetChangedEventHandler(RegularStep modifiedStep, ITarget modifiedTarget, PropertyChangedEventArgs changedProperty);
        public event StepTargetChangedEventHandler TargetChanged;

        private StepType m_StepType;
        private Workout m_ParentWorkout;
        private GarminFitnessString m_Notes = new GarminFitnessString();
        private GarminFitnessBool m_ForceSplit = new GarminFitnessBool(false);
   }
}
