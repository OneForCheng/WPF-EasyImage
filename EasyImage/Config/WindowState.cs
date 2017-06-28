using System;
using System.Windows;
using System.Xml.Serialization;

namespace EasyImage.Config
{
    [Serializable]
    public class WindowState
    {
        [XmlIgnore]
        public string InitEasyImagePath;

        private ImageWindowState _imageWindowState;
        private ImageFavoritesWindowState _imageFavoritesWindowState;

        public WindowState()
        {
            _imageWindowState = new ImageWindowState();
            _imageFavoritesWindowState = new ImageFavoritesWindowState();
        }

        public ImageFavoritesWindowState ImageFavoritesWindowState
        {
            get
            {
                return _imageFavoritesWindowState;
            }

            set
            {
                _imageFavoritesWindowState = value;
            }
        }

        public ImageWindowState ImageWindowState
        {
            get
            {
                return _imageWindowState;
            }

            set
            {
                _imageWindowState = value;
            }
        }
    }

    [Serializable]
    public class ImageWindowState
    {
        private double _autoHideFactor;
        private string _previousSaveImagePath;

        public ImageWindowState()
        {
            _autoHideFactor = 10.0;
            _previousSaveImagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public double AutoHideFactor
        {
            get
            {
                return _autoHideFactor;
            }

            set
            {
                _autoHideFactor = value;
            }

        }
        public string PreviousSaveImagePath
        {
            get
            {
                return _previousSaveImagePath;
            }

            set
            {
                _previousSaveImagePath = value;
            }
        }
    }

    [Serializable]
    public class ImageFavoritesWindowState
    {
        private string _favoritesPath;
        private double _width;
        private double _height;
        private double _left;
        private double _top;

        public ImageFavoritesWindowState()
        {
            _favoritesPath = "Favorites";
            _top = _left = 0;
            _width = _height = 300;

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

        public double Left
        {
            get
            {
                if (_left < 0)
                {
                    _left = 0;
                }
                else if (_left > SystemParameters.VirtualScreenWidth - Width)
                {
                    _left = SystemParameters.VirtualScreenWidth - Width;
                }
                return _left;
            }

            set
            {
                if (value < 0)
                {
                    _left = 0;
                }
                else if (value > SystemParameters.VirtualScreenWidth - Width)
                {
                    _left = SystemParameters.VirtualScreenWidth - Width;
                }
                else
                {
                    _left = value;
                }
               
            }
        }

        public double Top
        {
            get
            {
                if (_top < 0)
                {
                    _top = 0;
                }
                else if (_top > SystemParameters.VirtualScreenHeight - Height)
                {
                    _top = SystemParameters.VirtualScreenHeight - Height;
                }
                return _top;
            }

            set
            {
                if (value < 0)
                {
                    _top = 0;
                }
                else if (value > SystemParameters.VirtualScreenHeight - Height)
                {
                    _top = SystemParameters.VirtualScreenHeight - Height;
                }
                else
                {
                    _top = value;
                }
            }
        }

        public string FavoritesPath
        {
            get
            {
                return _favoritesPath;
            }

            set
            {
                _favoritesPath = value;
            }
        }
    }
}