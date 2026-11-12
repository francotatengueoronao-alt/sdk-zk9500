using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace VisitControl
{
    public class PersonForm : Form
    {
        private readonly XmlStore _store;
        private readonly Visitor _original;
        private readonly TextBox _dni, _first, _last, _role, _reason, _photo;
        private readonly CheckBox _denied;
        private readonly DateTimePicker _from, _until;
        private readonly PictureBox _preview;

        public PersonForm(XmlStore store, Visitor visitor)
        {
            _store = store; _original = visitor;
            Text = visitor == null ? "Enrolar visitante" : "Editar visitante";
            Width = 520; Height = 510; StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog;
            Label title = new Label { Text = "La mayoría de registros puede conservar la función Visitante.", Left = 15, Top = 10, Width = 460 };
            Controls.Add(title);
            _dni = AddText("DNI", 45, visitor == null ? "" : visitor.Dni, 270);
            _first = AddText("Nombre", 85, visitor == null ? "" : visitor.FirstName, 270);
            _last = AddText("Apellido", 125, visitor == null ? "" : visitor.LastName, 270);
            _role = AddText("Función / cargo", 165, visitor == null ? "Visitante" : visitor.Role, 270);
            _photo = AddText("Foto", 205, visitor == null ? "" : visitor.PhotoPath, 270);
            Button browse = new Button { Text = "Elegir foto...", Left = 390, Top = 203, Width = 95 };
            browse.Click += delegate { BrowsePhoto(); }; Controls.Add(browse);
            _preview = new PictureBox { Left = 390, Top = 245, Width = 95, Height = 105, SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(_preview); LoadPreview();
            _denied = new CheckBox { Text = "Restringir ingreso durante el período indicado", Left = 15, Top = 245, Width = 340, Checked = visitor != null && visitor.IsDenied };
            Controls.Add(_denied);
            AddLabel("Desde", 285); _from = AddDate(285, visitor == null ? DateTime.Today : (visitor.DenialFrom ?? DateTime.Today));
            AddLabel("Vencimiento", 325); _until = AddDate(325, visitor == null ? DateTime.Today : (visitor.DenialUntil ?? DateTime.Today));
            AddLabel("Motivo", 365); _reason = new TextBox { Left = 120, Top = 360, Width = 365, Text = visitor == null ? "" : visitor.DenialReason, Multiline = true, Height = 48 };
            Controls.Add(_reason);
            Button save = new Button { Text = "Guardar", Left = 300, Top = 425, Width = 90, DialogResult = DialogResult.OK };
            Button cancel = new Button { Text = "Cancelar", Left = 395, Top = 425, Width = 90, DialogResult = DialogResult.Cancel };
            save.Click += delegate { SaveVisitor(); }; Controls.Add(save); Controls.Add(cancel); AcceptButton = save; CancelButton = cancel;
            if (visitor != null) _dni.ReadOnly = true;
        }

        private TextBox AddText(string label, int top, string value, int width)
        {
            Controls.Add(new Label { Text = label, Left = 15, Top = top + 4, Width = 100 });
            TextBox box = new TextBox { Left = 120, Top = top, Width = width, Text = value ?? String.Empty };
            Controls.Add(box); return box;
        }
        private void AddLabel(string text, int top) { Controls.Add(new Label { Text = text, Left = 15, Top = top + 4, Width = 100 }); }
        private DateTimePicker AddDate(int top, DateTime value) { DateTimePicker picker = new DateTimePicker { Left = 120, Top = top, Width = 220, Format = DateTimePickerFormat.Short, Value = value }; Controls.Add(picker); return picker; }
        private void BrowsePhoto()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp|Todos los archivos|*.*";
                if (dialog.ShowDialog(this) == DialogResult.OK) { _photo.Text = dialog.FileName; LoadPreview(); }
            }
        }
        private void LoadPreview()
        {
            try { if (File.Exists(_photo.Text)) using (Image image = Image.FromFile(_photo.Text)) _preview.Image = new Bitmap(image); }
            catch { _preview.Image = null; }
        }
        private void SaveVisitor()
        {
            if (String.IsNullOrWhiteSpace(_dni.Text) || String.IsNullOrWhiteSpace(_first.Text) || String.IsNullOrWhiteSpace(_last.Text))
            {
                MessageBox.Show("DNI, nombre y apellido son obligatorios."); DialogResult = DialogResult.None; return;
            }
            Visitor visitor = _original ?? new Visitor();
            visitor.Dni = _dni.Text.Trim(); visitor.FirstName = _first.Text.Trim(); visitor.LastName = _last.Text.Trim();
            visitor.Role = String.IsNullOrWhiteSpace(_role.Text) ? "Visitante" : _role.Text.Trim();
            visitor.IsDenied = _denied.Checked; visitor.DenialReason = _reason.Text.Trim();
            visitor.DenialFrom = _denied.Checked ? (DateTime?)_from.Value.Date : null;
            visitor.DenialUntil = _denied.Checked ? (DateTime?)_until.Value.Date : null;
            visitor.Active = true;
            visitor.PhotoPath = _store.CopyPhoto(_photo.Text, visitor.Dni);
            if (String.IsNullOrEmpty(visitor.PhotoPath) && File.Exists(_photo.Text)) visitor.PhotoPath = _photo.Text;
            if (_original == null) _store.Data.Visitors.Add(visitor);
            _store.Save(); DialogResult = DialogResult.OK; Close();
        }
    }
}
