using System;

namespace EasyImage.Config
{
    [Serializable]
    public class AppSetting
    {
        private string _pluginPath;

        public string PluginPath
        {
            get { return _pluginPath; }
            set { _pluginPath = value; }
        }

        public AppSetting()
        {
            _pluginPath = "./Plugins";
        }
    }
}