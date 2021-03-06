using System;

namespace GarminFitnessPlugin.Controller
{
    class ComboBoxStringProviderAttribute : Attribute
    {
        public ComboBoxStringProviderAttribute(string name)
        {
            m_StringName = name;
		}

        protected string m_StringName;

        public string StringName
        {
            get { return m_StringName; }
            set { m_StringName = value; }
		}

    }
}
