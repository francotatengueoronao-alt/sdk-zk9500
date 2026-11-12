using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace VisitControl
{
    public sealed class XmlStore
    {
        private readonly string _filePath;
        private readonly string _photoDirectory;
        private LocalData _data;

        public XmlStore()
        {
            string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ControlVisitas");
            Directory.CreateDirectory(root);
            _photoDirectory = Path.Combine(root, "Fotos");
            Directory.CreateDirectory(_photoDirectory);
            _filePath = Path.Combine(root, "datos.xml");
            _data = Load();
        }

        public LocalData Data { get { return _data; } }
        public string PhotoDirectory { get { return _photoDirectory; } }

        private LocalData Load()
        {
            if (!File.Exists(_filePath)) return new LocalData();
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(LocalData));
                using (FileStream stream = File.OpenRead(_filePath)) return (LocalData)serializer.Deserialize(stream);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo leer la base local. Se conservará el archivo original.\r\n" + ex.Message,
                    "Error de datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new LocalData();
            }
        }

        public void Save()
        {
            string temp = _filePath + ".tmp";
            XmlSerializer serializer = new XmlSerializer(typeof(LocalData));
            using (FileStream stream = File.Create(temp)) serializer.Serialize(stream, _data);
            if (File.Exists(_filePath)) File.Delete(_filePath);
            File.Move(temp, _filePath);
        }

        public string CopyPhoto(string source, string dni)
        {
            if (String.IsNullOrEmpty(source) || !File.Exists(source)) return String.Empty;
            string extension = Path.GetExtension(source);
            string destination = Path.Combine(_photoDirectory, SafeFileName(dni) + extension);
            File.Copy(source, destination, true);
            return destination;
        }

        private static string SafeFileName(string value)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
            return value;
        }
    }
}
