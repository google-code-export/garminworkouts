using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.Controller;
using System.ComponentModel;

namespace GarminFitnessPlugin.Data
{
    class Workout : IPluginSerializable, IXMLSerializable, IDirty
    {
        public Workout(string name, IActivityCategory category)
        {
            Name = name;

            m_Steps.Add(new RegularStep(this));
            Category = category;
        }

        public Workout(Stream stream, DataVersion version)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            // Name
            stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(Name)), 0, sizeof(Int32));
            stream.Write(Encoding.UTF8.GetBytes(Name), 0, Encoding.UTF8.GetByteCount(Name));

            // Notes
            if (Notes != null && Notes != String.Empty)
            {
                stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(Notes)), 0, sizeof(Int32));
                stream.Write(Encoding.UTF8.GetBytes(Notes), 0, Encoding.UTF8.GetByteCount(Notes));
            }
            else
            {
                stream.Write(BitConverter.GetBytes((Int32)0), 0, sizeof(Int32));
            }

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
                stream.Write(BitConverter.GetBytes(ScheduledDates[i].Ticks), 0, sizeof(long));
            }

            // Last export date
            stream.Write(BitConverter.GetBytes(LastExportDate.Ticks), 0, sizeof(long));
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] stringBuffer;
            Int32 stringLength;
            Int32 stepCount;

            // Name
            stream.Read(intBuffer, 0, sizeof(Int32));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            Name = Encoding.UTF8.GetString(stringBuffer);

            // Notes
            stream.Read(intBuffer, 0, sizeof(Int32));
            stringLength = BitConverter.ToInt32(intBuffer, 0);

            if (stringLength > 0)
            {
                stringBuffer = new byte[stringLength];
                stream.Read(stringBuffer, 0, stringLength);
                Notes = Encoding.UTF8.GetString(stringBuffer);
            }
            else
            {
                Notes = String.Empty;
            }

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
                    Steps.Add(new RegularStep(stream, version, this));
                }
                else
                {
                    Steps.Add(new RepeatStep(stream, version, this));
                }
            }
        }

        public void Deserialize_V4(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] dateBuffer = new byte[sizeof(long)];
            byte[] stringBuffer;
            Int32 stringLength;
            Int32 stepCount;
            Int32 scheduledDatesCount;

            // Name
            stream.Read(intBuffer, 0, sizeof(Int32));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            Name = Encoding.UTF8.GetString(stringBuffer);

            // Notes
            stream.Read(intBuffer, 0, sizeof(Int32));
            stringLength = BitConverter.ToInt32(intBuffer, 0);

            if (stringLength > 0)
            {
                stringBuffer = new byte[stringLength];
                stream.Read(stringBuffer, 0, stringLength);
                Notes = Encoding.UTF8.GetString(stringBuffer);
            }
            else
            {
                Notes = String.Empty;
            }

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
                    Steps.Add(new RegularStep(stream, version, this));
                }
                else
                {
                    Steps.Add(new RepeatStep(stream, version, this));
                }
            }

            // Scheduled dates
            stream.Read(intBuffer, 0, sizeof(Int32));
            scheduledDatesCount = BitConverter.ToInt32(intBuffer, 0);
            ScheduledDates.Clear();
            for (int i = 0; i < scheduledDatesCount; ++i)
            {
                long scheduledDateInTicks;

                stream.Read(dateBuffer, 0, sizeof(long));
                scheduledDateInTicks = BitConverter.ToInt64(dateBuffer, 0);

                if (scheduledDateInTicks >= DateTime.Today.Ticks)
                {
                    ScheduledDates.Add(new DateTime(scheduledDateInTicks));
                }
            }
        }

        public void Deserialize_V5(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] dateBuffer = new byte[sizeof(long)];
            byte[] stringBuffer;
            Int32 stringLength;
            Int32 stepCount;
            Int32 scheduledDatesCount;

            // Name
            stream.Read(intBuffer, 0, sizeof(Int32));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            Name = Encoding.UTF8.GetString(stringBuffer);

            // Notes
            stream.Read(intBuffer, 0, sizeof(Int32));
            stringLength = BitConverter.ToInt32(intBuffer, 0);

            if (stringLength > 0)
            {
                stringBuffer = new byte[stringLength];
                stream.Read(stringBuffer, 0, stringLength);
                Notes = Encoding.UTF8.GetString(stringBuffer);
            }
            else
            {
                Notes = String.Empty;
            }

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
                    Steps.Add(new RegularStep(stream, version, this));
                }
                else
                {
                    Steps.Add(new RepeatStep(stream, version, this));
                }
            }

            // Scheduled dates
            stream.Read(intBuffer, 0, sizeof(Int32));
            scheduledDatesCount = BitConverter.ToInt32(intBuffer, 0);
            ScheduledDates.Clear();
            for (int i = 0; i < scheduledDatesCount; ++i)
            {
                long scheduledDateInTicks;

                stream.Read(dateBuffer, 0, sizeof(long));
                scheduledDateInTicks = BitConverter.ToInt64(dateBuffer, 0);

                if (scheduledDateInTicks >= DateTime.Today.Ticks)
                {
                    ScheduledDates.Add(new DateTime(scheduledDateInTicks));
                }
            }

            // Last export date
            stream.Read(dateBuffer, 0, sizeof(long));
            LastExportDate = new DateTime(BitConverter.ToInt64(dateBuffer, 0));
        }

        public void Serialize(XmlNode parentNode, XmlDocument document)
        {
            Serialize(parentNode, document, false);
        }

        public void Serialize(XmlNode parentNode, XmlDocument document, bool skipExtensions)
        {
            XmlNode childNode;
            XmlAttribute attribute;

            // Sport attribute
            attribute = document.CreateAttribute(null, "Sport", null);
            attribute.Value = Constants.GarminCategoryTCXString[(int)Options.GetGarminCategory(Category)];
            parentNode.Attributes.Append(attribute);

            // Name
            childNode = document.CreateElement("Name");
            childNode.AppendChild(document.CreateTextNode(Name));
            parentNode.AppendChild(childNode);

            // Export all steps
            for (int i = 0; i < Steps.Count; ++i)
            {
                childNode = document.CreateElement("Step");

                Steps[i].Serialize(childNode, document);
                parentNode.AppendChild(childNode);
            }

            // Scheduled dates
            for (int i = 0; i < ScheduledDates.Count; ++i)
            {
                childNode = document.CreateElement("ScheduledOn");
                childNode.AppendChild(document.CreateTextNode(ScheduledDates[i].ToString("yyyy-MM-dd")));
                parentNode.AppendChild(childNode);
            }

            // Notes
            if (Notes != String.Empty && Notes != null)
            {
                childNode = document.CreateElement("Notes");
                childNode.AppendChild(document.CreateTextNode(Notes));
                parentNode.AppendChild(childNode);
            }

            // Extensions
            if (!skipExtensions)
            {
                childNode = document.CreateElement("Extensions");
                // Steps extensions
                if (m_StepsExtensions.Count > 0)
                {
                    XmlNode extensionsNode = document.CreateElement("Steps");
                    attribute = document.CreateAttribute("xmlns");
                    attribute.Value = "http://www.garmin.com/xmlschemas/WorkoutExtension/v1";
                    extensionsNode.Attributes.Append(attribute);

                    for (int i = 0; i < m_StepsExtensions.Count; ++i)
                    {
                        XmlNode currentExtension = m_StepsExtensions[i];

                        extensionsNode.AppendChild(currentExtension);
                    }
                    childNode.AppendChild(extensionsNode);

                    m_StepsExtensions.Clear();
                }

                // ST extension
                if (m_STExtensions.Count > 0)
                {
                    XmlNode extensionsNode = document.CreateElement("SportTracksExtensions");
                    attribute = document.CreateAttribute("xmlns");
                    attribute.Value = "http://www.zonefivesoftware.com/SportTracks/Plugins/plugin_detail.php?id=97";
                    extensionsNode.Attributes.Append(attribute);

                    // Category
                    XmlNode categoryNode = document.CreateElement("SportTracksCategory");
                    categoryNode.AppendChild(document.CreateTextNode(Category.ReferenceId));
                    extensionsNode.AppendChild(categoryNode);

                    for (int i = 0; i < m_STExtensions.Count; ++i)
                    {
                        XmlNode currentExtension = m_STExtensions[i];

                        extensionsNode.AppendChild(currentExtension);
                    }
                    childNode.AppendChild(extensionsNode);

                    m_STExtensions.Clear();
                }
                parentNode.AppendChild(childNode);
            }
        }

        public bool Deserialize(XmlNode parentNode)
        {
            string workoutName = "";
            IActivityCategory category = PluginMain.GetApplication().Logbook.ActivityCategories[0];
            List<IStep> steps = new List<IStep>();
            XmlNode StepsExtensionsNode = null;
            XmlNode STExtensionsNode = null;

            ScheduledDates.Clear();

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode child = parentNode.ChildNodes.Item(i);

                if (child.Name == "Name")
                {
                    if (child.ChildNodes.Count != 1 || child.FirstChild.GetType() != typeof(XmlText))
                    {
                        return false;
                    }

                    XmlText nameNode = (XmlText)child.FirstChild;
                    workoutName = nameNode.Value;
                }
                else if (child.Name == "Step")
                {
                    if (child.Attributes.Count == 1 && child.Attributes[0].Name == "xsi:type")
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

                        if (newStep != null && newStep.Deserialize(child))
                        {
                            steps.Add(newStep);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else if (child.Name == "Notes")
                {
                    if (child.ChildNodes.Count != 1 || child.FirstChild.GetType() != typeof(XmlText))
                    {
                        return false;
                    }

                    Notes = ((XmlText)child.FirstChild).Value;
                }
                else if (child.Name == "ScheduledOn")
                {
                    CultureInfo info = new CultureInfo("En-us");

                    if (child.ChildNodes.Count != 1 || child.FirstChild.GetType() != typeof(XmlText))
                    {
                        return false;
                    }

                    ScheduledDates.Add(DateTime.ParseExact(((XmlText)child.FirstChild).Value, "yyyy-MM-dd", info.DateTimeFormat));
                }
                else if (child.Name == "Extensions")
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

                            category = Utils.FindCategoryByIDSafe(categoryNode.Value);
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

            if (workoutName == String.Empty || steps.Count < 1 || steps.Count > 20)
            {
                return false;
            }
            else
            {
                string finalName = GarminWorkoutManager.Instance.GetUniqueName(workoutName);

                Name = finalName;
                Steps.Clear();
                Steps.AddRange(steps);
                Category = category;

                if (StepsExtensionsNode != null)
                {
                    HandleStepExtension(StepsExtensionsNode);
                }

                if (STExtensionsNode != null)
                {
                    HandleSTExtension(STExtensionsNode);
                }

                return true;
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
                            childNode.Attributes[0].Name == "xsi:type" &&
                            childNode.Attributes[0].Value == Constants.TargetTypeTCXString[(int)ITarget.TargetType.Power])
                        {
                            Trace.Assert(step != null && step.Type == IStep.StepType.Regular);
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
                            Trace.Assert(step.Type == IStep.StepType.Regular);

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

        public byte GetStepCount()
        {
            byte stepCount = 0;

            for (int i = 0; i < m_Steps.Count; ++i)
            {
                stepCount += m_Steps[i].GetStepCount();
            }

            return stepCount;
        }

        public IStep GetStepById(int id)
        {
            return GetStepByIdInternal(Steps, id, 0);
        }

        private static IStep GetStepByIdInternal(IList<IStep> steps, int id, int baseId)
        {
            int currentId = baseId;

            for (int i = 0; i < steps.Count; ++i)
            {
                IStep currentStep = steps[i];

                if (currentId + currentStep.GetStepCount() == id)
                {
                    return currentStep;
                }
                else if (currentStep.Type == IStep.StepType.Repeat && id <= currentId + currentStep.GetStepCount())
                {
                    RepeatStep concreteStep = (RepeatStep)currentStep;
                    return GetStepByIdInternal(concreteStep.StepsToRepeat, id, currentId);
                }

                currentId += currentStep.GetStepCount();
            }

            return null;
        }

        public void AddSportTracksExtension(XmlNode extensionNode)
        {
            m_STExtensions.Add(extensionNode);
        }

        public void AddStepExtension(XmlNode extensionNode)
        {
            m_StepsExtensions.Add(extensionNode);
        }

        public bool ValidateAfterZoneCategoryChanged(IZoneCategory changedCategory)
        {
            bool valueChanged = false;

            // Validate all steps
            for (int i = 0; i < m_Steps.Count; ++i)
            {
                if (m_Steps[i].ValidateAfterZoneCategoryChanged(changedCategory))
                {
                    valueChanged = true;
                }
            }

            return valueChanged;
        }

        public void MarkAllCadenceSTZoneTargetsAsDirty()
        {
            for (int i = 0; i < m_Steps.Count; ++i)
            {
                if (m_Steps[i].Type == IStep.StepType.Regular)
                {
                    RegularStep concreteStep = (RegularStep)m_Steps[i];

                    if (concreteStep.Target.Type == ITarget.TargetType.Cadence)
                    {
                        BaseCadenceTarget baseTarget = (BaseCadenceTarget)concreteStep.Target;

                        if (baseTarget.ConcreteTarget.Type == IConcreteCadenceTarget.CadenceTargetType.ZoneST)
                        {
                            CadenceZoneSTTarget concreteTarget = (CadenceZoneSTTarget)baseTarget.ConcreteTarget;

                            concreteTarget.Zone = Options.CadenceZoneCategory.Zones[0];
                            concreteTarget.IsDirty = true;
                        }
                    }
                }
                else if (m_Steps[i].Type == IStep.StepType.Repeat)
                {
                    RepeatStep concreteStep = (RepeatStep)m_Steps[i];

                    concreteStep.MarkAllCadenceSTZoneTargetsAsDirty();
                }
            }
        }

        public void MarkAllPowerSTZoneTargetsAsDirty()
        {
            for (int i = 0; i < m_Steps.Count; ++i)
            {
                if (m_Steps[i].Type == IStep.StepType.Regular)
                {
                    RegularStep concreteStep = (RegularStep)m_Steps[i];

                    if (concreteStep.Target.Type == ITarget.TargetType.Power)
                    {
                        BasePowerTarget baseTarget = (BasePowerTarget)concreteStep.Target;

                        if (baseTarget.ConcreteTarget.Type == IConcretePowerTarget.PowerTargetType.ZoneST)
                        {
                            PowerZoneSTTarget concreteTarget = (PowerZoneSTTarget)baseTarget.ConcreteTarget;

                            concreteTarget.Zone = Options.PowerZoneCategory.Zones[0];
                            concreteTarget.IsDirty = true;
                        }
                    }
                }
                else if (m_Steps[i].Type == IStep.StepType.Repeat)
                {
                    RepeatStep concreteStep = (RepeatStep)m_Steps[i];

                    concreteStep.MarkAllPowerSTZoneTargetsAsDirty();
                }
            }
        }

        public IStep GetNextStep(IStep previousStep)
        {
            UInt16 index;
            List<IStep> parentList;

            if (Utils.GetStepInfo(previousStep, Steps, out parentList, out index))
            {
                if (index != parentList.Count - 1)
                {
                    index++;
                }

                return parentList[index];
            }

            return null;
        }

        public IStep GetPreviousStep(IStep nextStep)
        {
            UInt16 index;
            List<IStep> parentList;

            if (Utils.GetStepInfo(nextStep, Steps, out parentList, out index))
            {
                if (index != 0)
                {
                    return parentList[index - 1];
                }
            }

            return null;
        }

        public void AddStepsToRoot(List<IStep> stepsToAdd)
        {
            InsertStepsAfterStep(Steps[Steps.Count - 1], stepsToAdd);
        }

        public void InsertStepInRoot(int index, IStep stepToAdd)
        {
            List<IStep> stepsToAdd = new List<IStep>();

            stepsToAdd.Add(stepToAdd);

            InsertStepsBeforeStep(Steps[index], stepsToAdd);
        }

        public void InsertStepsInRoot(int index, List<IStep> stepsToAdd)
        {
            InsertStepsBeforeStep(Steps[index], stepsToAdd);
        }

        public void InsertStepAfterStep(IStep previousStep, IStep stepToAdd)
        {
            List<IStep> stepsToAdd = new List<IStep>();

            stepsToAdd.Add(stepToAdd);

            InsertStepsAfterStep(previousStep, stepsToAdd);
        }

        public void InsertStepsAfterStep(IStep previousStep, List<IStep> stepsToAdd)
        {
            Trace.Assert(previousStep != null);

            UInt16 previousPosition = 0;
            List<IStep> parentList = null;

            if (Utils.GetStepInfo(previousStep, Steps, out parentList, out previousPosition))
            {
                List<IStep> tempList;
                UInt16 tempPosition = 0;
                bool stepAdded = false;

                for (int i = 0; i < stepsToAdd.Count; ++i)
                {
                    // Make sure we don't duplicate the step in the list
                    if (!Utils.GetStepInfo(stepsToAdd[i], Steps, out tempList, out tempPosition))
                    {
                        parentList.Insert(++previousPosition, stepsToAdd[i]);
                        stepAdded = true;
                    }
                }

                if (stepAdded && WorkoutChanged != null)
                {
                    WorkoutChanged(this, new PropertyChangedEventArgs("Steps"));
                }
            }
            else
            {
                // We haven't found the right step, this shouldn't happen
                Trace.Assert(false);
            }
        }

        public void InsertStepsBeforeStep(IStep nextStep, List<IStep> stepsToAdd)
        {
            Trace.Assert(nextStep != null);

            UInt16 previousPosition = 0;
            List<IStep> parentList = null;

            if (Utils.GetStepInfo(nextStep, Steps, out parentList, out previousPosition))
            {
                List<IStep> tempList;
                UInt16 tempPosition = 0;
                bool stepAdded = false;

                for (int i = 0; i < stepsToAdd.Count; ++i)
                {
                    // Make sure we don't duplicate the step in the list
                    if (!Utils.GetStepInfo(stepsToAdd[i], Steps, out tempList, out tempPosition))
                    {
                        parentList.Insert(previousPosition++, stepsToAdd[i]);
                        stepAdded = true;
                    }
                }

                if (stepAdded && WorkoutChanged != null)
                {
                    WorkoutChanged(this, new PropertyChangedEventArgs("Steps"));
                }
            }
            else
            {
                // We haven't found the right step, this shouldn't happen
                Trace.Assert(false);
            }
        }

        public void RemoveStep(IStep stepToRemove)
        {
            List<IStep> stepsToRemove = new List<IStep>(); ;

            stepsToRemove.Add(stepToRemove);

            RemoveSteps(stepsToRemove);
        }

        public void RemoveSteps(List<IStep> stepsToRemove)
        {
            List<IStep> selectedList = null;
            UInt16 selectedPosition = 0;
            bool stepRemoved = false;

            for(int i = 0; i < stepsToRemove.Count; ++i)
            {
                if (Utils.GetStepInfo(stepsToRemove[i], Steps, out selectedList, out selectedPosition))
                {
                    selectedList.RemoveAt(selectedPosition);
                    stepRemoved = true;
                }
            }

            if(stepRemoved)
            {
                CleanUpAfterDelete();

                if (WorkoutChanged != null)
                {
                    WorkoutChanged(this, new PropertyChangedEventArgs("Steps"));
                }
            }
        }

        private void CleanUpAfterDelete()
        {
            if (Steps.Count > 0)
            {
                // Go through repeat steps and delete the ones which have 0 substeps
                for (int i = 0; i < Steps.Count; ++i)
                {
                    IStep currentStep = Steps[i];

                    if (currentStep.Type == IStep.StepType.Repeat)
                    {
                        RepeatStep concreteStep = (RepeatStep)currentStep;

                        if (concreteStep.StepsToRepeat.Count > 0)
                        {
                            CleanUpAfterDelete(concreteStep);
                        }

                        if (concreteStep.StepsToRepeat.Count == 0)
                        {
                            Steps.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }

            if (Steps.Count == 0)
            {
                // Cannot have an empty workout, recreate a base step
                Steps.Add(new RegularStep(this));
            }
        }

        private void CleanUpAfterDelete(RepeatStep step)
        {
            // Go through repeat steps and delete the ones which have 0 substeps
            for (int i = 0; i < step.StepsToRepeat.Count; ++i)
            {
                IStep currentStep = step.StepsToRepeat[i];

                if (currentStep.Type == IStep.StepType.Repeat)
                {
                    RepeatStep concreteStep = (RepeatStep)currentStep;

                    if (concreteStep.StepsToRepeat.Count == 0)
                    {
                        step.StepsToRepeat.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        CleanUpAfterDelete(concreteStep);
                    }
                }
            }
        }

        public void ScheduleWorkout(DateTime date)
        {
            if(!m_ScheduledDates.Contains(date))
            {
                m_ScheduledDates.Add(date);

                if (WorkoutChanged != null)
                {
                    WorkoutChanged(this, new PropertyChangedEventArgs("Schedule"));
                }
            }
        }

        public void RemoveScheduledDate(DateTime date)
        {
            if (m_ScheduledDates.Contains(date))
            {
                m_ScheduledDates.Remove(date);

                if (WorkoutChanged != null)
                {
                    WorkoutChanged(this, new PropertyChangedEventArgs("Schedule"));
                }
            }
        }

        public DateTime LastExportDate
        {
            get { return m_LastExportDate; }
            set
            {
                if (m_LastExportDate != value)
                {
                    m_LastExportDate = value;

                    if (WorkoutChanged != null)
                    {
                        WorkoutChanged(this, new PropertyChangedEventArgs("ExportDate"));
                    }
                }
            }
        }

        public List<DateTime> ScheduledDates
        {
            get { return m_ScheduledDates; }
        }

        public List<IStep> Steps
        {
            get { return m_Steps; }
        }

        public IActivityCategory Category
        {
            get { return m_Category; }
            set
            {
                if (m_Category != value)
                {
                    Trace.Assert(value != null);
                    m_Category = value;

                    if (WorkoutChanged != null)
                    {
                        WorkoutChanged(this, new PropertyChangedEventArgs("Category"));
                    }
                }
            }
        }

        public string Name
        {
            get { return m_Name; }
            set
            {
                if (m_Name != value)
                {
                    Trace.Assert(value.Length > 0 && value.Length <= 15);
                    m_Name = value;

                    if (WorkoutChanged != null)
                    {
                        WorkoutChanged(this, new PropertyChangedEventArgs("Name"));
                    }
                }
            }
        }

        public string Notes
        {
            get { return m_Notes; }
            set
            {
                if (m_Notes != value)
                {
                    Trace.Assert(value.Length <= 30000);
                    m_Notes = value;

                    if (WorkoutChanged != null)
                    {
                        WorkoutChanged(this, new PropertyChangedEventArgs("Notes"));
                    }
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
            set { Trace.Assert(false); }
        }

        public delegate void WorkoutChangedEventHandler(Workout modifiedWorkout, PropertyChangedEventArgs changedProperty);
        public event WorkoutChangedEventHandler WorkoutChanged;

        private DateTime m_LastExportDate = new DateTime(0);
        private List<DateTime> m_ScheduledDates = new List<DateTime>();
        private List<IStep> m_Steps = new List<IStep>(Constants.MaxStepsPerWorkout);
        private List<XmlNode> m_STExtensions = new List<XmlNode>();
        private List<XmlNode> m_StepsExtensions = new List<XmlNode>();
        private IActivityCategory m_Category;
        private string m_Name;
        private string m_Notes;
    }
}
