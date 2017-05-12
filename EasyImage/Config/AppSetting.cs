using System;

namespace EasyImage.Config
{
    [Serializable]
    public class AppSetting
    {
        private string _pluginPath;
        private bool _autoRun;

        public string PluginPath
        {
            get { return _pluginPath; }
            set { _pluginPath = value; }
        }

        public bool AutoRun
        {
            get { return _autoRun; }
            set { _autoRun = value; }
        }

        public AppSetting()
        {
            _pluginPath = "./Plugins";
            _autoRun = false;
        }
    }
}