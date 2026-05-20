using System.Drawing;
using System.Windows.Forms;

namespace HemaLeagueManager.Forms
{
    /// <summary>
    /// Borderless flat button with smooth hover and an optional accent style.
    /// </summary>
    public class FlatButton : Button
    {
        private Color _idle = UiTheme.ButtonIdle;
        private Color _hover = UiTheme.ButtonHover;
        private bool _isPrimary;

        public FlatButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = Color.Empty; // we paint manually
            BackColor = _idle;
            ForeColor = UiTheme.TextPrimary;
            Font = UiTheme.BodyBold;
            Height = 36;
            Cursor = Cursors.Hand;
            TextAlign = ContentAlignment.MiddleCenter;
            Padding = new Padding(14, 0, 14, 0);
            UseCompatibleTextRendering = false;
        }

        public bool IsPrimary
        {
            get => _isPrimary;
            set
            {
                _isPrimary = value;
                if (value)
                {
                    _idle  = UiTheme.Accent;
                    _hover = UiTheme.AccentHover;
                    ForeColor = UiTheme.Background;
                }
                else
                {
                    _idle  = UiTheme.ButtonIdle;
                    _hover = UiTheme.ButtonHover;
                    ForeColor = UiTheme.TextPrimary;
                }
                BackColor = _idle;
            }
        }

        public void SetColors(Color idle, Color hover, Color? fore = null)
        {
            _idle = idle;
            _hover = hover;
            BackColor = _idle;
            if (fore.HasValue) ForeColor = fore.Value;
        }

        protected override void OnMouseEnter(System.EventArgs e)
        {
            base.OnMouseEnter(e);
            BackColor = _hover;
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            BackColor = _idle;
        }
    }
}