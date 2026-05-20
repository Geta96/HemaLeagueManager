using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HemaLeagueManager.Models;
using HemaLeagueManager.Services;

namespace HemaLeagueManager.Forms
{
    public class TournamentForm : Form
    {
        private readonly List<Fencer> _availableFencers;
        public Tournament Tournament { get; private set; }

        private TextBox _nameBox = null!;
        private DateTimePicker _datePicker = null!;
        private CheckBox _grandPrixBox = null!;
        private ListBox _availableList = null!;
        private ListBox _placementList = null!;

        public TournamentForm(List<Fencer> fencers, Tournament? existing = null)
        {
            _availableFencers = fencers;
            Tournament = existing ?? new Tournament();
            BuildUi();
            LoadData();
        }

        private void BuildUi()
        {
            Text = "Tournament";
            Size = new Size(640, 520);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(45, 35, 25);
            ForeColor = Color.Wheat;
            Font = new Font("Garamond", 10F);

            var lblName = new Label { Text = "Tournament name:", Left = 12, Top = 15, Width = 130 };
            _nameBox = new TextBox { Left = 150, Top = 12, Width = 250 };

            var lblDate = new Label { Text = "Date:", Left = 12, Top = 45, Width = 130 };
            _datePicker = new DateTimePicker { Left = 150, Top = 42, Width = 250 };

            _grandPrixBox = new CheckBox
            {
                Text = "★  Grand Prix  (points are doubled)",
                Left = 150, Top = 72, Width = 320, Height = 24,
                ForeColor = Color.Goldenrod,
                BackColor = Color.FromArgb(45, 35, 25),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Garamond", 10F, FontStyle.Bold)
            };

            var lblAvail = new Label { Text = "Available fencers", Left = 12, Top = 110, Width = 280 };
            _availableList = new ListBox { Left = 12, Top = 130, Width = 280, Height = 290 };

            var lblPlace = new Label { Text = "Placements (1st on top)", Left = 330, Top = 110, Width = 280 };
            _placementList = new ListBox { Left = 330, Top = 130, Width = 280, Height = 290 };

            var btnAdd = new Button { Text = "Add ->", Left = 12, Top = 425, Width = 80 };
            btnAdd.Click += (s, e) =>
            {
                if (_availableList.SelectedItem is Fencer f && !_placementList.Items.Contains(f))
                    _placementList.Items.Add(f);
            };

            var btnRemove = new Button { Text = "Remove", Left = 100, Top = 425, Width = 80 };
            btnRemove.Click += (s, e) =>
            {
                if (_placementList.SelectedIndex >= 0)
                    _placementList.Items.RemoveAt(_placementList.SelectedIndex);
            };

            var btnUp = new Button { Text = "Up", Left = 330, Top = 425, Width = 60 };
            btnUp.Click += (s, e) => MovePlacement(-1);

            var btnDown = new Button { Text = "Down", Left = 395, Top = 425, Width = 60 };
            btnDown.Click += (s, e) => MovePlacement(1);

            var btnOk = new Button { Text = "Save", Left = 440, Top = 450, Width = 80, DialogResult = DialogResult.OK };
            btnOk.Click += (s, e) => SaveAndClose();
            var btnCancel = new Button { Text = "Cancel", Left = 530, Top = 450, Width = 80, DialogResult = DialogResult.Cancel };

            Controls.AddRange(new Control[]
            {
                lblName, _nameBox, lblDate, _datePicker, _grandPrixBox,
                lblAvail, _availableList, lblPlace, _placementList,
                btnAdd, btnRemove, btnUp, btnDown, btnOk, btnCancel
            });

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private void MovePlacement(int delta)
        {
            int i = _placementList.SelectedIndex;
            int j = i + delta;
            if (i < 0 || j < 0 || j >= _placementList.Items.Count) return;
            (_placementList.Items[i], _placementList.Items[j]) = (_placementList.Items[j], _placementList.Items[i]);
            _placementList.SelectedIndex = j;
        }

        private void LoadData()
        {
            _nameBox.Text = Tournament.Name;
            _datePicker.Value = Tournament.Date == default ? DateTime.Today : Tournament.Date;
            _grandPrixBox.Checked = Tournament.IsGrandPrix;

            foreach (var f in _availableFencers)
                _availableList.Items.Add(f);

            foreach (var name in Tournament.Placements)
            {
                var f = _availableFencers.FirstOrDefault(x => x.Name == name);
                if (f != null) _placementList.Items.Add(f);
            }
        }

        private void SaveAndClose()
        {
            if (string.IsNullOrWhiteSpace(_nameBox.Text))
            {
                MessageBox.Show("Please enter a tournament name.");
                DialogResult = DialogResult.None;
                return;
            }

            Tournament.Name = _nameBox.Text.Trim();
            Tournament.Date = _datePicker.Value.Date;
            Tournament.IsGrandPrix = _grandPrixBox.Checked;
            Tournament.Placements = _placementList.Items.Cast<Fencer>().Select(f => f.Name).ToList();
        }
    }
}