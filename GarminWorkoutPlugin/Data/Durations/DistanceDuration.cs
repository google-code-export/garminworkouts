using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class DistanceDuration : IDuration
    {
        public DistanceDuration(IStep parent)
            : base(DurationType.Distance, parent)
        {
            Distance = 1;
            m_DistanceUnit = parent.ParentWorkout.Category.DistanceUnits;
        }

        public DistanceDuration(double distanceToGo, Length.Units distanceUnit, IStep parent)
            : this(parent)
        {
            SetDistanceInUnits(distanceToGo, distanceUnit);
        }

        public DistanceDuration(Stream stream, DataVersion version, IStep parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            stream.Write(BitConverter.GetBytes(Distance), 0, sizeof(double));
            stream.Write(BitConverter.GetBytes((Int32)m_DistanceUnit), 0, sizeof(Int32));
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IDuration), stream, version);

            byte[] doubleBuffer = new byte[sizeof(double)];
            byte[] intBuffer = new byte[sizeof(Int32)];

            stream.Read(doubleBuffer, 0, sizeof(double));
            stream.Read(intBuffer, 0, sizeof(Int32));

            SetDistanceInUnits(BitConverter.ToDouble(doubleBuffer, 0),
                               (Length.Units)BitConverter.ToInt32(intBuffer, 0));
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            base.Serialize(parentNode, document);

            XmlNode childNode;

            childNode = document.CreateElement("Meters");
            childNode.AppendChild(document.CreateTextNode(DistanceInMeters.ToString()));

            parentNode.AppendChild(childNode);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if (base.Deserialize(parentNode))
            {
                if (parentNode.ChildNodes.Count == 1 && parentNode.FirstChild.Name == "Meters")
                {
                    XmlNode child = parentNode.FirstChild;

                    if (child.ChildNodes.Count == 1 && child.FirstChild.GetType() == typeof(XmlText) && Utils.IsTextFloatInRange(child.FirstChild.Value, 1, 65000))
                    {
                        SetDistanceInUnits(UInt16.Parse(child.FirstChild.Value), Length.Units.Meter);
                        return true;
                    }
                }
            }

            return false;
        }

        public UInt16 DistanceInMeters
        {
            get { return (UInt16)Length.Convert(m_Distance, m_DistanceUnit, Length.Units.Meter); }
        }

        private double Distance
        {
            get { return m_Distance; }
            set { m_Distance = value; }
        }

        public double GetDistanceInUnits(Length.Units distanceUnit)
        {
            return Length.Convert(m_Distance, m_DistanceUnit, distanceUnit);
        }

        public void SetDistanceInUnits(double distanceToGo, Length.Units distanceUnit)
        {
            if (distanceUnit == Length.Units.Mile)
            {
                Trace.Assert(distanceToGo >= 0.01 && distanceToGo <= 40.0);
            }
            else if (distanceUnit == Length.Units.Kilometer)
            {
                Trace.Assert(distanceToGo >= 0.01 && distanceToGo <= 65.0);
            }
            else
            {
                Trace.Assert(distanceToGo >= 1 && distanceToGo <= 65000);
            }

            m_DistanceUnit = distanceUnit;
            Distance = distanceToGo;
        }

        private double m_Distance;
        private Length.Units m_DistanceUnit;
    }
}
