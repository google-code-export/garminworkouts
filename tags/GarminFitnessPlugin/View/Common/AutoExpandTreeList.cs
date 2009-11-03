using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    class AutoExpandTreeList : ExtendedTreeList
    {
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            // Do nothing so the user can't collapse
        }

        public new void SetExpanded(object element, bool bExpanded, bool bExpandChildren)
        {
            // Ignore boolean parameters, force expanded
            base.SetExpanded(element, true, true);
        }

        public new object RowData
        {
            get { return base.RowData; }
            set
            {
                base.RowData = value;
                SetExpanded(RowData, true, true);
            }
        }
    }
}
