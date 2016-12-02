using System;

namespace EasyImage.Config
{
    /// <summary>
    /// 图像设置
    /// </summary>
    [Serializable]
    public class ImageSetting
    {
        private double _initMaxImgSize;
        private double _minScale;
        private double _maxScale;
        private bool _mouseFouseScale;

        public ImageSetting()
        {
            _initMaxImgSize = 500;
            _minScale = 0.1;
            _maxScale = 3;
            _mouseFouseScale = true;
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

        public double MinScale
        {
            get
            {
                return _minScale;
            }
            set
            {
                _minScale = value;
            }
        }

        public double MaxScale
        {
            get
            {
                return _maxScale;
            }
            set
            {
                _maxScale = value;
            }
        }

        public bool MouseFouseScale
        {
            get
            {
                return _mouseFouseScale;
            }
            set
            {
                _mouseFouseScale = value;
            }
        }
    }
}