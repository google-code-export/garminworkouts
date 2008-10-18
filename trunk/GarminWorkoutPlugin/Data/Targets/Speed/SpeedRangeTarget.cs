using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Measurement;

namespace GarminWorkoutPlugin.Data
{
    class SpeedRangeTarget : IConcreteSpeedTarget
    {
        public SpeedRangeTarget(BaseSpeedTarget baseTarget)
            : base(SpeedTargetType.Range, baseTarget)
        {
            Length.Units baseUnits = baseTarget.ParentStep.ParentWorkout.Category.DistanceUnits;

            if (baseUnits == Length.Units.Meter)
            {
                SetRangeInUnitsPerHour(15000, 25000, baseUnits);
            }
            else
            {
                SetRangeInUnitsPerHour(15, 25, baseUnits);
            }
        }

        public SpeedRangeTarget(double minUnitsPerHour, double maxUnitsPerHour, Length.Units speedUnit, Speed.Units speedPace, BaseSpeedTarget baseTarget)
            : this(baseTarget)
        {
            if (speedPace == Speed.Units.Pace)
            {
                SetRangeInMinutesPerUnit(minUnitsPerHour, maxUnitsPerHour, speedUnit);
            }
            else
            {
                SetRangeInUnitsPerHour(minUnitsPerHour, maxUnitsPerHour, speedUnit);
            }
        }

