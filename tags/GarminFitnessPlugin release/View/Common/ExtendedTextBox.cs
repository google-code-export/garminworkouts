using System;
using System.ComponentModel;
using System.Media;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    public partial class ExtendedTextBox : ZoneFiveSoftware.Common.Visuals.TextBox
    {
        public ExtendedTextBox()
        {
            InitializeComponent();

            KeyDown += new KeyEventHandler(OnKeyDown);
            base.TextChanged += new EventHandler(OnTextChanged);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!Multiline && e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            if (TextChanged != null)
            {
                TextChanged(this, e);
            }
        }

        public void Cut()
        {
            Focus();
            SendKeys.Send("^X");
        }

        public void Copy()
        {
            Focus();
            SendKeys.Send("^C");
        }

        public void Paste()
        {
            Focus();
            SendKeys.Send("^V");
        }

        [Category("Action")]
        [Browsable(true)]
        public new event EventHandler TextChanged;
    }
}

