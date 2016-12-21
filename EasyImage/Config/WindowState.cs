using System;
using System.Xml.Serialization;

namespace EasyImage.Config
{
    [Serializable]
    public class WindowState
    {
        [XmlIgnore]
        public string InitEasyImagePath;

    }
}