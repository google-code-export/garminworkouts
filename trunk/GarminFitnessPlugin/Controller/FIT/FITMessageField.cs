using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    public class FITMessageField
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

        public FITMessageField(Byte definitionNumber) :
            this(definitionNumber, (Byte)DataType.Enum, 1)
        {
        }

        public FITMessageField(Byte definitionNumber, Byte type, byte size)
        {
            try
            {
                m_DefinitionNumber = definitionNumber;
                m_Type = (DataType)type;

                if (m_Type == DataType.String ||
                    m_Type == DataType.Byte ||
                    m_Type == DataType.UInt8 ||
                    m_Type == DataType.UInt8z ||
                    m_Type == DataType.Enum)
                {
                    m_ByteValues = new Byte[size];

                    if (m_Type == DataType.String)
                    {
                        m_StringLength = size;
                    }
                }
                else if (m_Type == DataType.SInt8)
                {
                    // Array
                    m_SByteValues = new SByte[size];
                }
                else if (m_Type == DataType.UInt16 ||
                         m_Type == DataType.UInt16z)
                {
                    // Array
                    m_UInt16Values = new UInt16[size / 2];
                }
                else if (m_Type == DataType.SInt16)
                {
                    // Array
                    m_Int16Values = new Int16[size / 2];
                }
                else if (m_Type == DataType.UInt32 ||
                         m_Type == DataType.UInt32z)
                {
                    // Array
                    m_UInt32Values = new UInt32[size / 4];
                }
                else if (m_Type == DataType.SInt32)
                {
                    // Array
                    m_Int32Values = new Int32[size / 4];
                }
                else if (m_Type == DataType.Float32)
                {
                    // Array
                    m_SingleValues = new Single[size / 4];
                }
                else if (m_Type == DataType.Float64)
                {
                    // Array
                    m_DoubleValues = new Double[size / 8];
                }

                Debug.Assert(size == FieldSize * ArraySize,
                             String.Format("Size inconsistent for type {0}.  Size = {1}; Expected = {2}", m_Type.ToString(), FieldSize * ArraySize, size));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SetEnum(Byte value)
        {
            SetEnum(value, 0);
        }

        public void SetEnum(Byte value, int index)
        {
            if (m_ByteValues == null ||
                index >= m_ByteValues.Length)
            {
                Byte[] temp = m_ByteValues;

                m_ByteValues = new Byte[index + 1];

                if (temp != null)
                {
                    int copyIndex = 0;
                    foreach (Byte copyValue in temp)
                    {
                        m_ByteValues[copyIndex] = copyValue;
                    }
                }
            }

            m_Type = DataType.Enum;
            m_ByteValues[index] = value;
        }

        public Byte GetEnum()
        {
            return GetEnum(0);
        }

        public Byte GetEnum(int index)
        {
            Debug.Assert(m_Type == DataType.Enum);
            Debug.Assert(index >= 0 && index < ArraySize);

            return m_ByteValues[index];
        }

        public void SetSInt8(SByte value)
        {
            SetSInt8(value, 0);
        }

        public void SetSInt8(SByte value, int index)
        {
            if (m_SByteValues == null ||
                index >= m_SByteValues.Length)
            {
                SByte[] temp = m_SByteValues;

                m_SByteValues = new SByte[index + 1];

                if (temp != null)
                {
                    int copyIndex = 0;
                    foreach (SByte copyValue in temp)
                    {
                        m_SByteValues[copyIndex] = copyValue;
                    }
                }
            }

            m_Type = DataType.Enum;
            m_SByteValues[index] = value;
        }

        public SByte GetSInt8()
        {
            return GetSInt8(0);
        }

        public SByte GetSInt8(int index)
        {
            Debug.Assert(m_Type == DataType.SInt8);
            Debug.Assert(index >= 0 && index < ArraySize);

            return m_SByteValues[index];
        }

        public void SetUInt8(Byte value)
        {
            SetUInt8(value, 0);
        }

        public void SetUInt8(Byte value, int index)
        {
            if (m_ByteValues == null ||
                index >= m_ByteValues.Length)
            {
                Byte[] temp = m_ByteValues;

                m_ByteValues = new Byte[index + 1];

                if (temp != null)
                {
                    int copyIndex = 0;
                    foreach (Byte copyValue in temp)
                    {
                        m_ByteValues[copyIndex] = copyValue;
                    }
                }
            }

            m_Type = DataType.UInt8;
            m_ByteValues[index] = value;
        }

        public Byte GetUInt8()
        {
            return GetUInt8(0);
        }

        public Byte GetUInt8(int index)
        {
            Debug.Assert(m_Type == DataType.UInt8);
            Debug.Assert(index >= 0 && index < ArraySize);

            return m_ByteValues[index];
        }

        public void SetSInt16(Int16 value)
        {
            SetSInt16(value, 0);
        }

        public void SetSInt16(Int16 value, int index)
        {
            if (m_Int16Values == null ||
                index >= m_Int16Values.Length)
            {
                Int16[] temp = m_Int16Values;

                m_Int16Values = new Int16[index + 1];

                if (temp != null)
                {
                    int copyIndex = 0;
                    foreach (Int16 copyValue in temp)
                    {
                        m_Int16Values[copyIndex] = copyValue;
                    }
                }
            }

            m_Type = DataType.SInt16;
            m_Int16Values[index] = value;
        }

        public Int16 GetSInt16()
        {
            return GetSInt16(0);
        }

        public Int16 GetSInt16(int index)
        {
            Debug.Assert(m_Type == DataType.SInt16);
            Debug.Assert(index >= 0 && index < ArraySize);

            return m_Int16Values[index];
        }

        public void SetUInt16(UInt16 value)
        {
            SetUInt16(value, 0);
        }
        
        public void SetUInt16(UInt16 value, int index)
        {
            if (m_UInt16Values == null ||
                index >= m_UInt16Values.Length)
            {
                UInt16[] temp = m_UInt16Values;

                m_UInt16Values = new UInt16[index + 1];

                if (temp != null)
                {
                    int copyIndex = 0;
                    foreach (UInt16 copyValue in temp)
                    {
                        m_UInt16Values[copyIndex] = copyValue;
                    }
                }
            }

            m_Type = DataType.UInt16;
            m_UInt16Values[index] = value;
        }

        public UInt16 GetUInt16()
        {
            return GetUInt16(0);
        }

        public UInt16 GetUInt16(int index)
        {
            Debug.Assert(m_Type == DataType.UInt16);
            Debug.Assert(index >= 0 && index < ArraySize);

            return m_UInt16Values[index];
        }

        public void SetSInt32(Int32 value)
        {
            SetSInt32(value, 0);
        }

        public void SetSInt32(Int32 value, int index)
        {
            if (m_Int32Values == null ||
                index >= m_Int32Values.Length)
            {
                Int32[] temp = m_Int32Values;

                m_Int32Values = new Int32[index + 1];

                if (temp != null)
                {
                    int copyIndex = 0;
                    foreach (Int32 copyValue in temp)
                    {
                        m_Int32Values[copyIndex] = copyValue;
                    }
                }
            }

            m_Type = DataType.SInt32;
            m_Int32Values[index] = value;
        }

        public Int32 GetSInt32()
        {
            return GetSInt32(0);
        }

        public Int32 GetSInt32(int index)
        {
            Debug.Assert(m_Type == DataType.SInt32);
            Debug.Assert(index >= 0 && index < ArraySize);

            return m_Int32Values[index];
        }

        public void SetUInt32(UInt32 value)
        {
            SetUInt32(value, 0);
        }

        public void SetUInt32(UInt32 value, int index)
        {
            if (m_UInt32Values == null ||
                index >= m_UInt32Values.Length)
            {
                UInt32[] temp = m_UInt32Values;

                m_UInt32Values = new UInt32[index + 1];

                if (temp != null)
                {
                    int copyIndex = 0;
                    foreach (UInt32 copyValue in temp)
                    {
                        m_UInt32Values[copyIndex] = copyValue;
                    }
                }
            }

            m_Type = DataType.UInt32;
            m_UInt32Values[index] = value;
        }

        public UInt32 GetUInt32()
        {
            return GetUInt32(0);
        }

        public UInt32 GetUInt32(int index)
        {
            Debug.Assert(m_Type == DataType.UInt32);
            Debug.Assert(index >= 0 && index < ArraySize);

            return m_UInt32Values[index];
        }

        public void SetString(String value, Byte stringLength)
        {
            m_Type = DataType.String;
            m_ByteValues = Encoding.UTF8.GetBytes(value);
            m_StringLength = stringLength;
        }

        public void SetString(String value)
        {
            Debug.Assert(value.Length < Byte.MaxValue);

            SetString(value, (Byte)(value.Length + 1));
        }

        public String GetString()
        {
            Debug.Assert(m_Type == DataType.String);

            return Encoding.UTF8.GetString(m_ByteValues).TrimEnd(new char[] { '\0' });
        }

        public void SetFloat32(Single value)
        {
            SetFloat32(value, 0);
        }

        public void SetFloat32(Single value, int index)
        {
            if (m_SingleValues == null ||
                index >= m_SingleValues.Length)
            {
                Single[] temp = m_SingleValues;

                m_SingleValues = new Single[index + 1];

                if (temp != null)
                {
                    int copyIndex = 0;
                    foreach (Single copyValue in temp)
                    {
                        m_SingleValues[copyIndex] = copyValue;
                    }
                }
            }

            m_Type = DataType.Float32;
            m_SingleValues[index] = value;
        }

        public Single GetFloat32()
        {
            return GetFloat32(0);
        }

        public Single GetFloat32(int index)
        {
            Debug.Assert(m_Type == DataType.Float32);
            Debug.Assert(index >= 0 && index < ArraySize);

            return m_SingleValues[index];
        }

        public void SetFloat64(Double value)
        {
            SetFloat64(0);
        }

        public void SetFloat64(Single value, int index)
        {
            if (m_DoubleValues == null ||
                index >= m_DoubleValues.Length)
            {
                Double[] temp = m_DoubleValues;

                m_DoubleValues = new Double[index + 1];

                if (temp != null)
                {
                    int copyIndex = 0;
                    foreach (Double copyValue in temp)
                    {
                        m_DoubleValues[copyIndex] = copyValue;
                    }
                }
            }

            m_Type = DataType.Float64;
            m_DoubleValues[index] = value;
        }

        public Double GetFloat64()
        {
            return GetFloat64(0);
        }

        public Double GetFloat64(int index)
        {
            Debug.Assert(m_Type == DataType.Float64);
            Debug.Assert(index >= 0 && index < ArraySize);

            return m_DoubleValues[index];
        }

        public void SetUInt8z(Byte value)
        {
            SetUInt8z(value, 0);
        }

        public void SetUInt8z(Byte value, int index)
        {
            if (m_ByteValues == null ||
                index >= m_ByteValues.Length)
            {
                Byte[] temp = m_ByteValues;

                m_ByteValues = new Byte[index + 1];

                if (temp != null)
                {
                    int copyIndex = 0;
                    foreach (Byte copyValue in temp)
                    {
                        m_ByteValues[copyIndex] = copyValue;
                    }
                }
            }

            m_Type = DataType.UInt8z;
            m_ByteValues[index] = value;
        }

        public Byte GetUInt8z()
        {
            return GetUInt8z(0);
        }

        public Byte GetUInt8z(int index)
        {
            Debug.Assert(m_Type == DataType.UInt8z);
            Debug.Assert(index >= 0 && index < ArraySize);

            return m_ByteValues[index];
        }

        public void SetUInt16z(UInt16 value)
        {
            SetUInt16z(value, 0);
        }

        public void SetUInt16z(UInt16 value, int index)
        {
            if (m_Int16Values == null ||
                index >= m_Int16Values.Length)
            {
                UInt16[] temp = m_UInt16Values;

                m_UInt16Values = new UInt16[index + 1];

                if (temp != null)
                {
                    int copyIndex = 0;
                    foreach (UInt16 copyValue in temp)
                    {
                        m_UInt16Values[copyIndex] = copyValue;
                    }
                }
            }

            m_Type = DataType.UInt16z;
            m_UInt16Values[index] = value;
        }

        public UInt16 GetUInt16z()
        {
            return GetUInt16z(0);
        }

        public UInt16 GetUInt16z(int index)
        {
            Debug.Assert(m_Type == DataType.UInt16z);
            Debug.Assert(index >= 0 && index < ArraySize);

            return m_UInt16Values[index];
        }

        public void SetUInt32z(UInt32 value)
        {
            SetUInt32z(value, 0);
        }

        public void SetUInt32z(UInt32 value, int index)
        {
            if (m_Int32Values == null ||
                index >= m_Int32Values.Length)
            {
                UInt32[] temp = m_UInt32Values;

                m_UInt32Values = new UInt32[index + 1];

                if (temp != null)
                {
                    int copyIndex = 0;
                    foreach (UInt32 copyValue in temp)
                    {
                        m_UInt32Values[copyIndex] = copyValue;
                    }
                }
            }

            m_Type = DataType.UInt32z;
            m_UInt32Values[index] = value;
        }

        public UInt32 GetUInt32z()
        {
            return GetUInt32z(0);
        }

        public UInt32 GetUInt32z(int index)
        {
            Debug.Assert(m_Type == DataType.UInt32z);
            Debug.Assert(index >= 0 && index < ArraySize);

            return m_UInt32Values[index];
        }

        public void SetByte(Byte[] value)
        {
            m_Type = DataType.Byte;
            m_ByteValues = value;
        }

        public Byte[] GetByte()
        {
            Debug.Assert(m_Type == DataType.Byte);

            return m_ByteValues;
        }

        public void SerializeDefinition(Stream stream)
        {
            GarminFitnessByteRange definitionNumber = new GarminFitnessByteRange(m_DefinitionNumber);
            GarminFitnessByteRange size = new GarminFitnessByteRange((Byte)(FieldSize * ArraySize));
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
                        for (int i = 0; i < ArraySize; ++i)
                        {
                            GarminFitnessByteRange data = new GarminFitnessByteRange(m_ByteValues[i]);

                            data.Serialize(stream);
                        }
                        break;
                    }
                case DataType.SInt8:
                    {
                        for (int i = 0; i < ArraySize; ++i)
                        {
                            GarminFitnessSByteRange data = new GarminFitnessSByteRange(m_SByteValues[i]);

                            data.Serialize(stream);
                        }
                        break;
                    }
                case DataType.UInt16:
                case DataType.UInt16z:
                    {
                        for (int i = 0; i < ArraySize; ++i)
                        {
                            GarminFitnessUInt16Range data = new GarminFitnessUInt16Range(m_UInt16Values[i]);

                            data.Serialize(stream);
                        }
                        break;
                    }
                case DataType.SInt16:
                    {
                        for (int i = 0; i < ArraySize; ++i)
                        {
                            GarminFitnessInt16Range data = new GarminFitnessInt16Range(m_Int16Values[i]);

                            data.Serialize(stream);
                        }
                        break;
                    }
                case DataType.UInt32:
                case DataType.UInt32z:
                    {
                        for (int i = 0; i < ArraySize; ++i)
                        {
                            GarminFitnessUInt32Range data = new GarminFitnessUInt32Range(m_UInt32Values[i]);

                            data.Serialize(stream);
                        }
                        break;
                    }
                case DataType.SInt32:
                    {
                        for (int i = 0; i < ArraySize; ++i)
                        {
                            GarminFitnessInt32Range data = new GarminFitnessInt32Range(m_Int32Values[i]);

                            data.Serialize(stream);
                        }
                        break;
                    }
                case DataType.Float32:
                    {
                        for (int i = 0; i < ArraySize; ++i)
                        {
                            GarminFitnessFloatRange data = new GarminFitnessFloatRange(m_SingleValues[i]);

                            data.Serialize(stream);
                        }
                        break;
                    }
                case DataType.Float64:
                    {
                        for (int i = 0; i < ArraySize; ++i)
                        {
                            GarminFitnessDoubleRange data = new GarminFitnessDoubleRange(m_DoubleValues[i]);

                            data.Serialize(stream);
                        }
                        break;
                    }
                case DataType.String:
                    {
                        Byte[] valueStored = new Byte[FieldSize];

                        for(int i = 0; i < valueStored.Length; ++i)
                        {
                            if (i < m_ByteValues.Length)
                            {
                                valueStored[i] = m_ByteValues[i];
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
                        stream.Write(m_ByteValues, 0, m_ByteValues.Length);
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
                        for (int i = 0; i < ArraySize; ++i)
                        {
                            GarminFitnessByteRange data = new GarminFitnessByteRange(0);

                            data.Deserialize(stream, Constants.CurrentVersion);
                            m_ByteValues[i] = data;
                        }
                        break;
                    }
                case DataType.SInt8:
                    {
                        for (int i = 0; i < ArraySize; ++i)
                        {
                            GarminFitnessSByteRange data = new GarminFitnessSByteRange(0);

                            data.Deserialize(stream, Constants.CurrentVersion);
                            m_SByteValues[i] = data;
                        }
                        break;
                    }
                case DataType.UInt16:
                case DataType.UInt16z:
                    {
                        for (int i = 0; i < ArraySize; ++i)
                        {
                            Byte[] data = new Byte[sizeof(UInt16)];

                            stream.Read(data, 0, sizeof(UInt16));
                            m_UInt16Values[i] = BitConverter.ToUInt16(data, 0);

                            if (!isLittleEndian)
                            {
                                Byte temp = data[0];
                                data[0] = data[1];
                                data[1] = temp;

                                m_UInt16Values[i] = BitConverter.ToUInt16(data, 0);
                            }
                        }
                        break;
                    }
                case DataType.SInt16:
                    {
                        for (int i = 0; i < ArraySize; ++i)
                        {
                            Byte[] data = new Byte[sizeof(Int16)];

                            stream.Read(data, 0, sizeof(UInt16));
                            m_Int16Values[i] = BitConverter.ToInt16(data, 0);

                            if (!isLittleEndian)
                            {
                                Byte temp = data[0];
                                data[0] = data[1];
                                data[1] = temp;

                                m_Int16Values[i] = BitConverter.ToInt16(data, 0);
                            }
                        }
                        break;
                    }
                case DataType.UInt32:
                case DataType.UInt32z:
                    {
                        for (int i = 0; i < ArraySize; ++i)
                        {
                            Byte[] data = new Byte[sizeof(UInt32)];

                            stream.Read(data, 0, sizeof(UInt32));
                            m_UInt32Values[i] = BitConverter.ToUInt32(data, 0);

                            if (!isLittleEndian)
                            {
                                Byte temp = data[0];
                                data[0] = data[3];
                                data[3] = temp;
                                temp = data[1];
                                data[1] = data[2];
                                data[2] = temp;

                                m_UInt32Values[i] = BitConverter.ToUInt32(data, 0);
                            }
                        }
                        break;
                    }
                case DataType.SInt32:
                    {
                        for (int i = 0; i < ArraySize; ++i)
                        {
                            Byte[] data = new Byte[sizeof(UInt32)];

                            stream.Read(data, 0, sizeof(UInt32));
                            m_Int32Values[i] = BitConverter.ToInt32(data, 0);

                            if (!isLittleEndian)
                            {
                                Byte temp = data[0];
                                data[0] = data[3];
                                data[3] = temp;
                                temp = data[1];
                                data[1] = data[2];
                                data[2] = temp;

                                m_Int32Values[i] = BitConverter.ToInt32(data, 0);
                            }
                        }
                        break;
                    }
                case DataType.Float32:
                    {
                        for (int i = 0; i < ArraySize; ++i)
                        {
                            Byte[] data = new Byte[sizeof(Single)];

                            stream.Read(data, 0, sizeof(Single));
                            m_SingleValues[i] = BitConverter.ToSingle(data, 0);

                            if (!isLittleEndian)
                            {
                                Byte temp = data[0];
                                data[0] = data[3];
                                data[3] = temp;
                                temp = data[1];
                                data[1] = data[2];
                                data[2] = temp;

                                m_SingleValues[i] = BitConverter.ToSingle(data, 0);
                            }
                        }
                        break;
                    }
                case DataType.Float64:
                    {
                        for (int i = 0; i < ArraySize; ++i)
                        {
                            Byte[] data = new Byte[sizeof(Double)];

                            stream.Read(data, 0, sizeof(Double));
                            m_DoubleValues[i] = BitConverter.ToDouble(data, 0);

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

                                m_DoubleValues[i] = BitConverter.ToDouble(data, 0);
                            }
                        }
                        break;
                    }
                case DataType.String:
                case DataType.Byte:
                    {
                        stream.Read(m_ByteValues, 0, m_ByteValues.Length);
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
                    case DataType.Byte:
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
                    default:
                        {
                            Debug.Assert(false);
                            break;
                        }
                }

                return 0;
            }
        }

        private int ArraySize
        {
            get
            {
                switch (m_Type)
                {
                    case DataType.Enum:
                    case DataType.UInt8:
                    case DataType.UInt8z:
                    case DataType.Byte:
                        {
                            return m_ByteValues.Length;
                        }
                    case DataType.SInt8:
                        {
                            return m_SByteValues.Length;
                        }
                    case DataType.UInt16:
                    case DataType.UInt16z:
                        {
                            return m_UInt16Values.Length;
                        }
                    case DataType.SInt16:
                        {
                            return m_Int16Values.Length;
                        }
                    case DataType.UInt32:
                    case DataType.UInt32z:
                        {
                            return m_UInt32Values.Length;
                        }
                    case DataType.SInt32:
                        {
                            return m_Int32Values.Length;
                        }
                    case DataType.Float32:
                        {
                            return m_SingleValues.Length;
                        }
                    case DataType.Float64:
                        {
                            return m_DoubleValues.Length;
                        }
                    case DataType.String:
                        {
                            return 1;
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
                    case DataType.Byte:
                        {
                            foreach (Byte currentValue in m_ByteValues)
                            {
                                if (currentValue == 0xFF)
                                {
                                    return false;
                                }
                            }

                            return true;
                        }
                    case DataType.SInt8:
                        {
                            foreach (Byte currentValue in m_ByteValues)
                            {
                                if (currentValue == 0x7F)
                                {
                                    return false;
                                }
                            }

                            return true;
                        }
                    case DataType.UInt8z:
                        {
                            foreach (Byte currentValue in m_ByteValues)
                            {
                                if (currentValue == 0x00)
                                {
                                    return false;
                                }
                            }

                            return true;
                        }
                    case DataType.UInt16:
                        {
                            foreach (UInt16 currentValue in m_UInt16Values)
                            {
                                if (currentValue == 0xFFFF)
                                {
                                    return false;
                                }
                            }

                            return true;
                        }
                    case DataType.SInt16:
                        {
                            foreach (Int16 currentValue in m_Int16Values)
                            {
                                if (currentValue == 0x7FFF)
                                {
                                    return false;
                                }
                            }

                            return true;
                        }
                    case DataType.UInt16z:
                        {
                            foreach (UInt16 currentValue in m_UInt16Values)
                            {
                                if (currentValue == 0x7F)
                                {
                                    return false;
                                }
                            }

                            return true;
                        }
                    case DataType.UInt32:
                        {
                            foreach (UInt32 currentValue in m_UInt32Values)
                            {
                                if (currentValue == 0xFFFFFFFF)
                                {
                                    return false;
                                }
                            }

                            return true;
                        }
                    case DataType.SInt32:
                        {
                            foreach (Int32 currentValue in m_Int32Values)
                            {
                                if (currentValue == 0x7FFFFFFF)
                                {
                                    return false;
                                }
                            }

                            return true;
                        }
                    case DataType.UInt32z:
                        {
                            foreach (UInt32 currentValue in m_UInt32Values)
                            {
                                if (currentValue == 0x00000000)
                                {
                                    return false;
                                }
                            }

                            return true;
                        }
                    case DataType.Float32:
                        {
                            foreach (Single currentValue in m_SingleValues)
                            {
                                if (currentValue == 0xFFFFFFFF)
                                {
                                    return false;
                                }
                            }

                            return true;
                        }
                    case DataType.Float64:
                        {
                            foreach (Double currentValue in m_DoubleValues)
                            {
                                if (currentValue == 0xFFFFFFFFFFFFFFFF)
                                {
                                    return false;
                                }
                            }

                            return true;
                        }
                    case DataType.String:
                        {
                            return GetString() != "";
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
        private SByte[] m_SByteValues = null;
        private UInt16[] m_UInt16Values = null;
        private Int16[] m_Int16Values = null;
        private UInt32[] m_UInt32Values = null;
        private Int32[] m_Int32Values = null;
        private Single[] m_SingleValues = null;
        private Double[] m_DoubleValues = null;
        private Byte[] m_ByteValues = null;
        Byte m_StringLength = 0;
    }
}
