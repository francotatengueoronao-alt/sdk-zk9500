using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VisitControl
{
    public class MainForm : Form
    {
        private readonly XmlStore _store;
        private readonly DataGridView _grid;
        private readonly TextBox _search;
        private readonly DateTimePicker _from;
        private readonly DateTimePicker _to;
        private readonly Label _readerStatus;

        public MainForm()
        {
            _store = new XmlStore();
            Text = "Control local de visitas";
            Width = 1050; Height = 650; StartPosition = FormStartPosition.CenterScreen;

            FlowLayoutPanel actions = new FlowLayoutPanel();
            actions.Dock = DockStyle.Top; actions.Height = 88; actions.Padding = new Padding(8);
            Button enroll = new Button(); enroll.Text = "Enrolar / editar visitante"; enroll.Width = 180; enroll.Height = 34;
            enroll.Click += delegate { EditVisitor(null); };
            Button edit = new Button(); edit.Text = "Editar seleccionado"; edit.Width = 150; edit.Height = 34;
            edit.Click += delegate { EditSelected(); };
            Button mark = new Button(); mark.Text = "Registrar por DNI"; mark.Width = 140; mark.Height = 34;
            mark.Click += delegate { RegisterByDni(); };
            Button export = new Button(); export.Text = "Exportar CSV"; export.Width = 120; export.Height = 34;
            export.Click += delegate { ExportCsv(); };
            actions.Controls.Add(enroll); actions.Controls.Add(edit); actions.Controls.Add(mark); actions.Controls.Add(export);

            FlowLayoutPanel filters = new FlowLayoutPanel(); filters.Dock = DockStyle.Top; filters.Height = 42; filters.Padding = new Padding(8, 4, 4, 4);
            filters.Controls.Add(new Label { Text = "Buscar", AutoSize = true, Padding = new Padding(0, 7, 0, 0) });
            _search = new TextBox(); _search.Width = 180; _search.TextChanged += delegate { RefreshGrid(); }; filters.Controls.Add(_search);
            filters.Controls.Add(new Label { Text = "Desde", AutoSize = true, Padding = new Padding(8, 7, 0, 0) });
            _from = new DateTimePicker(); _from.Format = DateTimePickerFormat.Short; _from.Value = DateTime.Today; _from.ValueChanged += delegate { RefreshGrid(); }; filters.Controls.Add(_from);
            filters.Controls.Add(new Label { Text = "Hasta", AutoSize = true, Padding = new Padding(8, 7, 0, 0) });
            _to = new DateTimePicker(); _to.Format = DateTimePickerFormat.Short; _to.Value = DateTime.Today; _to.ValueChanged += delegate { RefreshGrid(); }; filters.Controls.Add(_to);

            _grid = new DataGridView(); _grid.Dock = DockStyle.Fill; _grid.ReadOnly = true; _grid.AllowUserToAddRows = false;
            _grid.AutoGenerateColumns = false; _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            AddColumn("Fecha y hora", "Time", 150); AddColumn("DNI", "Dni", 120); AddColumn("Nombre", "Name", 210);
            AddColumn("Marcación", "Type", 110); AddColumn("Método", "Method", 100); AddColumn("Resultado", "Result", 130); AddColumn("Observación", "Note", 200);
            _readerStatus = new Label(); _readerStatus.Dock = DockStyle.Bottom; _readerStatus.Height = 28;
            _readerStatus.Text = "Lector: integración pendiente de validar con las DLL nativas y el dispositivo.";
            Controls.Add(_grid); Controls.Add(_readerStatus); Controls.Add(filters); Controls.Add(actions);
            RefreshGrid();
        }

        private void AddColumn(string title, string name, int width)
        {
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = title, Name = name, Width = width });
        }

        private void RefreshGrid()
        {
            if (_grid == null) return;
            _grid.Rows.Clear();
            string term = (_search == null ? String.Empty : _search.Text).Trim();
            DateTime from = _from == null ? DateTime.Today : _from.Value.Date;
            DateTime to = _to == null ? DateTime.Today : _to.Value.Date;
            var records = _store.Data.Records.Where(r => r.Time.Date >= from && r.Time.Date <= to)
                .OrderByDescending(r => r.Time);
            foreach (AttendanceRecord record in records)
            {
                Visitor visitor = _store.Data.Visitors.FirstOrDefault(v => String.Equals(v.Dni, record.Dni, StringComparison.OrdinalIgnoreCase));
                string name = visitor == null ? "" : (visitor.FirstName + " " + visitor.LastName).Trim();
                if (term.Length > 0 && record.Dni.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0) continue;
                _grid.Rows.Add(record.Time.ToString("dd/MM/yyyy HH:mm:ss"), record.Dni, name, record.Type, record.Method, record.Result, record.Note);
            }
        }

        private void EditSelected()
        {
            if (_grid.SelectedRows.Count == 0) return;
            string dni = Convert.ToString(_grid.SelectedRows[0].Cells["Dni"].Value);
            Visitor visitor = _store.Data.Visitors.FirstOrDefault(v => String.Equals(v.Dni, dni, StringComparison.OrdinalIgnoreCase));
            if (visitor != null) EditVisitor(visitor);
        }

        private void EditVisitor(Visitor visitor)
        {
            using (PersonForm form = new PersonForm(_store, visitor))
            {
                if (form.ShowDialog(this) == DialogResult.OK) RefreshGrid();
            }
        }

        private void RegisterByDni()
        {
            string dni = Prompt.Show("Ingrese el DNI para localizar al visitante:", "Registro por DNI");
            if (String.IsNullOrWhiteSpace(dni)) return;
            Visitor visitor = _store.Data.Visitors.FirstOrDefault(v => v.Active && String.Equals(v.Dni.Trim(), dni.Trim(), StringComparison.OrdinalIgnoreCase));
            if (visitor == null)
            {
                VisitPopup.ShowUnknown(this);
                return;
            }
            Register(visitor, "DNI");
        }

        private void Register(Visitor visitor, string method)
        {
            DateTime now = DateTime.Now;
            if (visitor.IsDeniedOn(now))
            {
                AttendanceRecord denied = new AttendanceRecord { Dni = visitor.Dni, Time = now, Type = "Intento", Method = method,
                    Result = "INGRESO DENEGADO", Note = visitor.DenialReason ?? String.Empty };
                _store.Data.Records.Add(denied); _store.Save();
                VisitPopup.ShowVisitor(this, visitor, "INGRESO DENEGADO", denied.Note, visitor.DenialUntil);
                RefreshGrid(); return;
            }

            int todayMarks = _store.Data.Records.Count(r => String.Equals(r.Dni, visitor.Dni, StringComparison.OrdinalIgnoreCase) &&
                r.Time.Date == now.Date && r.Result == "Permitido" && (r.Type == "Entrada" || r.Type == "Salida"));
            string type = todayMarks == 0 ? "Entrada" : (todayMarks == 1 ? "Salida" : "Adicional");
            string note = type == "Adicional" ? "Marca adicional: no cambia automáticamente entrada/salida." : String.Empty;
            AttendanceRecord record = new AttendanceRecord { Dni = visitor.Dni, Time = now, Type = type, Method = method, Result = "Permitido", Note = note };
            _store.Data.Records.Add(record); _store.Save();
            VisitPopup.ShowVisitor(this, visitor, type, null, null);
            RefreshGrid();
        }

        private void ExportCsv()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV (*.csv)|*.csv"; dialog.FileName = "marcaciones.csv";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                string term = _search.Text.Trim();
                DateTime from = _from.Value.Date, to = _to.Value.Date;
                StringBuilder csv = new StringBuilder();
                csv.AppendLine("Fecha y hora;DNI;Nombre;Marcacion;Metodo;Resultado;Observacion");
                foreach (AttendanceRecord r in _store.Data.Records.Where(x => x.Time.Date >= from && x.Time.Date <= to).OrderBy(x => x.Time))
                {
                    Visitor v = _store.Data.Visitors.FirstOrDefault(x => String.Equals(x.Dni, r.Dni, StringComparison.OrdinalIgnoreCase));
                    string name = v == null ? "" : (v.FirstName + " " + v.LastName).Trim();
                    if (term.Length > 0 && r.Dni.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0 && name.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    csv.AppendLine(Csv(r.Time.ToString("dd/MM/yyyy HH:mm:ss")) + ";" + Csv(r.Dni) + ";" + Csv(name) + ";" + Csv(r.Type) + ";" + Csv(r.Method) + ";" + Csv(r.Result) + ";" + Csv(r.Note));
                }
                File.WriteAllText(dialog.FileName, "\uFEFF" + csv.ToString(), Encoding.UTF8);
                MessageBox.Show("Se exportaron los resultados filtrados.", "Exportación", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string Csv(string value) { return "\"" + (value ?? String.Empty).Replace("\"", "\"\"") + "\""; }
    }

    internal static class Prompt
    {
        public static string Show(string text, string caption)
        {
            using (Form form = new Form())
            using (TextBox input = new TextBox())
            using (Button ok = new Button())
            using (Button cancel = new Button())
            {
                form.Text = caption; form.Width = 380; form.Height = 140; form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent; form.MinimizeBox = false; form.MaximizeBox = false;
                Label label = new Label { Left = 12, Top = 12, Width = 340, Text = text };
                input.Left = 12; input.Top = 38; input.Width = 340;
                ok.Text = "Aceptar"; ok.Left = 190; ok.Top = 70; ok.DialogResult = DialogResult.OK;
                cancel.Text = "Cancelar"; cancel.Left = 275; cancel.Top = 70; cancel.DialogResult = DialogResult.Cancel;
                form.Controls.Add(label); form.Controls.Add(input); form.Controls.Add(ok); form.Controls.Add(cancel);
                form.AcceptButton = ok; form.CancelButton = cancel;
                return form.ShowDialog() == DialogResult.OK ? input.Text : null;
            }
        }
    }
}
