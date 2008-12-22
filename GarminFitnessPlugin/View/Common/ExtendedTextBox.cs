using System;
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
            TextChanged += new EventHandler(OnTextChanged);

            m_MaxLength = UInt16.MaxValue;
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
            if (m_LastText != base.Text)
            {
                if (base.Text.Length <= m_MaxLength)
                {
                    m_LastText = base.Text;
                }
                else
                {
                    Text = m_LastText;
                    SendKeys.Send("{END}");
                    SystemSounds.Asterisk.Play();
                }
            }
        }

        public override string Text
        {
            get
            {
                return m_LastText;
            }
            set
            {
                base.Text = value;
            }
        }

        public UInt16 MaxLength
        {
            get { return m_MaxLength; }
            set { m_MaxLength = value; }
        }

        private String m_LastText;
        private UInt16 m_MaxLength;
    }
}

