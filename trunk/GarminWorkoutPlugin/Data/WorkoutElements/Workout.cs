using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminWorkoutPlugin.Controller;

namespace GarminWorkoutPlugin.Data
{
    [Serializable()]

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
            Category = Utils.FindCategoryByID(Encoding.UTF8.GetString(stringBuffer));

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
            Category = Utils.FindCategoryByID(Encoding.UTF8.GetString(stringBuffer));

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
            Category = Utils.FindCategoryByID(Encoding.UTF8.GetString(stringBuffer));

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
            attribute.Value = "Other";
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

                            category = Utils.FindCategoryByID(categoryNode.Value);
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
                string finalName = WorkoutManager.Instance.GetUniqueName(workoutName);

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

                            concreteStep.Target = TargetFactory.Create(ITarget.TargetType.Power, childNode, step);
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

                        Category = Utils.FindCategoryByID(categoryNode.Value);
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
            return GetStepByIdInternal(Steps, id, 1);
        }

        private static IStep GetStepByIdInternal(IList<IStep> steps, int id, int baseId)
        {
            int currentId = baseId;

            for (int i = 0; i < steps.Count; ++i)
            {
                IStep currentStep = steps[i];

                if (currentId == id)
                {
                    return currentStep;
                }
                else if (currentStep.Type == IStep.StepType.Repeat && id < currentId + currentStep.GetStepCount())
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

        public DateTime LastExportDate
        {
            get { return m_LastExportDate; }
            set { m_LastExportDate = value; }
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
                Trace.Assert(value != null);
                m_Category = value;
            }
        }

        public string Name
        {
            get { return m_Name; }
            set
            {
                Trace.Assert(value.Length > 0 && value.Length <= 15);
                m_Name = value;
            }
        }

        public string Notes
        {
            get { return m_Notes; }
            set
            {
                Trace.Assert(value.Length <= 30000);
                m_Notes = value;
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

        private DateTime m_LastExportDate = DateTime.Now;
        private List<DateTime> m_ScheduledDates = new List<DateTime>();
        private List<IStep> m_Steps = new List<IStep>();
        private List<XmlNode> m_STExtensions = new List<XmlNode>();
        private List<XmlNode> m_StepsExtensions = new List<XmlNode>();
        private IActivityCategory m_Category;
        private string m_Name;
        private string m_Notes;
    }
}
