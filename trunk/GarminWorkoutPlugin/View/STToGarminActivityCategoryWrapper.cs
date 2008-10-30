using System;
using System.Reflection;
using System.Resources;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
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

                return GarminFitnessView.ResourceManager.GetString(providerAttribute.StringName, GarminFitnessView.UICulture);
            }
        }
    }
}
