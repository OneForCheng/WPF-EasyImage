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
            _shortcutSetting = new ShortcutSetting();
        }

        private WindowState _windowState;
        private ImageSetting _imageSetting;
        private AppSetting _appSetting;
        private ShortcutSetting _shortcutSetting;

        [XmlIgnore]
        public string DataFilePath { get; private set; }

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

        public ShortcutSetting ShortcutSetting
        {
            get { return _shortcutSetting; }
            set { _shortcutSetting = value; }
        }

        public void Load(string path)
        {
            if (!File.Exists(path))
            {
                if(path == null)path = "Config/UserConfig.xml";
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
                if(!File.Exists(path))return;
            }
            DataFilePath = path;
            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var xmldes = new XmlSerializer(typeof(UserConfig));
                    var config = (UserConfig)xmldes.Deserialize(fs);
                    _imageSetting = config._imageSetting;
                    _windowState = config._windowState;
                    _appSetting = config.AppSetting;
                    _shortcutSetting = config.ShortcutSetting;
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }

        public void SaveChanged()
        {
            var path = DataFilePath;
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