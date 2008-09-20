using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using GarminWorkoutPlugin.Data;

namespace GarminWorkoutPlugin.Controller
{
    class WorkoutExporter
    {

        public static void ExportWorkout(Workout workout, Stream exportStream)
        {
            ExportWorkout(workout, exportStream, false);
        }

        public static void ExportWorkout(Workout workout, Stream exportStream, bool skipExtensions)
        {
            Trace.Assert(exportStream.CanWrite && exportStream.Length == 0);
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
            attribute.Value = Constants.xsins;
            database.Attributes.Append(attribute);

            // xsi:schemaLocation namespace attribute
            attribute = document.CreateAttribute("xsi", "schemaLocation", Constants.xsins);
            attribute.Value = "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2 http://www.garmin.com/xmlschemas/TrainingCenterDatabasev2.xsd";
            database.Attributes.Append(attribute);
/*
            {
                XmlNode foldersNode = document.CreateNode(XmlNodeType.Element, "Folders", null);
                database.AppendChild(foldersNode);
                XmlNode workoutsNodeTemp = document.CreateNode(XmlNodeType.Element, "Workouts", null);
                foldersNode.AppendChild(workoutsNodeTemp);
                XmlNode temp = document.CreateNode(XmlNodeType.Element, "Running", null);
                workoutsNodeTemp.AppendChild(temp);
                attribute = document.CreateAttribute(null, "Name", null);
                attribute.Value = "Running";
                temp.Attributes.Append(attribute);
                temp = document.CreateNode(XmlNodeType.Element, "Biking", null);
                workoutsNodeTemp.AppendChild(temp);
                attribute = document.CreateAttribute(null, "Name", null);
                attribute.Value = "Biking";
                temp.Attributes.Append(attribute);
                XmlNode otherNode = document.CreateNode(XmlNodeType.Element, "Other", null);
                workoutsNodeTemp.AppendChild(otherNode);
                attribute = document.CreateAttribute(null, "Name", null);
                attribute.Value = "Other";
                otherNode.Attributes.Append(attribute);
                XmlNode nameRefNode = document.CreateNode(XmlNodeType.Element, "WorkoutNameRef", null);
                otherNode.AppendChild(nameRefNode);
                XmlNode IdNode = document.CreateNode(XmlNodeType.Element, "Id", null);
                nameRefNode.AppendChild(IdNode);
                IdNode.AppendChild(document.CreateTextNode(workout.Name));
            }
*/
            XmlNode workoutsNode = document.CreateNode(XmlNodeType.Element, "Workouts", null);
            database.AppendChild(workoutsNode);
            ExportWorkoutInternal(workout, document, workoutsNode, skipExtensions);

            document.Save(new StreamWriter(exportStream));
        }

        private static void ExportWorkoutInternal(Workout workout, XmlDocument document, XmlNode parentNode, bool skipExtensions)
        {
            XmlNode workoutNode = document.CreateElement("Workout");

            workout.Serialize(workoutNode, document, skipExtensions);
            parentNode.AppendChild(workoutNode);
        }
    }
}
