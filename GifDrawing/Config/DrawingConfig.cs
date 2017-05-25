using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace GifDrawing.Config
{
    [Serializable]
    public class DrawingConfig
    {

        private DrawingPickerInfo _drawingPickerInfo;

        public DrawingConfig()
        {
            DrawingPickerInfo = new DrawingPickerInfo();
        }

        public DrawingPickerInfo DrawingPickerInfo
        {
            get
            {
                return _drawingPickerInfo;
            }

            set
            {
                _drawingPickerInfo = value;
            }
        }

        public void LoadConfigFromXml(string path)
        {
            if (!File.Exists(path))
            {
                if (path == null) path = "Config/GifDrawingConfig.xml";
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
                if (!File.Exists(path)) return;
            }
            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var xmldes = new XmlSerializer(typeof(DrawingConfig));
                    var config = (DrawingConfig)xmldes.Deserialize(fs);
                    _drawingPickerInfo = config._drawingPickerInfo;
                }
            }
            catch (Exception ex)
            {
               Trace.WriteLine(ex.ToString());
            }
        }

        public void SaveConfigToXml(string path)
        {
            if (!File.Exists(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Config/GifDrawingConfig.xml");
            }

            var xmlsz = new XmlSerializer(typeof(DrawingConfig));

            var file = new FileInfo(path);

            if (file.Directory != null && !file.Directory.Exists)
            {
                file.Directory.Create();
            }
            using (var sw = new StreamWriter(file.FullName))
            {
                xmlsz.Serialize(sw, this);
            }
        }

    }

  
}
