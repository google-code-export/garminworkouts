using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class FITMessageField
    {
        private enum DataType
        {
            Enum = 0x00,
            SInt8 = 0x01,
            UInt8 = 0x02,
            SInt16 = 0x83,
            UInt16 = 0x84,
            SInt32 = 0x85,
            UInt32 = 0x86,
            String = 0x07,
            Float32 = 0x88,
            Float64 = 0x89,
            UInt8z = 0x0A,
            UInt16z = 0x8B,
            UInt32z = 0x8C,
            Byte = 0x0D
        }

        public FITMessageField(Byte definitionNumber)
        {
            m_DefinitionNumber = definitionNumber;
        }

        public void SetEnum(Byte value)
        {
            m_Type = DataType.Enum;
            m_ByteValue = value;
        }

        public void SetSInt8(SByte value)
        {
            m_Type = DataType.SInt8;
            m_SByteValue = value;
        }

        public void SetUInt8(Byte value)
        {
            m_Type = DataType.UInt8;
            m_ByteValue = value;
        }

        public void SetSInt16(Int16 value)
        {
            m_Type = DataType.SInt16;
            m_Int16Value = value;
        }

        public void SetUInt16(UInt16 value)
        {
            m_Type = DataType.UInt16;
            m_UInt16Value = value;
        }

        public void SetSInt32(Int32 value)
        {
            m_Type = DataType.SInt32;
            m_Int32Value = value;
        }

        public void SetUInt32(UInt32 value)
        {
            m_Type = DataType.UInt32;
            m_UInt32Value = value;
        }

        public void SetString(String value)
        {
            m_Type = DataType.String;
            m_StringValue = value;
        }

        public void SetFloat32(Single value)
        {
            m_Type = DataType.Float32;
            m_SingleValue = value;
        }

        public void SetFloat64(Double value)
        {
            m_Type = DataType.Float64;
            m_DoubleValue = value;
        }

        public void SetUInt8z(Byte value)
        {
            m_Type = DataType.UInt8z;
            m_ByteValue = value;
        }

        public void SetUInt16z(UInt16 value)
        {
            m_Type = DataType.UInt16z;
            m_UInt16Value = value;
        }

        public void SetUInt32z(UInt32 value)
        {
            m_Type = DataType.UInt32z;
            m_UInt32Value = value;
        }
        
        public void SetByte(Byte[] value)
        {
            m_Type = DataType.Byte;
            m_ByteArrayValue = value;
        }

        public void SerializeDefinition(Stream stream)
        {
            GarminFitnessByteRange definitionNumber = new GarminFitnessByteRange(m_DefinitionNumber);
            GarminFitnessByteRange size = new GarminFitnessByteRange(FieldSize);
            GarminFitnessByteRange baseType = new GarminFitnessByteRange((Byte)m_Type);

            definitionNumber.Serialize(stream);
            size.Serialize(stream);
            baseType.Serialize(stream);
        }

        public void SerializeData(Stream stream)
        {
            switch (m_Type)
            {
                case DataType.Enum:
                case DataType.UInt8:
                case DataType.UInt8z:
                    {
                        GarminFitnessByteRange data = new GarminFitnessByteRange(m_ByteValue);

                        data.Serialize(stream);
                        break;
                    }
                case DataType.SInt8:
                    {
                        GarminFitnessSByteRange data = new GarminFitnessSByteRange(m_SByteValue);

                        data.Serialize(stream);
                        break;
                    }
                case DataType.UInt16:
                case DataType.UInt16z:
                    {
                        GarminFitnessUInt16Range data = new GarminFitnessUInt16Range(m_UInt16Value);

                        data.Serialize(stream);
                        break;
                    }
                case DataType.SInt16:
                    {
                        GarminFitnessInt16Range data = new GarminFitnessInt16Range(m_Int16Value);

                        data.Serialize(stream);
                        break;
                    }
                case DataType.UInt32:
                case DataType.UInt32z:
                    {
                        GarminFitnessUInt32Range data = new GarminFitnessUInt32Range(m_UInt32Value);

                        data.Serialize(stream);
                        break;
                    }
                case DataType.SInt32:
                    {
                        GarminFitnessInt32Range data = new GarminFitnessInt32Range(m_Int32Value);

                        data.Serialize(stream);
                        break;
                    }
                case DataType.Float32:
                    {
                        GarminFitnessFloatRange data = new GarminFitnessFloatRange(m_SingleValue);

                        data.Serialize(stream);
                        break;
                    }
                case DataType.Float64:
                    {
                        GarminFitnessDoubleRange data = new GarminFitnessDoubleRange(m_DoubleValue);

                        data.Serialize(stream);
                        break;
                    }
                case DataType.String:
                    {
                        stream.Write(Encoding.UTF8.GetBytes(m_StringValue), 0, m_StringValue.Length);
                        break;
                    }
                case DataType.Byte:
                    {
                        stream.Write(m_ByteArrayValue, 0, m_ByteArrayValue.Length);
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        break;
                    }
            }
        }

        private Byte FieldSize
        {
            get
            {
                switch(m_Type)
                {
                    case DataType.Enum:
                    case DataType.UInt8:
                    case DataType.SInt8:
                    case DataType.UInt8z:
                        {
                            return 1;
                        }
                    case DataType.UInt16:
                    case DataType.SInt16:
                    case DataType.UInt16z:
                        {
                            return 2;
                        }
                    case DataType.UInt32:
                    case DataType.SInt32:
                    case DataType.Float32:
                    case DataType.UInt32z:
                        {
                            return 4;
                        }
                    case DataType.Float64:
                        {
                            return 8;
                        }
                    case DataType.String:
                        {
                            return (Byte)m_StringValue.Length;
                        }
                    case DataType.Byte:
                        {
                            return (Byte)m_ByteArrayValue.Length;
                        }
                    default:
                        {
                            Debug.Assert(false);
                            break;
                        }
                }

                return 0;
            }
        }

        public Byte DefinitionNumber
        {
            get { return m_DefinitionNumber; }
        }

        private Byte m_DefinitionNumber = 0;

        // Field data, use the right member depending on value of enum m_Type
        private DataType m_Type = DataType.Enum;
        private Byte m_ByteValue = 0;
        private SByte m_SByteValue = 0;
        private UInt16 m_UInt16Value = 0;
        private Int16 m_Int16Value = 0;
        private UInt32 m_UInt32Value = 0;
        private Int32 m_Int32Value = 0;
        private String m_StringValue = String.Empty;
        private Single m_SingleValue = 0;
        private Double m_DoubleValue = 0;
        private Byte[] m_ByteArrayValue = null;
    }
}
