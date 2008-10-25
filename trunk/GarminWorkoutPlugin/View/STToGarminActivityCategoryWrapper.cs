using System;
using System.Reflection;
using System.Resources;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Visuals;
using GarminWorkoutPlugin.Controller;
using GarminWorkoutPlugin.Data;

namespace GarminWorkoutPlugin.View
{
    class STToGarminActivityCategoryWrapper : TreeList.TreeListNode
    {
        public STToGarminActivityCategoryWrapper(STToGarminActivityCategoryWrapper parent, IActivityCategory element)
            : base(parent, element)
        {
        }


        public String Name
        {
            get { return ((IActivityCategory)Element).Name; }
        }

        public String GarminCategory
        {
            get
            {
                GarminCategories garminCategory = Options.GetGarminCategory((IActivityCategory)Element);
                FieldInfo fieldInfo = garminCategory.GetType().GetField(Enum.GetName(garminCategory.GetType(), garminCategory));
                GarminCategoryStringProviderAttribute providerAttribute = (GarminCategoryStringProviderAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(GarminCategoryStringProviderAttribute));

                return m_ResourceManager.GetString(providerAttribute.StringName, GarminWorkoutView.UICulture);
            }
        }

        private ResourceManager m_ResourceManager = new ResourceManager("GarminWorkoutPlugin.Resources.StringResources",
                                                                Assembly.GetExecutingAssembly());
    }
}
