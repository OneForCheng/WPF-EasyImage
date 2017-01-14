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
            _windowState = new WindowState();
            _appSetting = new AppSetting();
        }

        private WindowState _windowState;
        private ImageSetting _imageSetting;
        private AppSetting _appSetting;

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

        public WindowState WindowState
        {
            get { return _windowState; }
            set { _windowState = value; }
        }

        public AppSetting AppSetting
        {
            get { return _appSetting; }
            set { _appSetting = value; }
        }

        public void LoadConfigFromXml(string path)
        {
            if (!File.Exists(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
                if(!File.Exists(path))return;
            }
            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var xmldes = new XmlSerializer(typeof(UserConfig));
                    var userConfg = (UserConfig)xmldes.Deserialize(fs);
                    _imageSetting = userConfg._imageSetting;
                    _windowState = userConfg._windowState;
                    _appSetting = userConfg.AppSetting;
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }

        public void SaveConfigToXml(string path)
        {
            if (!File.Exists(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Config/UserConfig.xml");
            }   
  
            var xmlsz = new XmlSerializer(typeof(UserConfig));

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