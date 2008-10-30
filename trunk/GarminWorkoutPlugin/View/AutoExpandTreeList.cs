using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    class AutoExpandTreeList : TreeList
    {
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            // Do nothing so the user can't collapse
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (PaintEnabled)
            {
                base.OnPaint(e);
            }
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

        [DesignerSerializationVisibility(0)]
        [Browsable(false)]
        public bool PaintEnabled
        {
            get { return m_PaintDisableCount == 0; }
            set
            {
                if (value == true)
                {
                    Trace.Assert(m_PaintDisableCount > 0);
                    m_PaintDisableCount--;

                    if (m_PaintDisableCount == 0)
                    {
                        Invalidate();
                    }
                }
                else
                {
                    m_PaintDisableCount++;
                }
            }
        }

        private int m_PaintDisableCount = 0;
    }
}
