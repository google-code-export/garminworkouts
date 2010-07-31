using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Diagnostics;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class WorkoutStepsList : IPluginSerializable, IXMLSerializable, IEnumerable<IStep>
    {
        public WorkoutStepsList(IWorkout parentWorkout)
        {
            m_ParentWorkout = parentWorkout;
        }

#region IPluginSerializable members

        public override void Serialize(Stream stream)
        {
            // Steps
            stream.Write(BitConverter.GetBytes(m_InternalStepList.Count), 0, sizeof(Int32));
            foreach (IStep currentStep in m_InternalStepList)
            {
                currentStep.Serialize(stream);
            }
        }

#endregion

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            Int32 stepCount;

            stream.Read(intBuffer, 0, sizeof(Int32));
            stepCount = BitConverter.ToInt32(intBuffer, 0);
            m_InternalStepList.Clear();
            for (int i = 0; i < stepCount; ++i)
            {
                IStep.StepType type;

                stream.Read(intBuffer, 0, sizeof(Int32));
                type = (IStep.StepType)BitConverter.ToInt32(intBuffer, 0);

                if (type == IStep.StepType.Regular)
                {
                    AddStepToRoot(new RegularStep(stream, version, m_ParentWorkout.ConcreteWorkout));
                }
                else if (type == IStep.StepType.Repeat)
                {
                    AddStepToRoot(new RepeatStep(stream, version, m_ParentWorkout.ConcreteWorkout));
                }
                else
                {
                    AddStepToRoot(new WorkoutLinkStep(stream, version, m_ParentWorkout.ConcreteWorkout));
                }
            }
        }

