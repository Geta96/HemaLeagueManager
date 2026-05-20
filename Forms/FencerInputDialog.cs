using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HemaLeagueManager.Models;
using HemaLeagueManager.Services;

namespace HemaLeagueManager.Forms
{
    public class FencerInputDialog : Form
    {
        public Fencer Result { get; private set; } = new Fencer();

        private TextBox _nameBox = null!;
        private ComboBox _sexBox = null!;
        private ComboBox _clubBox = null!;

        public FencerInputDialog(Fencer? existing = null)
        {
            Text = existing == null ? "New Fencer" : "Edit Fencer";
            Size = new Size(400, 260);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = UiTheme.Background;
            ForeColor = UiTheme.TextPrimary;
            Font = UiTheme.Body;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;

            Controls.Add(new Label { Text = "Name:", Left = 16, Top = 18, Width = 80 });
            _nameBox = new TextBox { Left = 110, Top = 15, Width = 250 };
            Controls.Add(_nameBox);

            Controls.Add(new Label { Text = "Sex:", Left = 16, Top = 56, Width = 80 });
            _sexBox = new ComboBox
            {
                Left = 110, Top = 53, Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _sexBox.Items.AddRange(new object[] { "Male", "Female", "Other" });
            Controls.Add(_sexBox);

            Controls.Add(new Label { Text = "Club:", Left = 16, Top = 94, Width = 80 });
            _clubBox = new ComboBox
            {
                Left = 110, Top = 91, Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            ReloadClubs();
            Controls.Add(_clubBox);

            var btnNewClub = new FlatButton { Text = "+ New", Left = 295, Top = 90, Width = 65, Height = 26 };
            btnNewClub.Click += (s, e) => AddNewClub();
            Controls.Add(btnNewClub);

            var ok = new FlatButton { Text = "OK", Left = 190, Top = 170, Width = 80, IsPrimary = true, DialogResult = DialogResult.OK };
            ok.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_nameBox.Text) || _sexBox.SelectedItem == null || _clubBox.SelectedItem == null)
                {
                    MessageBox.Show("Name, sex and club are required.");
                    DialogResult = DialogResult.None;
                    return;
                }
                Result = new Fencer
                {
                    Name = _nameBox.Text.Trim(),
                    Sex = _sexBox.SelectedItem!.ToString()!,
                    ClubName = _clubBox.SelectedItem!.ToString()!
                };
            };
            var cancel = new FlatButton { Text = "Cancel", Left = 280, Top = 170, Width = 80, DialogResult = DialogResult.Cancel };
            Controls.Add(ok);
            Controls.Add(cancel);

            AcceptButton = ok;
            CancelButton = cancel;

            if (existing != null)
            {
                _nameBox.Text = existing.Name;
                _sexBox.SelectedItem = existing.Sex;
                if (!string.IsNullOrWhiteSpace(existing.ClubName) && _clubBox.Items.Contains(existing.ClubName))
                    _clubBox.SelectedItem = existing.ClubName;
            }
        }

        private void ReloadClubs()
        {
            var selected = _clubBox.SelectedItem?.ToString();
            _clubBox.Items.Clear();
            foreach (var name in ClubRegistry.Names)
                _clubBox.Items.Add(name);
            if (selected != null && _clubBox.Items.Contains(selected))
                _clubBox.SelectedItem = selected;
        }

        private void AddNewClub()
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox("Club name:", "New Club", "");
            if (string.IsNullOrWhiteSpace(name)) return;
            var city = Microsoft.VisualBasic.Interaction.InputBox("City (optional):", "New Club", "");
            var club = ClubRegistry.AddIfMissing(name.Trim(), city.Trim());
            ClubRegistry.Save();
            ReloadClubs();
            _clubBox.SelectedItem = club.Name;
        }
    }
}