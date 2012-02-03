using System.Collections;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    class ExtendedRowDataRenderer : TreeList.DefaultRowDataRenderer
    {
        public ExtendedRowDataRenderer(TreeList list)
            : base(list)
        {
        }

        // This method is protected, make it accessible
        public new IList GetRows()
        {
            return base.GetRows();
        }
    }
}
