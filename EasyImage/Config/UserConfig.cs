using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace EasyImage.Config
{
    /// <summary>
    /// 用户配置信息
    /// </summary>
    [Serializable]
    public class UserConfig
    {
        public UserConfig()
        {
            _imageSetting = new ImageSetting();
        }

        private ImageSetting _imageSetting;

        public ImageSetting ImageSetting
        {
            get
            {
                return _imageSetting;
            }
            set
            {
                _imageSetting = value;
            }
        }

        public void LoadConfigFromXml(string path = "Config/UserConfig.xml")
        {
            if (!File.Exists(path)) return;
            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var xmldes = new XmlSerializer(typeof(UserConfig));
                    var userConfg = (UserConfig)xmldes.Deserialize(fs);
                    _imageSetting = userConfg._imageSetting;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"LoadConfigFromXml operation failed: {ex.Message}");
            }
        }

        public void SaveConfigToXml(string path = "Config/UserConfig.xml")
        {
            var xmlsz = new XmlSerializer(typeof(UserConfig));

            var file = new FileInfo(path);

            if (file.Directory != null && !file.Directory.Exists)
            {
                file.Directory.Create();
            }
            if (!file.Exists)
            {
                using (var fs = file.Create())
                {
                    xmlsz.Serialize(fs, this);
                }
            }
            else
            {
                using (var fs = file.Open(FileMode.Open))
                {
                    xmlsz.Serialize(fs, this);
                }
            }
        }
    }
}