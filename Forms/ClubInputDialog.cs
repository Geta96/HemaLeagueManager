using System.Drawing;
using System.Windows.Forms;
using HemaLeagueManager.Models;

namespace HemaLeagueManager.Forms
{
    public class ClubInputDialog : Form
    {
        public Club Result { get; private set; } = new Club();

        private TextBox _nameBox = null!;
        private TextBox _shortBox = null!;
        private TextBox _cityBox = null!;

        public ClubInputDialog(Club? existing = null)
        {
            Text = existing == null ? "New Club" : "Edit Club";
            Size = new Size(420, 260);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = UiTheme.Background;
            ForeColor = UiTheme.TextPrimary;
            Font = UiTheme.Body;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;

            Controls.Add(new Label { Text = "Full name:", Left = 16, Top = 18, Width = 90 });
            _nameBox = new TextBox { Left = 120, Top = 15, Width = 260 };
            Controls.Add(_nameBox);

            Controls.Add(new Label { Text = "Short name:", Left = 16, Top = 56, Width = 90 });
            _shortBox = new TextBox { Left = 120, Top = 53, Width = 110, MaxLength = 10 };
            Controls.Add(_shortBox);
            Controls.Add(new Label
            {
                Text = "(max 10 chars)",
                Left = 240, Top = 56, Width = 140,
                ForeColor = UiTheme.TextMuted,
                Font = UiTheme.Small
            });

            Controls.Add(new Label { Text = "City:", Left = 16, Top = 94, Width = 90 });
            _cityBox = new TextBox { Left = 120, Top = 91, Width = 260 };
            Controls.Add(_cityBox);

            var ok = new FlatButton { Text = "OK", Left = 210, Top = 170, Width = 80, IsPrimary = true, DialogResult = DialogResult.OK };
            ok.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_nameBox.Text))
                {
                    MessageBox.Show("Full club name is required.");
                    DialogResult = DialogResult.None;
                    return;
                }
                if (_shortBox.Text.Trim().Length > 10)
                {
                    MessageBox.Show("Short name must be at most 10 characters.");
                    DialogResult = DialogResult.None;
                    return;
                }
                Result = new Club
                {
                    Name = _nameBox.Text.Trim(),
                    ShortName = _shortBox.Text.Trim(),
                    City = _cityBox.Text.Trim()
                };
            };
            var cancel = new FlatButton { Text = "Cancel", Left = 300, Top = 170, Width = 80, DialogResult = DialogResult.Cancel };
            Controls.Add(ok);
            Controls.Add(cancel);

            AcceptButton = ok;
            CancelButton = cancel;

            if (existing != null)
            {
                _nameBox.Text = existing.Name;
                _shortBox.Text = existing.ShortName;
                _cityBox.Text = existing.City;
            }
        }
    }
}