#region IXMLSerializable Members

        public void Serialize(XmlNode parentNode, string nodeName, XmlDocument document)
        {
            XmlNode childNode;

            foreach (IStep currentStep in m_InternalStepList)
            {
                childNode = document.CreateElement("Step");

                currentStep.Serialize(childNode, "Step", document);
                parentNode.AppendChild(childNode);
            }
        }

        public void Deserialize(XmlNode parentNode)
        {
            List<IStep> steps = new List<IStep>();
            XmlNode StepsExtensionsNode = null;
            XmlNode STExtensionsNode = null;

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode child = parentNode.ChildNodes.Item(i);

                if (child.Name == "Step")
                {
                    if (child.Attributes.Count == 1 && child.Attributes[0].Name == Constants.XsiTypeTCXString)
                    {
                        string stepTypeString = child.Attributes[0].Value;
                        IStep newStep = null;

                        if (stepTypeString == Constants.StepTypeTCXString[(int)IStep.StepType.Regular])
                        {
                            newStep = new RegularStep(m_ParentWorkout.ConcreteWorkout);
                        }
                        else if (stepTypeString == Constants.StepTypeTCXString[(int)IStep.StepType.Repeat])
                        {
                            newStep = new RepeatStep(m_ParentWorkout.ConcreteWorkout);
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
                else if (child.Name == Constants.ExtensionsTCXString)
                {
                    for (int j = 0; j < child.ChildNodes.Count; ++j)
                    {
                        XmlNode currentNode = child.ChildNodes[j];

                        if (currentNode.Name == "Steps")
                        {
                            StepsExtensionsNode = currentNode;
                        }
                        else if (currentNode.Name == "SportTracksExtensions")
                        {
                            STExtensionsNode = currentNode;
                        }
                    }
                }
            }

            if (steps.Count < 1)
            {
                throw new GarminFitnessXmlDeserializationException("Information missing in the XML node", parentNode);
            }
            else if (steps.Count > 20)
            {
                throw new GarminFitnessXmlDeserializationException("Too many steps in the XML node", parentNode);
            }

            m_InternalStepList.Clear();

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

#endregion

#region IEnumerable<IStep> Members

        public IEnumerator<IStep> GetEnumerator()
        {
            return m_InternalStepList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_InternalStepList.GetEnumerator();
        }

#endregion

#region List<IStep> encapsulation members

        public IStep this[int index]
        {
            get { return m_InternalStepList[index]; }
            set { m_InternalStepList[index] = value; }
        }

        public int Count
        {
            get { return m_InternalStepList.Count; }
        }

        public void Add(IStep newStep)
        {
            m_InternalStepList.Add(newStep);
        }

        public void Clear()
        {
            m_InternalStepList.Clear();
        }

#endregion

        public UInt32 Serialize(GarXFaceNet._Workout workout, UInt32 stepIndex)
        {
            UInt32 internalStepIndex = stepIndex;

            foreach (IStep step in m_InternalStepList)
            {
                internalStepIndex = step.Serialize(workout, internalStepIndex);
            }

            return internalStepIndex;
        }

        public void Deserialize(GarXFaceNet._Workout workout)
        {
            for (UInt32 i = 0; i < workout.GetNumValidSteps(); ++i)
            {
                GarXFaceNet._Workout._Step step = workout.GetStep(i);
                IStep newStep;

                if (step.GetDurationType() == GarXFaceNet._Workout._Step.DurationTypes.Repeat)
                {
                    newStep = new RepeatStep(m_ParentWorkout.ConcreteWorkout);
                }
                else
                {
                    newStep = new RegularStep(m_ParentWorkout.ConcreteWorkout);
                }

                newStep.Deserialize(workout, i);
                AddStepToRoot(newStep);
            }
        }

        private void HandleSTExtension(XmlNode extensionsNode)
        {
            foreach (XmlNode currentExtension in extensionsNode.ChildNodes)
            {
                // TODO
                if (currentExtension.Name == "WorkoutLink")
                {
                }
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

        public IStep GetStepById(int id)
        {
            return GetStepByIdInternal(m_InternalStepList, id, 0);
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

        public bool AddStepToRoot(IStep stepToAdd)
        {
            List<IStep> newStep = new List<IStep>();

            newStep.Add(stepToAdd);

            return AddStepsToRoot(newStep);
        }

        public bool AddStepsToRoot(List<IStep> stepsToAdd)
        {
            if (m_InternalStepList.Count > 0)
            {
                return InsertStepsAfterStep(stepsToAdd, m_InternalStepList[m_InternalStepList.Count - 1]);
            }
            else
            {
                return InsertStepsAfterStep(stepsToAdd, null);
            }
        }

        public bool InsertStepInRoot(int index, IStep stepToAdd)
        {
            List<IStep> stepsToAdd = new List<IStep>();

            stepsToAdd.Add(stepToAdd);

            return InsertStepsBeforeStep(stepsToAdd, m_InternalStepList[index]);
        }

        public bool InsertStepsInRoot(List<IStep> stepsToAdd, int index)
        {
            return InsertStepsBeforeStep(stepsToAdd, m_InternalStepList[index]);
        }

        public bool InsertStepAfterStep(IStep stepToAdd, IStep previousStep)
        {
            List<IStep> stepsToAdd = new List<IStep>();

            stepsToAdd.Add(stepToAdd);

            return InsertStepsAfterStep(stepsToAdd, previousStep);
        }

        public bool InsertStepsAfterStep(List<IStep> stepsToAdd, IStep previousStep)
        {
            int insertStepsCount = 0;

            // We need to count the number of items being moved
            for (int i = 0; i < stepsToAdd.Count; ++i)
            {
                insertStepsCount += stepsToAdd[i].GetStepCount();
            }

            if (m_ParentWorkout.CanAcceptNewStep(insertStepsCount, previousStep))
            {
                return InsertStepsAfterStepInternal(stepsToAdd, previousStep);
            }

            return false;
        }

        public bool InsertStepBeforeStep(IStep stepToAdd, IStep previousStep)
        {
            List<IStep> stepsToAdd = new List<IStep>();

            stepsToAdd.Add(stepToAdd);

            return InsertStepsBeforeStep(stepsToAdd, previousStep);
        }

        public bool InsertStepsBeforeStep(List<IStep> stepsToAdd, IStep nextStep)
        {
            Debug.Assert(nextStep != null);

            int insertStepsCount = 0;

            // We need to count the number of items being moved
            for (int i = 0; i < stepsToAdd.Count; ++i)
            {
                insertStepsCount += stepsToAdd[i].GetStepCount();
            }

            if (m_ParentWorkout.CanAcceptNewStep(insertStepsCount, nextStep))
            {
                return InsertStepsBeforeStepInternal(stepsToAdd, nextStep);
            }

            return false;
        }

        private bool InsertStepAfterStepInternal(IStep stepToAdd, IStep previousStep)
        {
            List<IStep> stepsToAdd = new List<IStep>();

            stepsToAdd.Add(stepToAdd);

            return InsertStepsAfterStepInternal(stepsToAdd, previousStep);
        }

        private bool InsertStepsAfterStepInternal(List<IStep> stepsToAdd, IStep previousStep)
        {
            UInt16 previousPosition = 0;
            UInt16 preAddSplitCount = m_ParentWorkout.ConcreteWorkout.GetSplitPartsCount();
            List<IStep> parentList = null;
            List<IStep> tempList;
            UInt16 tempPosition = 0;
            bool stepAdded = false;
            bool addToEnd = false;

            if (!Utils.GetStepInfo(previousStep, m_ParentWorkout.ConcreteWorkout.Steps, out parentList, out previousPosition))
            {
                addToEnd = true;
                parentList = m_ParentWorkout.ConcreteWorkout.Steps;
            }

            for (int i = 0; i < stepsToAdd.Count; ++i)
            {
                IStep currentStep = stepsToAdd[i];

                // Make sure we don't duplicate the step in the list
                Debug.Assert(!Utils.GetStepInfo(currentStep, m_ParentWorkout.ConcreteWorkout.Steps, out tempList, out tempPosition));

                TriggerStepAdded(currentStep);

                if (addToEnd)
                {
                    parentList.Add(currentStep);
                }
                else
                {
                    parentList.Insert(++previousPosition, currentStep);
                }
                stepAdded = true;
            }

            if (stepAdded)
            {
                TriggerListChanged("Steps");
            }

            return true;
        }

        private bool InsertStepBeforeStepInternal(IStep stepToAdd, IStep previousStep)
        {
            List<IStep> stepsToAdd = new List<IStep>();

            stepsToAdd.Add(stepToAdd);

            return InsertStepsBeforeStepInternal(stepsToAdd, previousStep);
        }

        private bool InsertStepsBeforeStepInternal(List<IStep> stepsToAdd, IStep nextStep)
        {
            UInt16 previousPosition = 0;
            UInt16 preAddSplitCount = m_ParentWorkout.ConcreteWorkout.GetSplitPartsCount();
            List<IStep> parentList = null;

            if (Utils.GetStepInfo(nextStep, m_ParentWorkout.ConcreteWorkout.Steps, out parentList, out previousPosition))
            {
                List<IStep> tempList;
                UInt16 tempPosition = 0;
                bool stepAdded = false;

                for (int i = 0; i < stepsToAdd.Count; ++i)
                {
                    // Make sure we don't duplicate the step in the list
                    Debug.Assert(!Utils.GetStepInfo(stepsToAdd[i], m_ParentWorkout.ConcreteWorkout.Steps, out tempList, out tempPosition));

                    TriggerStepAdded(stepsToAdd[i]);

                    parentList.Insert(previousPosition++, stepsToAdd[i]);
                    stepAdded = true;
                }

                if (stepAdded)
                {
                    TriggerListChanged("Steps");
                }

                return true;
            }
            else
            {
                // We haven't found the right step, this shouldn't happen
                Debug.Assert(false);
            }

            return false;
        }

        public void MoveStepsAfterStep(List<IStep> stepsToMove, IStep previousStep)
        {
            if (stepsToMove != null && stepsToMove.Count > 0)
            {
                m_EventsActive = false;
                {
                    // Mark the destination
                    IStep positionMarker = new RegularStep(m_ParentWorkout.ConcreteWorkout);

                    // Mark position
                    InsertStepAfterStepInternal(positionMarker, previousStep);

                    // Remove items from their original position
                    RemoveSteps(stepsToMove);

                    // Add at new position
                    InsertStepsAfterStepInternal(stepsToMove, positionMarker);

                    // Remove our position marker
                    RemoveStep(positionMarker);

                    m_EventsActive = true;
                }

                TriggerListChanged("Steps");
            }
        }

        public void MoveStepsBeforeStep(List<IStep> stepsToMove, IStep nextStep)
        {
            if (stepsToMove != null && stepsToMove.Count > 0)
            {
                m_EventsActive = false;
                {
                    // Make a copy of the destination
                    IStep positionMarker = nextStep.Clone();

                    // Mark position
                    InsertStepBeforeStepInternal(positionMarker, nextStep);

                    // Remove items from their original position
                    RemoveSteps(stepsToMove);

                    // Add at new position
                    InsertStepsAfterStepInternal(stepsToMove, positionMarker);

                    // Remove our position marker
                    RemoveStep(positionMarker);

                    m_EventsActive = true;
                }

                // Trigger event
                TriggerListChanged("Steps");
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
            UInt16 preRemoveSplitCount = m_ParentWorkout.ConcreteWorkout.GetSplitPartsCount();

            if (stepsToRemove.Count > 0)
            {
                List<IStep> selectedList = null;
                UInt16 selectedPosition = 0;

                foreach (IStep step in stepsToRemove)
                {
                    if (Utils.GetStepInfo(step, m_ParentWorkout.ConcreteWorkout.Steps, out selectedList, out selectedPosition))
                    {
                        selectedList.RemoveAt(selectedPosition);

                        TriggerStepRemoved(step);
                    }
                }

                CleanUpAfterDelete();

                TriggerListChanged("Steps");

                if (preRemoveSplitCount != m_ParentWorkout.ConcreteWorkout.GetSplitPartsCount())
                {
                    TriggerListChanged("PartsCount");
                }
            }
        }

        private void CleanUpAfterDelete()
        {
            CleanUpAfterDelete(m_InternalStepList);
        }

        private void CleanUpAfterDelete(List<IStep> listToClean)
        {
            if (listToClean.Count > 0)
            {
                List<IStep> stepsToRemove = new List<IStep>();

                // Go through repeat steps and delete the ones which have 0 substeps
                foreach (IStep currentStep in listToClean)
                {
                    if (currentStep.Type == IStep.StepType.Repeat)
                    {
                        RepeatStep concreteStep = (RepeatStep)currentStep;

                        CleanUpAfterDelete(concreteStep.StepsToRepeat);

                        if (concreteStep.StepsToRepeat.Count == 0)
                        {
                            stepsToRemove.Add(currentStep);
                        }
                    }
                }

                // Remove the empty repeats
                foreach (IStep step in stepsToRemove)
                {
                    listToClean.Remove(step);

                    TriggerStepRemoved(step);
                }
            }

            if (m_ParentWorkout is Workout && m_InternalStepList.Count == 0)
            {
                // Cannot have an empty workout, recreate a base step
                AddStepToRoot(new RegularStep(m_ParentWorkout.ConcreteWorkout));
            }
        }

        private void TriggerStepAdded(IStep step)
        {
            if (m_EventsActive && StepAdded != null)
            {
                StepAdded(step);
            }
        }

        private void TriggerStepRemoved(IStep step)
        {
            if (m_EventsActive && StepRemoved != null)
            {
                StepRemoved(step);
            }
        }

        private void TriggerListChanged(string property)
        {
            if (m_EventsActive && ListChanged != null)
            {
                ListChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public List<IStep> List
        {
            get { return m_InternalStepList; }
        }

        public static implicit operator List<IStep>(WorkoutStepsList value)
        {
            return value.m_InternalStepList;
        }

        public delegate void StepAddedEventHandler(IStep addedStep);
        public event StepAddedEventHandler StepAdded;

        public delegate void StepRemovedEventHandler(IStep removedStep);
        public event StepRemovedEventHandler StepRemoved;

        public event PropertyChangedEventHandler ListChanged;

        private IWorkout m_ParentWorkout = null;
        private List<IStep> m_InternalStepList = new List<IStep>();
        private bool m_EventsActive = true;
    }
}
