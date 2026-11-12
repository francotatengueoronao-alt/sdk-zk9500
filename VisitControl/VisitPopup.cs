using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace VisitControl
{
    public static class VisitPopup
    {
        public static void ShowUnknown(IWin32Window owner)
        {
            using (Form form = NewPopup("HUELLA SIN REGISTRAR", Color.DarkOrange))
            {
                Label message = new Label { Text = "No existe una persona enrolada con ese DNI.", Left = 155, Top = 75, Width = 330, Height = 55, Font = new Font("Arial", 14, FontStyle.Bold) };
                form.Controls.Add(message); Show(form, owner);
            }
        }

        public static void ShowVisitor(IWin32Window owner, Visitor visitor, string mark, string reason, DateTime? until)
        {
            bool denied = mark == "INGRESO DENEGADO";
            using (Form form = NewPopup(denied ? "INGRESO DENEGADO" : "REGISTRO: " + mark.ToUpperInvariant(), denied ? Color.Firebrick : Color.SeaGreen))
            {
                PictureBox photo = new PictureBox { Left = 20, Top = 65, Width = 125, Height = 150, SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };
                try { if (!String.IsNullOrEmpty(visitor.PhotoPath) && File.Exists(visitor.PhotoPath)) using (Image image = Image.FromFile(visitor.PhotoPath)) photo.Image = new Bitmap(image); } catch { }
                form.Controls.Add(photo);
                Label name = new Label { Text = (visitor.FirstName + " " + visitor.LastName).Trim(), Left = 165, Top = 70, Width = 370, Height = 40, Font = new Font("Arial", 20, FontStyle.Bold) };
                Label dni = new Label { Text = "DNI: " + visitor.Dni, Left = 165, Top = 115, Width = 370, Height = 30, Font = new Font("Arial", 16, FontStyle.Regular) };
                Label role = new Label { Text = "Función: " + visitor.Role, Left = 165, Top = 150, Width = 370, Height = 30, Font = new Font("Arial", 13, FontStyle.Regular) };
                form.Controls.Add(name); form.Controls.Add(dni); form.Controls.Add(role);
                if (denied)
                {
                    Label details = new Label { Text = "Motivo: " + (String.IsNullOrWhiteSpace(reason) ? "No especificado" : reason) +
                        (until.HasValue ? "\r\nVencimiento: " + until.Value.ToString("dd/MM/yyyy") : "\r\nVencimiento: No especificado"),
                        Left = 165, Top = 190, Width = 380, Height = 75, Font = new Font("Arial", 12, FontStyle.Bold), ForeColor = Color.Firebrick };
                    form.Controls.Add(details);
                }
                Show(form, owner);
            }
        }

        private static Form NewPopup(string title, Color color)
        {
            Form form = new Form(); form.Text = title; form.Width = 590; form.Height = 330; form.StartPosition = FormStartPosition.CenterScreen;
            form.FormBorderStyle = FormBorderStyle.FixedDialog; form.MaximizeBox = false; form.MinimizeBox = false; form.TopMost = true;
            form.BackColor = Color.White;
            Label banner = new Label { Text = title, Dock = DockStyle.Top, Height = 48, TextAlign = ContentAlignment.MiddleCenter,
                BackColor = color, ForeColor = Color.White, Font = new Font("Arial", 18, FontStyle.Bold) };
            form.Controls.Add(banner); return form;
        }
        private static void Show(Form form, IWin32Window owner)
        {
            Button close = new Button { Text = "Cerrar", Width = 100, Height = 30, Left = 455, Top = 250, DialogResult = DialogResult.OK };
            form.Controls.Add(close); form.AcceptButton = close; form.ShowDialog(owner);
        }
    }
}
