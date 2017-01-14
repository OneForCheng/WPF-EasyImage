using System;
using System.Windows;

namespace EasyImage.Config
{
    /// <summary>
    /// 图像设置
    /// </summary>
    [Serializable]
    public class ImageSetting
    {
        private double _initMaxImgSize;
        private double _pasteMoveUnitDistace;
        private MainMenuInfo _mainMenuInfo;

        public ImageSetting()
        {
            _initMaxImgSize = 500;
            _pasteMoveUnitDistace = 15;
            _mainMenuInfo = new MainMenuInfo();
        }

        public double InitMaxImgSize
        {
            get
            {
                return _initMaxImgSize;
            }
            set
            {
                _initMaxImgSize = value;
            }
        }

        public double PasteMoveUnitDistace
        {
            get
            {
                return _pasteMoveUnitDistace;
            }
            set
            {
                _pasteMoveUnitDistace = value;
            }
        }

        public MainMenuInfo MainMenuInfo
        {
            get { return _mainMenuInfo; }
            set { _mainMenuInfo = value; }
        }
    }

    [Serializable]
    public class MainMenuInfo
    {
        private string _mainMenuIconPath;
        private double _width;
        private double _height;
        private double _translateX;
        private double _translateY;

        public MainMenuInfo()
        {
            _mainMenuIconPath = string.Empty;
            _width = _height = 30;
            _translateX = SystemParameters.VirtualScreenWidth / 2;
            _translateY = SystemParameters.VirtualScreenHeight / 2;
        }

        public string Path
        {
            get { return _mainMenuIconPath; }
            set { _mainMenuIconPath = value; }
        }

        public double Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
            }
        }

        public double Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
            }
        }

        public double TranslateX
        {
            get
            {
                if (_translateX < 0)
                {
                    _translateX = 0;
                }
                else if (_translateX > SystemParameters.VirtualScreenWidth - Width)
                {
                    _translateX = SystemParameters.VirtualScreenWidth - Width;
                }
                return _translateX;
            }
            set
            {
                if (_translateX < 0)
                {
                    _translateX = 0;
                }
                else if (_translateX > SystemParameters.VirtualScreenWidth - Width)
                {
                    _translateX = SystemParameters.VirtualScreenWidth - Width;
                }
                else
                {
                    _translateX = value;
                }
                
            }
        }

        public double TranslateY
        {
            get
            {
                if (_translateY < 0)
                {
                    _translateY = 0;
                }
                else if (_translateY > SystemParameters.VirtualScreenHeight - Height)
                {
                    _translateY = SystemParameters.VirtualScreenHeight - Height;
                }
                return _translateY;
            }
            set
            {
                if (_translateY < 0)
                {
                    _translateY = 0;
                }
                else if (_translateY > SystemParameters.VirtualScreenHeight - Height)
                {
                    _translateY = SystemParameters.VirtualScreenHeight - Height;
                }
                else
                {
                    _translateY = value;
                }
            }
        }
    }
}