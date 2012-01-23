using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace GarminFitnessPlugin.Controller
{
    public class FITParser
    {
        private FITParser()
        {
        }

        public bool Init(Stream stream)
        {
            // Make sure we at least have a header
            if (stream.Length < 12)
            {
                Logger.Instance.LogText("Bad stream length");
                return false;
            }

            Byte[] streamData = new Byte[stream.Length - 2];
            m_DataStream = new MemoryStream();

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(streamData, 0, (int)(stream.Length - 2));

            // Don't copy CRC
            m_DataStream.Write(streamData, 0, (int)stream.Length - 2);

            // Validate crc against the one we can compute
            Byte[] crcData = new Byte[sizeof(UInt16)];
            stream.Read(crcData, 0, 2);
            UInt16 crc = BitConverter.ToUInt16(crcData, 0);

            if (!ValidateCRC(crc))
            {
                Logger.Instance.LogText("Bad CRC");
                Close();
                return false;
            }

            // Validate header
            if (!ValidateHeader())
            {
                Logger.Instance.LogText("Bad header");
                Close();
                return false;
            }

            // Complete initialization
            m_MessageDefinitions = new Dictionary<Byte, FITMessage>();

            return true;
        }

        public void Close()
        {
            m_DataStream = null;
        }

        public void RestartParsing()
        {
            m_DataStream.Seek(12, SeekOrigin.Begin);

            // Empty definitions dictionary
            m_MessageDefinitions = new Dictionary<Byte, FITMessage>();
        }

        public FITMessage ReadNextMessage()
        {
            int readByte;
            Byte recordHeader;

            do
            {
                // Peek record header
                readByte = m_DataStream.ReadByte();
                recordHeader = (Byte)readByte;

                if (readByte != -1)
                {
                    m_DataStream.Seek(-1, SeekOrigin.Current);

                    if (IsDefinitionHeaderByte(recordHeader))
                    {
                        // Definiton record
                        ProcessMessageDefinition();
                    }
                }
            }
            while (readByte != -1 && IsDefinitionHeaderByte(recordHeader));

            // We found a data record, process it using the right header
            if (readByte != -1)
            {
                return ProcessDataMessage();
            }

            return null;
        }

        public FITMessage PrefetchMessageOfType(FITGlobalMessageIds messageType)
        {
            long postitionBookmark = m_DataStream.Position;
            FITMessage readMessage = ReadNextMessage();

            while (readMessage != null &&
                   readMessage.GlobalMessageType != messageType)
            {
                readMessage = ReadNextMessage();
            }

            // Reset to our original position
            m_DataStream.Seek(postitionBookmark, SeekOrigin.Begin);

            return readMessage;
        }

        private void ProcessMessageDefinition()
        {
            FITMessage definition = null;
            Byte[] temp;
            UInt16 globalMessageId = 0;
            Byte recordHeader = 0;
            Byte localDefinitionId = 0;
            Byte endianness = 0;
            Byte fieldCount = 0;
            bool isLittleEndian = true;

            // Record header
            recordHeader = (Byte)m_DataStream.ReadByte();
            Debug.Assert(IsDefinitionHeaderByte(recordHeader));
            localDefinitionId = (Byte)(recordHeader & 0xF);

            // Skip reserved byte
            m_DataStream.ReadByte();

            // Architecture byte
            endianness = (Byte)m_DataStream.ReadByte();
            isLittleEndian = (FITEndianness)endianness == FITEndianness.LittleEndian;

            // Global message id
            temp = new Byte[sizeof(UInt16)];
            m_DataStream.Read(temp, 0, 2);

            if (!isLittleEndian)
            {
                Byte swapHolder = temp[0];

                temp[0] = temp[1];
                temp[1] = swapHolder;
            }
            globalMessageId = BitConverter.ToUInt16(temp, 0);

            // Field count
            fieldCount = (Byte)m_DataStream.ReadByte();

            try
            {
                definition = new FITMessage((FITGlobalMessageIds)globalMessageId, isLittleEndian);
            }
            catch(Exception e)
            {
                // Unsuported message type
                throw e;
            }

            // Read fields
            for (int i = 0; i < fieldCount; ++i)
            {
                Byte fieldId = 0;
                Byte fieldSize = 0;
                Byte fieldType = 0;

                fieldId = (Byte)m_DataStream.ReadByte();
                fieldSize = (Byte)m_DataStream.ReadByte();
                fieldType = (Byte)m_DataStream.ReadByte();

                definition.AddField(new FITMessageField(fieldId, fieldType, fieldSize));
            }

            // We might overwrite an old message, this is valid
            m_MessageDefinitions[localDefinitionId] = definition;
        }

        private FITMessage ProcessDataMessage()
        {
            Byte recordHeader;
            Byte localMessageId;
            FITMessage messageToDeserialize;

            recordHeader = (Byte)m_DataStream.ReadByte();

            if ((recordHeader & 0x80) == 0)
            {
                // Normal header
                localMessageId = (Byte)(recordHeader & 0x0F);
            }
            else
            {
                // Compressed header
                localMessageId = (Byte)((recordHeader >> 5) & 0x03);
            }

            if (m_MessageDefinitions.ContainsKey(localMessageId))
            {
                messageToDeserialize = m_MessageDefinitions[localMessageId];

                messageToDeserialize.DeserializeDataMessage(m_DataStream);

                return messageToDeserialize;
            }

            return null;
        }

        private bool ValidateCRC(UInt16 fileCRC)
        {
            m_DataStream.Seek(0, SeekOrigin.Begin);

            UInt16 crc = FITUtils.ComputeStreamCRC(m_DataStream);

            Logger.Instance.LogText(String.Format("CRC expected = {0}, computed = {1}", fileCRC, crc));

            return fileCRC == crc;
        }

        private bool ValidateHeader()
        {
            m_DataStream.Seek(0, SeekOrigin.Begin);

            // Check header size
            if (m_DataStream.ReadByte() != 12)
            {
                Logger.Instance.LogText("Bad Header size");
                return false;
            }

            // Check version support
            Byte versionByte = (Byte)m_DataStream.ReadByte();
            if ((versionByte >> 4 & 0xF) > FITConstants.FITProfileMajorVersion)
            {
                Logger.Instance.LogText("Bad version");
                return false;
            }

            // Skip profile version
            m_DataStream.Seek(2, SeekOrigin.Current);

            // Check data size
            byte[] intBuffer = new byte[sizeof(UInt32)];
            m_DataStream.Read(intBuffer, 0, sizeof(UInt32));
            UInt32 dataSize = BitConverter.ToUInt32(intBuffer, 0);
            if (dataSize + 12 != m_DataStream.Length) // Add 14 for 12 header + 2 CRC bytes
            {
                Logger.Instance.LogText("Bad data size");
                return false;
            }

            Byte[] FITDescription = new Byte[4];
            m_DataStream.Read(FITDescription, 0, 4);
            if (!Encoding.UTF8.GetString(FITDescription).Equals(FITConstants.FITFileDescriptor))
            {
                Logger.Instance.LogText("Bad FIT descriptor");
                return false;
            }

            return true;
        }

        private bool IsDefinitionHeaderByte(Byte header)
        {
            return (header & 0x40) != 0 && (header & 0x80) == 0;
        }

        public static FITParser Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new FITParser();
                }

                return m_Instance;
            }
        }

        private static FITParser m_Instance = null;

        private MemoryStream m_DataStream = null;
        private Dictionary<Byte, FITMessage> m_MessageDefinitions = null;
    }
}
