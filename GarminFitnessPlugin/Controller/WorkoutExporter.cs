using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class WorkoutExporter
    {
        public static void ExportWorkout(IWorkout workout, Stream exportStream)
        {
            List<IWorkout> workouts = new List<IWorkout>();

            workouts.Add(workout);

            ExportWorkouts(workouts, exportStream);
        }

        public static void ExportWorkouts(List<IWorkout> workouts, Stream exportStream)
        {
            Debug.Assert(exportStream.CanWrite && exportStream.Length == 0);
            XmlDocument document = new XmlDocument();
            XmlNode database;
            XmlAttribute attribute;

            document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = document.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            document.AppendChild(database);
            
            // xmlns namespace attribute
            attribute = document.CreateAttribute("xmlns");
            attribute.Value = "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2";
            database.Attributes.Append(attribute);

            // xmlns:xsi namespace attribute
            attribute = document.CreateAttribute("xmlns", "xsi", Constants.xmlns);
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            database.Attributes.Append(attribute);

            // xsi:schemaLocation namespace attribute
            attribute = document.CreateAttribute("xsi", "schemaLocation", Constants.xsins);
            attribute.Value = "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2 http://www.garmin.com/xmlschemas/TrainingCenterDatabasev2.xsd http://www.garmin.com/xmlschemas/WorkoutExtension/v1 http://www.garmin.com/xmlschemas/WorkoutExtensionv1.xsd";
            database.Attributes.Append(attribute);

            XmlNode workoutsNode = document.CreateNode(XmlNodeType.Element, "Workouts", null);
            database.AppendChild(workoutsNode);

            foreach (IWorkout currentWorkout in workouts)
            {
                currentWorkout.LastExportDate = DateTime.Now;
                if (currentWorkout.GetSplitPartsCount() == 1)
                {
                    currentWorkout.Serialize(workoutsNode, "Workout", document);
                }
                else
                {
                    List<WorkoutPart> parts = currentWorkout.SplitInSeperateParts();

                    foreach (WorkoutPart part in parts)
                    {
                        part.Serialize(workoutsNode, "Workout", document);
                    }
                }
            }

            document.Save(new StreamWriter(exportStream));
        }

        public static void ExportWorkoutToFIT(IWorkout workout, Stream exportStream, UInt16 fileIdNumber)
        {
            ExportWorkoutToFIT(workout, exportStream, fileIdNumber, true);
        }

        public static void ExportWorkoutToFIT(IWorkout workout, Stream exportStream,
                                              UInt16 fileIdNumber, bool updateExportDate)
        {
            MemoryStream dataStream = new MemoryStream();

            // Reserve size for header
            dataStream.Write(new Byte[12], 0, 12);

            // File id message
            FITMessage fileIdMessage = new FITMessage(FITGlobalMessageIds.FileId);
            FITMessageField fileType = new FITMessageField((Byte)FITFileIdFieldsIds.FileType);
            FITMessageField manufacturerId = new FITMessageField((Byte)FITFileIdFieldsIds.ManufacturerId);
            FITMessageField productId = new FITMessageField((Byte)FITFileIdFieldsIds.ProductId);
            FITMessageField serialNumber = new FITMessageField((Byte)FITFileIdFieldsIds.SerialNumber);
            FITMessageField exportDate = new FITMessageField((Byte)FITFileIdFieldsIds.ExportDate);
            FITMessageField number = new FITMessageField((Byte)FITFileIdFieldsIds.Number);

            fileType.SetEnum((Byte)FITFileTypes.Workout);
            fileIdMessage.AddField(fileType);
            manufacturerId.SetUInt16(1);
            fileIdMessage.AddField(manufacturerId);
            productId.SetUInt16(20119);
            fileIdMessage.AddField(productId);
            serialNumber.SetUInt32z(0);
            fileIdMessage.AddField(serialNumber);
            exportDate.SetUInt32(workout.CreationTimestamp);
            fileIdMessage.AddField(exportDate);
            number.SetUInt16(0);
            fileIdMessage.AddField(number);

            fileIdMessage.Serialize(dataStream);

            // Write workout
            workout.SerializetoFIT(dataStream);

            // Write FIT header at the start of the stream
            GarminFitnessByteRange headerSize = new GarminFitnessByteRange(12);
            GarminFitnessByteRange protocolVersion = new GarminFitnessByteRange((Byte)((FITConstants.FITProtocolMajorVersion << 4) | FITConstants.FITProtocolMinorVersion));
            GarminFitnessUInt16Range profileVersion = new GarminFitnessUInt16Range((UInt16)((FITConstants.FITProfileMajorVersion * FITConstants.FITProfileMajorVersionMultiplier) + FITConstants.FITProfileMinorVersion));
            GarminFitnessInt32Range dataSize = new GarminFitnessInt32Range(0);

            dataStream.Seek(0, SeekOrigin.Begin);
            dataSize.Value = (int)dataStream.Length - 12;

            headerSize.Serialize(dataStream);
            protocolVersion.Serialize(dataStream);
            profileVersion.Serialize(dataStream);
            dataSize.Serialize(dataStream);
            dataStream.Write(Encoding.UTF8.GetBytes(FITConstants.FITFileDescriptor), 0, 4);

            // Write CRC
            GarminFitnessUInt16Range crc = new GarminFitnessUInt16Range(FITUtils.ComputeStreamCRC(dataStream));
            dataStream.Seek(0, SeekOrigin.End);
            crc.Serialize(dataStream);

            // Write all data to output stream
            exportStream.Write(dataStream.GetBuffer(), 0, (int)dataStream.Length);

            if (updateExportDate)
            {
                workout.LastExportDate = DateTime.Now;
            }
        }

        public static void ExportSchedulesFITFile(Stream exportStream, MemoryStream dataStream, UInt16 fileIdNumber)
        {
            MemoryStream tempDataStream = new MemoryStream();

            // Reserve size for header
            tempDataStream.Write(new Byte[12], 0, 12);

            // File id message
            FITMessage fileIdMessage = new FITMessage(FITGlobalMessageIds.FileId);
            FITMessageField fileType = new FITMessageField((Byte)FITFileIdFieldsIds.FileType);
            FITMessageField manufacturerId = new FITMessageField((Byte)FITFileIdFieldsIds.ManufacturerId);
            FITMessageField productId = new FITMessageField((Byte)FITFileIdFieldsIds.ProductId);
            FITMessageField serialNumber = new FITMessageField((Byte)FITFileIdFieldsIds.SerialNumber);
            FITMessageField exportDate = new FITMessageField((Byte)FITFileIdFieldsIds.ExportDate);
            FITMessageField number = new FITMessageField((Byte)FITFileIdFieldsIds.Number);

            manufacturerId.SetUInt16(1);
            fileIdMessage.AddField(manufacturerId);
            fileType.SetEnum((Byte)FITFileTypes.Schedules);
            fileIdMessage.AddField(fileType);

            fileIdMessage.Serialize(tempDataStream);

            // Write all passed in data to output stream
            tempDataStream.Write(dataStream.GetBuffer(), 0, (int)dataStream.Length);

            // Write FIT header at the start of the stream
            GarminFitnessByteRange headerSize = new GarminFitnessByteRange(12);
            GarminFitnessByteRange protocolVersion = new GarminFitnessByteRange((Byte)((FITConstants.FITProtocolMajorVersion << 4) | FITConstants.FITProtocolMinorVersion));
            GarminFitnessUInt16Range profileVersion = new GarminFitnessUInt16Range((UInt16)((FITConstants.FITProfileMajorVersion * FITConstants.FITProfileMajorVersionMultiplier) + FITConstants.FITProfileMinorVersion));
            GarminFitnessInt32Range dataSize = new GarminFitnessInt32Range(0);

            tempDataStream.Seek(0, SeekOrigin.Begin);
            dataSize.Value = (int)tempDataStream.Length - 12;

            headerSize.Serialize(tempDataStream);
            protocolVersion.Serialize(tempDataStream);
            profileVersion.Serialize(tempDataStream);
            dataSize.Serialize(tempDataStream);
            tempDataStream.Write(Encoding.UTF8.GetBytes(FITConstants.FITFileDescriptor), 0, 4);

            // Write CRC
            GarminFitnessUInt16Range crc = new GarminFitnessUInt16Range(FITUtils.ComputeStreamCRC(tempDataStream));
            tempDataStream.Seek(0, SeekOrigin.End);
            crc.Serialize(tempDataStream);

            // Write all data to output stream
            exportStream.Write(tempDataStream.GetBuffer(), 0, (int)tempDataStream.Length);
        }
    }
}