        public SpeedRangeTarget(Stream stream, DataVersion version, BaseSpeedTarget baseTarget)
            : this(baseTarget)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            stream.Write(BitConverter.GetBytes(m_MinUnitsPerSecond), 0, sizeof(double));
            stream.Write(BitConverter.GetBytes(m_MaxUnitsPerSecond), 0, sizeof(double));
            stream.Write(BitConverter.GetBytes((Int32)m_SpeedUnit), 0, sizeof(Int32));
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IConcreteSpeedTarget), stream, version);

            byte[] doubleBuffer = new byte[sizeof(double)];
            byte[] intBuffer = new byte[sizeof(Int32)];
            double minSpeed;
            double maxSpeed;

            stream.Read(doubleBuffer, 0, sizeof(double));
            minSpeed = BitConverter.ToDouble(doubleBuffer, 0) * Constants.SecondsPerHour;
            stream.Read(doubleBuffer, 0, sizeof(double));
            maxSpeed = BitConverter.ToDouble(doubleBuffer, 0) * Constants.SecondsPerHour;
            stream.Read(intBuffer, 0, sizeof(Int32));

            SetRangeInUnitsPerHour(minSpeed, maxSpeed, (Length.Units)BitConverter.ToInt32(intBuffer, 0));
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IConcreteSpeedTarget), stream, version);

            byte[] doubleBuffer = new byte[sizeof(double)];
            byte[] intBuffer = new byte[sizeof(Int32)];
            double minSpeed;
            double maxSpeed;
            Length.Units speedUnit;
            Speed.Units speedOrPace;

            stream.Read(doubleBuffer, 0, sizeof(double));
            minSpeed = BitConverter.ToDouble(doubleBuffer, 0) * Constants.SecondsPerHour;
            stream.Read(doubleBuffer, 0, sizeof(double));
            maxSpeed = BitConverter.ToDouble(doubleBuffer, 0) * Constants.SecondsPerHour;
            stream.Read(intBuffer, 0, sizeof(Int32));
            speedUnit = (Length.Units)BitConverter.ToInt32(intBuffer, 0);

            // This is deprecated in version 0.2.58, should be removed from next data version
            stream.Read(intBuffer, 0, sizeof(Int32));
            speedOrPace = (Speed.Units)BitConverter.ToInt32(intBuffer, 0);

            SetRangeInUnitsPerHour(minSpeed, maxSpeed, speedUnit);
        }

        public void Deserialize_V6(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IConcreteSpeedTarget), stream, version);

            byte[] doubleBuffer = new byte[sizeof(double)];
            byte[] intBuffer = new byte[sizeof(Int32)];
            double minSpeed;
            double maxSpeed;
            Length.Units speedUnit;

            stream.Read(doubleBuffer, 0, sizeof(double));
            minSpeed = BitConverter.ToDouble(doubleBuffer, 0) * Constants.SecondsPerHour;
            stream.Read(doubleBuffer, 0, sizeof(double));
            maxSpeed = BitConverter.ToDouble(doubleBuffer, 0) * Constants.SecondsPerHour;
            stream.Read(intBuffer, 0, sizeof(Int32));
            speedUnit = (Length.Units)BitConverter.ToInt32(intBuffer, 0);

            SetRangeInUnitsPerHour(minSpeed, maxSpeed, speedUnit);
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            base.Serialize(parentNode, document);

            CultureInfo culture = new CultureInfo("en-us");
            XmlNode valueNode;
            XmlAttribute attribute;

            // Type
            attribute = document.CreateAttribute("xsi", "type", Constants.xsins);
            attribute.Value = "CustomSpeedZone_t";
            parentNode.Attributes.Append(attribute);

            // View as
            valueNode = document.CreateElement("ViewAs");
            valueNode.AppendChild(document.CreateTextNode(Constants.SpeedOrPaceTCXString[ViewAsPace ? 0 : 1]));
            parentNode.AppendChild(valueNode);

            // Low
            valueNode = document.CreateElement("LowInMetersPerSecond");
            valueNode.AppendChild(document.CreateTextNode(String.Format(culture.NumberFormat, "{0:0.00000}", MinMetersPerSecond)));
            parentNode.AppendChild(valueNode);

            // High
            valueNode = document.CreateElement("HighInMetersPerSecond");
            valueNode.AppendChild(document.CreateTextNode(String.Format(culture.NumberFormat, "{0:0.00000}", MaxMetersPerSecond)));
            parentNode.AppendChild(valueNode);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if (base.Deserialize(parentNode))
            {
                double minSpeed = 0;
                double maxSpeed = 0;

                for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
                {
                    XmlNode valueNode = parentNode.ChildNodes[i];
                    CultureInfo culture = new CultureInfo("en-us");

/*                    if(valueNode.Name == "ViewAs" &&
                        valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText))
                    {
                        if (valueNode.FirstChild.Value == "Pace")
                        {
                            speedOrPace = Speed.Units.Pace;
                        }
                    }
                    else */if (valueNode.Name == "LowInMetersPerSecond" &&
                        valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText))
                    {
                        minSpeed = double.Parse(valueNode.FirstChild.Value, culture.NumberFormat);
                    }
                    else if (valueNode.Name == "HighInMetersPerSecond" &&
                        valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText))
                    {
                        maxSpeed = double.Parse(valueNode.FirstChild.Value, culture.NumberFormat);
                    }
                }

                if (minSpeed > 0 && maxSpeed > 0)
                {
                    double minInMiles = Length.Convert(minSpeed * Constants.SecondsPerHour, Length.Units.Meter, Length.Units.Mile);
                    double maxInMiles = Length.Convert(maxSpeed * Constants.SecondsPerHour, Length.Units.Meter, Length.Units.Mile);

                    if (minInMiles >= 1 && minInMiles <= 60 && maxInMiles >= 1 && maxInMiles <= 60)
                    {
                        if (minSpeed < maxSpeed)
                        {
                            SetRangeInUnitsPerHour(minSpeed * Constants.SecondsPerHour,
                                                   maxSpeed * Constants.SecondsPerHour,
                                                   Length.Units.Meter);
                        }
                        else
                        {
                            SetRangeInUnitsPerHour(maxSpeed * Constants.SecondsPerHour,
                                                   minSpeed * Constants.SecondsPerHour,
                                                   Length.Units.Meter);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public double MinMetersPerSecond
        {
            get { return Length.Convert(m_MinUnitsPerSecond, m_SpeedUnit, Length.Units.Meter); }
        }

        public double MaxMetersPerSecond
        {
            get { return Length.Convert(m_MaxUnitsPerSecond, m_SpeedUnit, Length.Units.Meter); }
        }

        public double GetMinSpeedInUnitsPerHour(Length.Units speedUnit)
        {
            return Length.Convert(m_MinUnitsPerSecond, m_SpeedUnit, speedUnit) * Constants.SecondsPerHour;
        }

        public double GetMaxSpeedInUnitsPerHour(Length.Units speedUnit)
        {
            return Length.Convert(m_MaxUnitsPerSecond, m_SpeedUnit, speedUnit) * Constants.SecondsPerHour;
        }

        public double GetMinSpeedInMinutesPerUnit(Length.Units speedUnit)
        {
            double unitsPerMinute = Length.Convert(m_MinUnitsPerSecond, m_SpeedUnit, speedUnit) * Constants.SecondsPerMinute;
            return 1.0 / unitsPerMinute;
        }

        public double GetMaxSpeedInMinutesPerUnit(Length.Units speedUnit)
        {
            double unitsPerMinute = Length.Convert(m_MaxUnitsPerSecond, m_SpeedUnit, speedUnit) * Constants.SecondsPerMinute;
            return 1.0 / unitsPerMinute;
        }

        public void SetMinSpeedInUnitsPerHour(double minUnitsPerHour, Length.Units speedUnit)
        {
            double minInMiles = Length.Convert(minUnitsPerHour, speedUnit, Length.Units.Mile);

            Trace.Assert(minInMiles >= 1 && minInMiles <= 60);

            m_MinUnitsPerSecond = minUnitsPerHour / Constants.SecondsPerHour;

            if (speedUnit != m_SpeedUnit)
            {
                // Convert the old max
                m_MaxUnitsPerSecond = Length.Convert(m_MaxUnitsPerSecond, m_SpeedUnit, speedUnit);
            }

            m_SpeedUnit = speedUnit;
        }

        public void SetMaxSpeedInUnitsPerHour(double maxUnitsPerHour, Length.Units speedUnit)
        {
            double maxInMiles = Length.Convert(maxUnitsPerHour, speedUnit, Length.Units.Mile);

            Trace.Assert(maxInMiles >= 1 && maxInMiles <= 60);

            m_MaxUnitsPerSecond = maxUnitsPerHour / Constants.SecondsPerHour;

            if (speedUnit != m_SpeedUnit)
            {
                // Convert the old max
                m_MinUnitsPerSecond = Length.Convert(m_MinUnitsPerSecond, m_SpeedUnit, speedUnit);
            }

            m_SpeedUnit = speedUnit;
        }

        public void SetMinSpeedInMinutesPerUnit(double minMinutesPerUnit, Length.Units speedUnit)
        {
            // Convert to speed (units/hr)
            SetMinSpeedInUnitsPerHour(60.0 / minMinutesPerUnit, speedUnit);
        }

        public void SetMaxSpeedInMinutesPerUnit(double maxMinutesPerUnit, Length.Units speedUnit)
        {
            // Convert to speed (units/hr)
            SetMaxSpeedInUnitsPerHour(60.0 / maxMinutesPerUnit, speedUnit);
        }

        public void SetRangeInUnitsPerHour(double minUnitsPerHour, double maxUnitsPerHour, Length.Units speedUnit)
        {
            double minInMiles = Length.Convert(minUnitsPerHour, speedUnit, Length.Units.Mile);
            double maxInMiles = Length.Convert(maxUnitsPerHour, speedUnit, Length.Units.Mile);

            Trace.Assert(minInMiles >= 1 && minInMiles <= 60);
            Trace.Assert(maxInMiles >= 1 && maxInMiles <= 60);

            m_MinUnitsPerSecond = minUnitsPerHour / Constants.SecondsPerHour;
            m_MaxUnitsPerSecond = maxUnitsPerHour / Constants.SecondsPerHour;
            m_SpeedUnit = speedUnit;
        }

        public void SetRangeInMinutesPerUnit(double minMinutesPerUnit, double maxMinutesPerUnit, Length.Units speedUnit)
        {
            // Convert to speed (units/hr)
            SetRangeInMinutesPerUnit(60.0 / minMinutesPerUnit, 60.0 / maxMinutesPerUnit, speedUnit);
        }

        public override bool IsDirty
        {
            get { return false; }
            set { Trace.Assert(false); }
        }

        private double m_MinUnitsPerSecond;
        private double m_MaxUnitsPerSecond;
        private Length.Units m_SpeedUnit;
    }
}
