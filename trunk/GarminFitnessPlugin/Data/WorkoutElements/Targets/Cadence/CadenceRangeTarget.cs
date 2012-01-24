using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    public class CadenceRangeTarget : BaseCadenceTarget.IConcreteCadenceTarget
    {
        public CadenceRangeTarget(BaseCadenceTarget baseTarget)
            : base(CadenceTargetType.Range, baseTarget)
        {
        }

        public CadenceRangeTarget(Byte minCadence, Byte maxCadence, BaseCadenceTarget baseTarget)
            : this(baseTarget)
        {
            SetValues(minCadence, maxCadence);
        }

        public CadenceRangeTarget(Stream stream, DataVersion version, BaseCadenceTarget baseTarget)
            : this(baseTarget)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            m_MinCadence.Serialize(stream);
            m_MaxCadence.Serialize(stream);
        }

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField cadenceZone = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetValue);
            FITMessageField minCadence = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetCustomValueLow);
            FITMessageField maxCadence = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetCustomValueHigh);

            cadenceZone.SetUInt32((Byte)0);
            message.AddField(cadenceZone);
            minCadence.SetUInt32((UInt32)MinCadence);
            message.AddField(minCadence);
            maxCadence.SetUInt32((UInt32)MaxCadence);
            message.AddField(maxCadence);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseCadenceTarget.IConcreteCadenceTarget), stream, version);

            SetValues((Byte)stream.ReadByte(), (Byte)stream.ReadByte());
        }

        public void Deserialize_V10(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseCadenceTarget.IConcreteCadenceTarget), stream, version);

            // It was decided to move teh cadence range from Byte to Double because they are
            //  serialized as double in the XML.  This is complete non-sense becasue a fraction
            //  of a pedal stroke doesn't really make sense for a target.  Anyways all
            //  interfaces use Byte so it's hidden to the end user
            m_MinCadence.Deserialize(stream, version);
            m_MaxCadence.Deserialize(stream, version);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            base.Serialize(parentNode, nodeName, document);

            m_MinCadence.Serialize(parentNode, "Low", document);
            m_MaxCadence.Serialize(parentNode, "High", document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            bool minRead = false;
            bool maxRead = false;

            base.Deserialize(parentNode);

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode child = parentNode.ChildNodes[i];

                if (child.GetType() == typeof(XmlElement) && child.Name == "Low")
                {
                    m_MinCadence.Deserialize(child);
                    minRead = true;
                }
                else if (child.GetType() == typeof(XmlElement) && child.Name == "High")
                {
                    m_MaxCadence.Deserialize(child);
                    maxRead = true;
                }
            }

            // Just make sure there were no decimals in the number
            if (!minRead || !maxRead)
            {
                throw new GarminFitnessXmlDeserializationException("Missing information in cadence range target XML node", parentNode);
            }

            // Reorder, GTC doesn't enforce
            SetValues(Math.Min(MinCadence, MaxCadence), Math.Max(MinCadence, MaxCadence));
        }

        public void ValidateValues(Byte min, Byte max)
        {
            Debug.Assert(min <= max);
            Debug.Assert(m_MinCadence.IsInRange(min));
            Debug.Assert(m_MaxCadence.IsInRange(max));
        }

        public void SetMinCadence(Byte min)
        {
            ValidateValues(min, MaxCadence);

            MinCadence = min;
        }

        public void SetMaxCadence(Byte max)
        {
            ValidateValues(MinCadence, max);

            MaxCadence = max;
        }

        public void SetValues(Byte min, Byte max)
        {
            ValidateValues(min, max);

            MinCadence = min;
            MaxCadence = max;
        }

        public Byte MinCadence
        {
            get { return (Byte)m_MinCadence; }
            private set
            {
                if (MinCadence != value)
                {
                    m_MinCadence.Value = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("MaxCadence"));
                }
            }
        }

        public Byte MaxCadence
        {
            get { return (Byte)m_MaxCadence; }
            private set
            {
                if (MaxCadence != value)
                {
                    m_MaxCadence.Value = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("MaxCadence"));
                }
            }
        }

        public override bool IsDirty
        {
            get { return false; }
            set { Debug.Assert(false); }
        }

        private GarminFitnessDoubleRange m_MinCadence = new GarminFitnessDoubleRange(75, Constants.MinCadence, Constants.MaxCadence);
        private GarminFitnessDoubleRange m_MaxCadence = new GarminFitnessDoubleRange(95, Constants.MinCadence, Constants.MaxCadence);
    }
}
