using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;

namespace GarminWorkoutPlugin.Data
{
    class CadenceRangeTarget : IConcreteCadenceTarget
    {
        public CadenceRangeTarget(BaseCadenceTarget baseTarget)
            : base(CadenceTargetType.Range, baseTarget)
        {
            m_MinCadence = 75;
            m_MaxCadence = 95;
        }

        public CadenceRangeTarget(Byte minCadence, Byte maxCadence, BaseCadenceTarget baseTarget)
            : this(baseTarget)
        {
            m_MinCadence = minCadence;
            m_MaxCadence = maxCadence;
        }

        public CadenceRangeTarget(Stream stream, DataVersion version, BaseCadenceTarget baseTarget)
            : this(baseTarget)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            stream.WriteByte(MinCadence);
            stream.WriteByte(MaxCadence);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IConcreteCadenceTarget), stream, version);

            SetValues((Byte)stream.ReadByte(), (Byte)stream.ReadByte());
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            base.Serialize(parentNode, document);

            CultureInfo culture = new CultureInfo("en-us");
            XmlNode childNode;

            // Low
            childNode = document.CreateElement("Low");
            childNode.AppendChild(document.CreateTextNode(String.Format(culture.NumberFormat, "{0:0.00000}", MinCadence)));
            parentNode.AppendChild(childNode);

            // High
            childNode = document.CreateElement("High");
            childNode.AppendChild(document.CreateTextNode(String.Format(culture.NumberFormat, "{0:0.00000}", MaxCadence)));
            parentNode.AppendChild(childNode);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if (base.Deserialize(parentNode))
            {
                double minCadence = 0;
                double maxCadence = 0;

                for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
                {
                    XmlNode child = parentNode.ChildNodes[i];
                    CultureInfo culture = new CultureInfo("en-us");

                    if (child.GetType() == typeof(XmlElement) && child.Name == "Low")
                    {
                        if (child.ChildNodes.Count == 1 && child.FirstChild.GetType() == typeof(XmlText))
                        {
                            minCadence = double.Parse(child.FirstChild.Value, culture.NumberFormat);
                        }
                    }
                    else if (child.GetType() == typeof(XmlElement) && child.Name == "High")
                    {
                        if (child.ChildNodes.Count == 1 && child.FirstChild.GetType() == typeof(XmlText))
                        {
                            maxCadence = double.Parse(child.FirstChild.Value, culture.NumberFormat);
                        }
                    }
                }

                if (minCadence > 0 && minCadence <= 254 && maxCadence > 0 && maxCadence <= 254)
                {
                    Byte min = (Byte)minCadence;
                    Byte max = (Byte)maxCadence;

                    // Just make sure there were no decimals in the number
                    if (min == minCadence && max == maxCadence)
                    {
                        // Reorder, GTC doesn't enforce
                        if (minCadence < maxCadence)
                        {
                            SetValues(min, max);
                        }
                        else
                        {
                            SetValues(max, min);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public void ValidateValues(Byte min, Byte max)
        {
            Trace.Assert(min <= max);
            Trace.Assert(max <= 254);
        }

        public void SetValues(Byte min, Byte max)
        {
            ValidateValues(min, max);

            m_MinCadence = min;
            m_MaxCadence = max;
        }

        public Byte MinCadence
        {
            get { return m_MinCadence; }
            set
            {
                ValidateValues(value, MaxCadence);
                m_MinCadence = value;
            }
        }

        public Byte MaxCadence
        {
            get { return m_MaxCadence; }
            set
            {
                ValidateValues(MinCadence, value);
                m_MaxCadence = value;
            }
        }

        public override bool IsDirty
        {
            get { return false; }
            set { Trace.Assert(false); }
        }

        private Byte m_MinCadence;
        private Byte m_MaxCadence;
    }
}
