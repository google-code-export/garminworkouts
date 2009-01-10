using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.View;

namespace GarminFitnessPlugin.Data
{
    class Workout : IWorkout, IDirty
    {
        public Workout(string name, IActivityCategory category)
        {
            m_Name.Value = name;
            Category = category;

            AddStepToRoot(new RegularStep(this));
        }

        public Workout(string name, IActivityCategory category, List<IStep> steps)
        {
            m_Name.Value = name;
            Category = category;

            AddStepsToRoot(steps);
        }

        public Workout(Stream stream, DataVersion version)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            m_Name.Serialize(stream);

            m_Notes.Serialize(stream);

            // Category ID
            stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(Category.ReferenceId)), 0, sizeof(Int32));
            stream.Write(Encoding.UTF8.GetBytes(Category.ReferenceId), 0, Encoding.UTF8.GetByteCount(Category.ReferenceId));

            // Steps
            stream.Write(BitConverter.GetBytes(Steps.Count), 0, sizeof(Int32));
            for (int i = 0; i < Steps.Count; ++i)
            {
                Steps[i].Serialize(stream);
            }

            // Scheduled dates
            stream.Write(BitConverter.GetBytes(ScheduledDates.Count), 0, sizeof(Int32));
            for (int i = 0; i < ScheduledDates.Count; ++i)
            {
                ScheduledDates[i].Serialize(stream);
            }

            m_LastExportDate.Serialize(stream);

            m_AddToDailyViewOnSchedule.Serialize(stream);
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] stringBuffer;
            Int32 stringLength;
            Int32 stepCount;

            m_Name.Deserialize(stream, version);

            m_Notes.Deserialize(stream, version);

            // Category
            stream.Read(intBuffer, 0, sizeof(Int32));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            Category = Utils.FindCategoryByIDSafe(Encoding.UTF8.GetString(stringBuffer));

            // Steps
            stream.Read(intBuffer, 0, sizeof(Int32));
            stepCount = BitConverter.ToInt32(intBuffer, 0);
            Steps.Clear();
            for (int i = 0; i < stepCount; ++i)
            {
                IStep.StepType type;

                stream.Read(intBuffer, 0, sizeof(Int32));
                type = (IStep.StepType)BitConverter.ToInt32(intBuffer, 0);

                if (type == IStep.StepType.Regular)
                {
                    AddStepToRoot(new RegularStep(stream, version, this));
                }
                else
                {
                    AddStepToRoot(new RepeatStep(stream, version, this));
                }
            }
        }

        public void Deserialize_V4(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] dateBuffer = new byte[sizeof(long)];
            Int32 scheduledDatesCount;

            Deserialize_V0(stream, version);

            // Scheduled dates
            stream.Read(intBuffer, 0, sizeof(Int32));
            scheduledDatesCount = BitConverter.ToInt32(intBuffer, 0);
            ScheduledDates.Clear();
            for (int i = 0; i < scheduledDatesCount; ++i)
            {
                GarminFitnessDate newDate = new GarminFitnessDate();

                newDate.Deserialize(stream, version);
                ScheduleWorkout(newDate);
            }
        }

        public void Deserialize_V5(Stream stream, DataVersion version)
        {
            byte[] dateBuffer = new byte[sizeof(long)];

            Deserialize_V4(stream, version);

            m_LastExportDate.Deserialize(stream, version);
        }

        public void Deserialize_V12(Stream stream, DataVersion version)
        {
            byte[] dateBuffer = new byte[sizeof(long)];

            Deserialize_V5(stream, version);

            m_AddToDailyViewOnSchedule.Deserialize(stream, version);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            List<IStep> steps = new List<IStep>();
            XmlNode StepsExtensionsNode = null;
            XmlNode STExtensionsNode = null;
            bool nameRead = false;

            ScheduledDates.Clear();

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode child = parentNode.ChildNodes.Item(i);

                if (child.Name == "Name")
                {
                    m_Name.Deserialize(child);
                    nameRead = true;
                }
                else if (child.Name == "Step")
                {
                    if (child.Attributes.Count == 1 && child.Attributes[0].Name == Constants.XsiTypeTCXString)
                    {
                        string stepTypeString = child.Attributes[0].Value;
                        IStep newStep = null;

                        if (stepTypeString == Constants.StepTypeTCXString[(int)IStep.StepType.Regular])
                        {
                            newStep = new RegularStep(this);
                        }
                        else if (stepTypeString == Constants.StepTypeTCXString[(int)IStep.StepType.Repeat])
                        {
                            newStep = new RepeatStep(this);
                        }
                        else
                        {
                            Debug.Assert(false);
                        }

                        if (newStep != null)
                        {
                            newStep.Deserialize(child);
                            steps.Add(newStep);
                        }
                    }
                }
                else if (child.Name == "Notes")
                {
                    m_Notes.Deserialize(child);
                }
                else if (child.Name == "ScheduledOn")
                {
                    GarminFitnessDate newDate = new GarminFitnessDate();

                    newDate.Deserialize(child);
                    ScheduleWorkout(newDate);
                }
                else if (child.Name == Constants.ExtensionsTCXString)
                {
                    for (int j = 0; j < child.ChildNodes.Count; ++j)
                    {
                        XmlNode currentNode = child.ChildNodes[j];

                        // This condition remains for backsward compatibility with V0 exports
                        if (currentNode.Name == "SportTracksCategory" &&
                            currentNode.FirstChild.ChildNodes.Count == 1 &&
                            currentNode.FirstChild.FirstChild.GetType() == typeof(XmlText))
                        {
                            XmlText categoryNode = (XmlText)child.FirstChild.FirstChild;

                            Category = Utils.FindCategoryByIDSafe(categoryNode.Value);
                        }
                        else if (currentNode.Name == "SportTracksExtensions")
                        {
                            STExtensionsNode = currentNode;
                        }
                        else if(currentNode.Name == "Steps")
                        {
                            StepsExtensionsNode = currentNode;
                        }
                    }
                }
            }

            if (!nameRead || steps.Count < 1)
            {
                throw new GarminFitnesXmlDeserializationException("Information missing in the XML node", parentNode);
            }
            else if( steps.Count > 20)
            {
                throw new GarminFitnesXmlDeserializationException("Too many steps in the XML node", parentNode);
            }

            m_Name.Value = GarminWorkoutManager.Instance.GetUniqueName(Name);
            Steps.Clear();

            for (int i = 0; i < steps.Count; ++i)
            {
                AddStepToRoot(steps[i]);
            }

            if (StepsExtensionsNode != null)
            {
                HandleStepExtension(StepsExtensionsNode);
            }

            if (STExtensionsNode != null)
            {
                HandleSTExtension(STExtensionsNode);
            }
        }

        public Workout Clone()
        {
            Workout result;
            MemoryStream stream = new MemoryStream();

            Serialize(stream);

            // Put back at start but skip the first 4 bytes which are the step type
            stream.Seek(0, SeekOrigin.Begin);

            result = GarminWorkoutManager.Instance.CreateWorkout(stream, Constants.CurrentVersion);

            return result;
        }

        public Workout Clone(IActivityCategory newCategory)
        {
            Workout result;
            MemoryStream stream = new MemoryStream();

            Serialize(stream);

            // Put back at start but skip the first 4 bytes which are the step type
            stream.Seek(0, SeekOrigin.Begin);

            result = GarminWorkoutManager.Instance.CreateWorkout(stream, Constants.CurrentVersion, newCategory);

            stream.Close();

            return result;
        }

        private void HandleStepExtension(XmlNode extensionsNode)
        {
            for (int i = 0; i < extensionsNode.ChildNodes.Count; ++i)
            {
                XmlNode currentExtension = extensionsNode.ChildNodes[i];

                if (currentExtension.Name == "Step")
                {
                    IStep step = null;

                    for (int j = 0; j < currentExtension.ChildNodes.Count; ++j)
                    {
                        XmlNode childNode = currentExtension.ChildNodes[j];

                        if (childNode.Name == "StepId")
                        {
                            try
                            {
                                Byte id = Byte.Parse(((XmlText)childNode.FirstChild).Value);

                                step = GetStepById(id);
                            }
                            catch
                            {
                            }
                        }
                        else if (childNode.Name == "Target" && childNode.Attributes.Count == 1 &&
                            childNode.Attributes[0].Name == Constants.XsiTypeTCXString &&
                            childNode.Attributes[0].Value == Constants.TargetTypeTCXString[(int)ITarget.TargetType.Power])
                        {
                            Debug.Assert(step != null && step.Type == IStep.StepType.Regular);
                            RegularStep concreteStep = (RegularStep)step;

                            TargetFactory.Create(ITarget.TargetType.Power, childNode, concreteStep);
                        }
                    }
                }
            }
        }

        private void HandleSTExtension(XmlNode extensionsNode)
        {
            for (int i = 0; i < extensionsNode.ChildNodes.Count; ++i)
            {
                XmlNode currentExtension = extensionsNode.ChildNodes[i];

                if (currentExtension.Name == "SportTracksCategory")
                {
                    if (currentExtension.ChildNodes.Count == 1 &&
                        currentExtension.FirstChild.GetType() == typeof(XmlText))
                    {
                        XmlText categoryNode = (XmlText)currentExtension.FirstChild;

                        Category = Utils.FindCategoryByIDSafe(categoryNode.Value);
                    }
                }
                else if (currentExtension.Name == "TargetOverride")
                {
                    XmlNode idNode = null;
                    XmlNode categoryNode = null;

                    for (int j = 0; j < currentExtension.ChildNodes.Count; ++j)
                    {
                        XmlNode childNode = currentExtension.ChildNodes[j];

                        if (childNode.Name == "StepId" && childNode.ChildNodes.Count == 1 &&
                            childNode.FirstChild.GetType() == typeof(XmlText) && 
                            Utils.IsTextIntegerInRange(childNode.FirstChild.Value, 1, 20))
                        {
                            idNode = childNode.FirstChild;
                        }
                        else if (childNode.Name == "Category")
                        {
                            categoryNode = childNode;
                        }
                    }

                    if (idNode != null && categoryNode != null)
                    {
                        int stepId = Byte.Parse(idNode.Value);
                        IStep step = GetStepById(stepId);

                        if(step != null)
                        {
                            Debug.Assert(step.Type == IStep.StepType.Regular);

                            RegularStep concreteStep = (RegularStep)step;

                            concreteStep.HandleTargetOverride(categoryNode);
                        }
                    }
                }
                else if (currentExtension.Name == "StepNotes")
                {
                    XmlNode idNode = null;
                    XmlNode notesNode = null;

                    for (int j = 0; j < currentExtension.ChildNodes.Count; ++j)
                    {
                        XmlNode childNode = currentExtension.ChildNodes[j];

                        if (childNode.Name == "StepId" && childNode.ChildNodes.Count == 1 &&
                            childNode.FirstChild.GetType() == typeof(XmlText) &&
                            Utils.IsTextIntegerInRange(childNode.FirstChild.Value, 1, 20))
                        {
                            idNode = childNode.FirstChild;
                        }
                        else if (childNode.Name == "Notes" && childNode.ChildNodes.Count == 1 &&
                                 childNode.FirstChild.GetType() == typeof(XmlText))
                        {
                            notesNode = childNode;
                        }
                    }

                    if (idNode != null && notesNode != null)
                    {
                        int stepId = Byte.Parse(idNode.Value);
                        IStep step = GetStepById(stepId);

                        step.Notes = notesNode.FirstChild.Value;
                    }
                }
            }
        }

        public List<IStep> DeserializeSteps(Stream stream)
        {
            List<IStep> deserializedSteps = new List<IStep>();
            byte[] intBuffer = new byte[sizeof(Int32)];
            Byte stepCount = (Byte)stream.ReadByte();

            for (int i = 0; i < stepCount; i++)
            {
                IStep.StepType type;

                stream.Read(intBuffer, 0, sizeof(Int32));
                type = (IStep.StepType)BitConverter.ToInt32(intBuffer, 0);

                if (type == IStep.StepType.Regular)
                {
                    deserializedSteps.Add(new RegularStep(stream, Constants.CurrentVersion, this));
                }
                else
                {
                    deserializedSteps.Add(new RepeatStep(stream, Constants.CurrentVersion, this));
                }
            }

            // Now that we deserialized, paste in the current workout
            if (AddStepsToRoot(deserializedSteps))
            {
                return deserializedSteps;
            }

            return null;
        }

        public override int GetStepCount()
        {
            byte stepCount = 0;

            for (int i = 0; i < m_Steps.Count; ++i)
            {
                stepCount += m_Steps[i].GetStepCount();
            }

            return stepCount;
        }

        public override bool CanAcceptNewStep(int newStepCount, IStep destinationStep)
        {
            // Hard limit at PluginMaxStepsPerWorkout steps
            if (GetStepCount() + newStepCount > Constants.PluginMaxStepsPerWorkout)
            {
                return false;
            }

            IStep topMostRepeat = GetTopMostRepeatForStep(destinationStep);

            if (topMostRepeat == null)
            {
                // Destination is not part of a repeat
                if (Options.Instance.AllowSplitWorkouts)
                {
                    return true;
                }
                else
                {
                    return GetStepCount() + newStepCount <= Constants.MaxStepsPerWorkout;
                }
            }
            else
            {
                // When destination is part of a repeat it's a bit easier, there's a 19
                //  child steps limit.  Since this will occur of the repeat of the base
                //  level, check if we bust the limit
                if (Options.Instance.AllowSplitWorkouts)
                {
                    return topMostRepeat.GetStepCount() + newStepCount <= Constants.MaxStepsPerWorkout;
                }
                else
                {
                    return GetStepCount() + newStepCount <= Constants.MaxStepsPerWorkout;
                }
            }
        }

        public override Workout ConcreteWorkout
        {
            get { return this; }
        }

        public override DateTime LastExportDate
        {
            get { return m_LastExportDate; }
            set
            {
                if (m_LastExportDate != value)
                {
                    m_LastExportDate.Value = value;

                    TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("ExportDate"));
                }
            }
        }

        public override List<GarminFitnessDate> ScheduledDates
        {
            get { return m_ScheduledDates; }
        }

        public override List<IStep> Steps
        {
            get { return m_Steps; }
        }

        public override IActivityCategory Category
        {
            get { return m_Category; }
            set
            {
                if (m_Category != value)
                {
                    Debug.Assert(value != null);
                    m_Category = value;

                    TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("Category"));
                }
            }
        }

        public override GarminFitnessString NameInternal
        {
            get { return m_Name; }
        }

        public override GarminFitnessString NotesInternal
        {
            get { return m_Notes; }
        }

        public override bool AddToDailyViewOnSchedule
        {
            get { return m_AddToDailyViewOnSchedule; }
            set
            {
                if (AddToDailyViewOnSchedule != value)
                {
                    m_AddToDailyViewOnSchedule.Value = value;

                    TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("AddToDailyViewOnSchedule"));
                }
            }
        }

        public bool IsDirty
        {
            get
            {
                for (int i = 0; i < m_Steps.Count; ++i)
                {
                    if (m_Steps[i].IsDirty)
                    {
                        return true;
                    }
                }

                return false;
            }
            set { Debug.Assert(false); }
        }

        private GarminFitnessDate m_LastExportDate = new GarminFitnessDate();
        private List<GarminFitnessDate> m_ScheduledDates = new List<GarminFitnessDate>();
        private List<IStep> m_Steps = new List<IStep>();
        private IActivityCategory m_Category;
        private GarminFitnessString m_Name = new GarminFitnessString("", 15);
        private GarminFitnessString m_Notes = new GarminFitnessString("", 30000);
        private GarminFitnessBool m_AddToDailyViewOnSchedule = new GarminFitnessBool(false);
    }
}
