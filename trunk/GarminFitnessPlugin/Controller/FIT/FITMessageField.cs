using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
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

        public FITMessageField(Byte definitionNumber, Byte type, byte size) :
            this(definitionNumber)
        {
            try
            {
                m_Type = (DataType)type;

                if (m_Type == DataType.String ||
                    m_Type == DataType.Byte)
                {
                    m_ByteArrayValue = new Byte[size];

                    if (m_Type == DataType.String)
                    {
                        m_StringLength = size;
                    }
                }

                Debug.Assert(size == FieldSize,
                             String.Format("Size inconsistent for type {0}.  Size = {1}; Expected = {2}", m_Type.ToString(), FieldSize, size));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SetEnum(Byte value)
        {
            m_Type = DataType.Enum;
            m_ByteValue = value;
        }

        public Byte GetEnum()
        {
            Debug.Assert(m_Type == DataType.Enum);
            return m_ByteValue;
        }

        public void SetSInt8(SByte value)
        {
            m_Type = DataType.SInt8;
            m_SByteValue = value;
        }

        public SByte GetSInt8()
        {
            Debug.Assert(m_Type == DataType.SInt8);
            return m_SByteValue;
        }

        public void SetUInt8(Byte value)
        {
            m_Type = DataType.UInt8;
            m_ByteValue = value;
        }

        public Byte GetUInt8()
        {
            Debug.Assert(m_Type == DataType.UInt8);
            return m_ByteValue;
        }
        public void SetSInt16(Int16 value)
        {
            m_Type = DataType.SInt16;
            m_Int16Value = value;
        }

        public Int16 GetSInt16()
        {
            Debug.Assert(m_Type == DataType.SInt16);
            return m_Int16Value;
        }
        public void SetUInt16(UInt16 value)
        {
            m_Type = DataType.UInt16;
            m_UInt16Value = value;
        }

        public UInt16 GetUInt16()
        {
            Debug.Assert(m_Type == DataType.UInt16);
            return m_UInt16Value;
        }
        public void SetSInt32(Int32 value)
        {
            m_Type = DataType.SInt32;
            m_Int32Value = value;
        }

        public Int32 GetSInt32()
        {
            Debug.Assert(m_Type == DataType.SInt32);
            return m_Int32Value;
        }
        public void SetUInt32(UInt32 value)
        {
            m_Type = DataType.UInt32;
            m_UInt32Value = value;
        }

        public UInt32 GetUInt32()
        {
            Debug.Assert(m_Type == DataType.UInt32);
            return m_UInt32Value;
        }

        public void SetString(String value, Byte stringLength)
        {
            m_Type = DataType.String;
            m_StringLength = stringLength;
            m_ByteArrayValue = Encoding.UTF8.GetBytes(value);
        }

        public String GetString()
        {
            Debug.Assert(m_Type == DataType.String);
            return Encoding.UTF8.GetString(m_ByteArrayValue).TrimEnd(new char[] {'\0'});
        }

        public void SetFloat32(Single value)
        {
            m_Type = DataType.Float32;
            m_SingleValue = value;
        }

        public Single GetFloat32()
        {
            Debug.Assert(m_Type == DataType.Float32);
            return m_SingleValue;
        }

        public void SetFloat64(Double value)
        {
            m_Type = DataType.Float64;
            m_DoubleValue = value;
        }

        public Double GetFloat64()
        {
            Debug.Assert(m_Type == DataType.Float64);
            return m_DoubleValue;
        }

        public void SetUInt8z(Byte value)
        {
            m_Type = DataType.UInt8z;
            m_ByteValue = value;
        }

        public Byte GetUInt8z()
        {
            Debug.Assert(m_Type == DataType.UInt8z);
            return m_ByteValue;
        }

        public void SetUInt16z(UInt16 value)
        {
            m_Type = DataType.UInt16z;
            m_UInt16Value = value;
        }

        public UInt16 GetUInt16z()
        {
            Debug.Assert(m_Type == DataType.UInt16z);
            return m_UInt16Value;
        }

        public void SetUInt32z(UInt32 value)
        {
            m_Type = DataType.UInt32z;
            m_UInt32Value = value;
        }

        public UInt32 GetUInt32z()
        {
            Debug.Assert(m_Type == DataType.UInt32z);
            return m_UInt32Value;
        }

        public void SetByte(Byte[] value)
        {
            m_Type = DataType.Byte;
            m_ByteArrayValue = value;
        }

        public Byte[] GetByte()
        {
            Debug.Assert(m_Type == DataType.Byte);
            return m_ByteArrayValue;
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
                        Byte[] valueStored = new Byte[m_StringLength];

                        for(int i = 0; i < valueStored.Length; ++i)
                        {
                            if (i < m_ByteArrayValue.Length)
                            {
                                valueStored[i] = m_ByteArrayValue[i];
                            }
                            else
                            {
                                valueStored[i] = 0;
                            }
                        }
                        stream.Write(valueStored, 0, valueStored.Length);
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
        
        public void DeserializeData(Stream stream, bool isLittleEndian)
        {
            switch (m_Type)
            {
                case DataType.Enum:
                case DataType.UInt8:
                case DataType.UInt8z:
                    {
                        GarminFitnessByteRange data = new GarminFitnessByteRange(m_ByteValue);

                        data.Deserialize(stream, Constants.CurrentVersion);
                        m_ByteValue = data;
                        break;
                    }
                case DataType.SInt8:
                    {
                        GarminFitnessSByteRange data = new GarminFitnessSByteRange(0);

                        data.Deserialize(stream, Constants.CurrentVersion);
                        m_SByteValue = data;
                        break;
                    }
                case DataType.UInt16:
                case DataType.UInt16z:
                    {
                        Byte[] data = new Byte[sizeof(UInt16)];

                        stream.Read(data, 0, sizeof(UInt16));
                        m_UInt16Value = BitConverter.ToUInt16(data, 0);

                        if (!isLittleEndian)
                        {
                            Byte temp = data[0];
                            data[0] = data[1];
                            data[1] = temp;

                            m_UInt16Value = BitConverter.ToUInt16(data, 0);
                        }
                        break;
                    }
                case DataType.SInt16:
                    {
                        GarminFitnessInt16Range data = new GarminFitnessInt16Range(0);

                        data.Deserialize(stream, Constants.CurrentVersion);
                        m_Int16Value = data;

                        if (!isLittleEndian)
                        {
                            m_Int16Value = IPAddress.NetworkToHostOrder(m_Int16Value);
                        }
                        break;
                    }
                case DataType.UInt32:
                case DataType.UInt32z:
                    {
                        Byte[] data = new Byte[sizeof(UInt32)];

                        stream.Read(data, 0, sizeof(UInt32));
                        m_UInt32Value = BitConverter.ToUInt32(data, 0);

                        if (!isLittleEndian)
                        {
                            Byte temp = data[0];
                            data[0] = data[3];
                            data[3] = temp;
                            temp = data[1];
                            data[1] = data[2];
                            data[2] = temp;

                            m_UInt32Value = BitConverter.ToUInt32(data, 0);
                        }
                        break;
                    }
                case DataType.SInt32:
                    {
                        GarminFitnessInt32Range data = new GarminFitnessInt32Range(0);

                        data.Deserialize(stream, Constants.CurrentVersion);
                        m_Int32Value = data;

                        if (!isLittleEndian)
                        {
                            m_Int32Value = IPAddress.NetworkToHostOrder(m_Int32Value);
                        }
                        break;
                    }
                case DataType.Float32:
                    {
                        Byte[] data = new Byte[sizeof(Single)];

                        stream.Read(data, 0, sizeof(Single));
                        m_SingleValue = BitConverter.ToSingle(data, 0);

                        if (!isLittleEndian)
                        {
                            Byte temp = data[0];
                            data[0] = data[3];
                            data[3] = temp;
                            temp = data[1];
                            data[1] = data[2];
                            data[2] = temp;

                            m_SingleValue = BitConverter.ToSingle(data, 0);
                        }
                        break;
                    }
                case DataType.Float64:
                    {
                        Byte[] data = new Byte[sizeof(Double)];

                        stream.Read(data, 0, sizeof(Double));
                        m_DoubleValue = BitConverter.ToDouble(data, 0);

                        if (!isLittleEndian)
                        {
                            Byte temp = data[0];
                            data[0] = data[7];
                            data[7] = temp;
                            temp = data[1];
                            data[1] = data[6];
                            data[6] = temp;
                            temp = data[2];
                            data[2] = data[5];
                            data[5] = temp;
                            temp = data[3];
                            data[3] = data[4];
                            data[4] = temp;

                            m_DoubleValue = BitConverter.ToDouble(data, 0);
                        }
                        break;
                    }
                case DataType.String:
                case DataType.Byte:
                    {
                        stream.Read(m_ByteArrayValue, 0, m_ByteArrayValue.Length);
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
                            return m_StringLength;
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

        public bool IsValueValid
        {
            get
            {
                switch (m_Type)
                {
                    case DataType.Enum:
                    case DataType.UInt8:
                        {
                            return m_ByteValue != 0xFF;
                        }
                    case DataType.SInt8:
                        {
                            return m_SByteValue != 0x7F;
                        }
                    case DataType.UInt8z:
                        {
                            return m_ByteValue != 0x00;
                        }
                    case DataType.UInt16:
                        {
                            return m_UInt16Value != 0xFFFF;
                        }
                    case DataType.SInt16:
                        {
                            return m_Int16Value != 0x7FFF;
                        }
                    case DataType.UInt16z:
                        {
                            return m_UInt16Value != 0x0000;
                        }
                    case DataType.UInt32:
                        {
                            return m_UInt32Value != 0xFFFFFFFF;
                        }
                    case DataType.SInt32:
                        {
                            return m_Int32Value != 0x7FFFFFFF;
                        }
                    case DataType.UInt32z:
                        {
                            return m_UInt32Value != 0x00000000;
                        }
                    case DataType.Float32:
                        {
                            return m_SingleValue != 0xFFFFFFFF;
                        }
                    case DataType.Float64:
                        {
                            return m_DoubleValue != 0xFFFFFFFFFFFFFFFF;
                        }
                    case DataType.String:
                        {
                            return GetString() != "";
                        }
                    case DataType.Byte:
                        {
                            foreach (Byte currentByte in m_ByteArrayValue)
                            {
                                if (currentByte != 0xFF)
                                {
                                    return true;
                                }
                            }

                            return true;
                        }
                    default:
                        {
                            Debug.Assert(false);
                            break;
                        }
                }

                return false;
            }
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
        private Single m_SingleValue = 0;
        private Double m_DoubleValue = 0;
        private Byte[] m_ByteArrayValue = null;
        private Byte m_StringLength = 0;
    }
}
