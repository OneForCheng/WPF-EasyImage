using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Drawing.Config
{
    [Serializable]
    public class DrawingConfig
    {
        public DrawingConfig()
        {
            
        }

        public void LoadConfigFromXml(string path)
        {
            if (!File.Exists(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
                if (!File.Exists(path)) return;
            }
            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var xmldes = new XmlSerializer(typeof(DrawingConfig));
                    var userConfg = (DrawingConfig)xmldes.Deserialize(fs);
                   
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
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Config/UserConfig.xml");
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
