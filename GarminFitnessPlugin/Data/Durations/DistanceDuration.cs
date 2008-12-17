using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
            DistanceUnit = BaseUnit;
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
            stream.Write(BitConverter.GetBytes((Int32)DistanceUnit), 0, sizeof(Int32));
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
                    CultureInfo culture = new CultureInfo("en-us");

                    if (child.ChildNodes.Count == 1 && child.FirstChild.GetType() == typeof(XmlText) &&
                        Utils.IsTextFloatInRange(child.FirstChild.Value, Constants.MinDistanceMeters, Constants.MaxDistanceMeters, culture))
                    {
                        SetDistanceInUnits(UInt16.Parse(child.FirstChild.Value), Length.Units.Meter);
                        return true;
                    }
                }
            }

            return false;
        }

        public double GetDistanceInBaseUnit()
        {
            return GetDistanceInUnits(BaseUnit);
        }

        public void SetDistanceInBaseUnit(double distanceToGo)
        {
            SetDistanceInUnits(distanceToGo, BaseUnit);
        }

        private double GetDistanceInUnits(Length.Units distanceUnit)
        {
            return Length.Convert(Distance, DistanceUnit, distanceUnit);
        }

        private void SetDistanceInUnits(double distanceToGo, Length.Units distanceUnit)
        {
            if (Utils.IsStatute(distanceUnit))
            {
                distanceToGo = Length.Convert(distanceToGo, distanceUnit, Length.Units.Mile);
                distanceUnit = Length.Units.Mile;

                Debug.Assert(distanceToGo >= Constants.MinDistance && distanceToGo <= Constants.MaxDistanceStatute);
            }
            else
            {
                distanceToGo = Length.Convert(distanceToGo, distanceUnit, Length.Units.Kilometer);
                distanceUnit = Length.Units.Kilometer;

                Debug.Assert(distanceToGo >= Constants.MinDistance && distanceToGo <= Constants.MaxDistanceMetric);
            }

            DistanceUnit = distanceUnit;
            Distance = distanceToGo;
        }

        private UInt16 DistanceInMeters
        {
            get { return (UInt16)Length.Convert(Distance, DistanceUnit, Length.Units.Meter); }
        }

        private Length.Units DistanceUnit
        {
            get { return m_DistanceUnit; }
            set
            {
                Debug.Assert(value == Length.Units.Kilometer || value == Length.Units.Mile);

                if (DistanceUnit != value)
                {
                    m_DistanceUnit = value;

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("DistanceUnit"));
                }
            }
        }

        private double Distance
        {
            get { return m_Distance; }
            set
            {
                if (Distance != value)
                {
                    m_Distance = value;

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("Distance"));
                }
            }
        }

        public Length.Units BaseUnit
        {
            get
            {
                if (Utils.IsStatute(ParentStep.ParentWorkout.Category.DistanceUnits))
                {
                    return Length.Units.Mile;
                }
                else
                {
                    return Length.Units.Kilometer;
                }
            }
        }

        private double m_Distance;
        private Length.Units m_DistanceUnit;
    }
}
