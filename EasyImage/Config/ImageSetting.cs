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
        private double _pasteMoveUnitDistace;

        public ImageSetting()
        {
            _initMaxImgSize = 500;
            _pasteMoveUnitDistace = 15;
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
    }
}