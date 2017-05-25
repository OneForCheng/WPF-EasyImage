using System;

namespace GifDrawing.Config
{
    [Serializable]
    public class DrawingPickerInfo
    {
        private DrawingTool _drawingTool;
        private bool _selectedMosaic;
        private bool _selectedLeftRadioBtn;
        private byte _leftSliderValue;
        private double _rightSliderValue;
        private ColorInfo _colorInfo;
        private FontInfo _fontInfo;

        public DrawingPickerInfo()
        {
            _drawingTool = DrawingTool.PenTool;
            _selectedMosaic = false;
            _selectedLeftRadioBtn = true;
            _leftSliderValue = 255;
            _rightSliderValue = 0.0;
            _colorInfo = new ColorInfo();
            _fontInfo = new FontInfo();
        }

        public DrawingTool DrawingTool
        {
            get
            {
                return _drawingTool;
            }

            set
            {
                _drawingTool = value;
            }
        }

        public bool SelectedMosaic
        {
            get
            {
                return _selectedMosaic;
            }

            set
            {
                _selectedMosaic = value;
            }
        }

        public bool SelectedLeftRadioBtn
        {
            get
            {
                return _selectedLeftRadioBtn;
            }

            set
            {
                _selectedLeftRadioBtn = value;
            }
        }

        public byte LeftSliderValue
        {
            get
            {
                return _leftSliderValue;
            }

            set
            {
                _leftSliderValue = value;
            }
        }

        public double RightSliderValue
        {
            get
            {
                return _rightSliderValue;
            }

            set
            {
                _rightSliderValue = value;
            }
        }

        public ColorInfo ColorInfo
        {
            get
            {
                return _colorInfo;
            }

            set
            {
                _colorInfo = value;
            }
        }

        public FontInfo FontInfo
        {
            get
            {
                return _fontInfo;
            }

            set
            {
                _fontInfo = value;
            }
        }

    }

    [Serializable]
    public class ColorInfo
    {
        private byte _r;
        private byte _g;
        private byte _b;

        public ColorInfo()
        {
            _r = 0;
            _g = 0;
            _b = 0;
        }

        public byte R
        {
            get
            {
                return _r;
            }

            set
            {
                _r = value;
            }
        }

        public byte G
        {
            get
            {
                return _g;
            }

            set
            {
                _g = value;
            }
        }

        public byte B
        {
            get
            {
                return _b;
            }

            set
            {
                _b = value;
            }
        }
    }

    [Serializable]
    public class FontInfo
    {
        private string _fontFamilyName;
        private double _fontSize;
        private bool _bold;
        private bool _italic;
        private bool _strikeout;
        private bool _underline;

        public FontInfo()
        {
            _fontFamilyName = "ËÎÌו";
            _fontSize = 20;
        }

        public string FontFamilyName
        {
            get
            {
                return _fontFamilyName;
            }

            set
            {
                _fontFamilyName = value;
            }
        }

        public double FontSize
        {
            get
            {
                return _fontSize;
            }

            set
            {
                _fontSize = value;
            }
        }

        public bool Bold
        {
            get
            {
                return _bold;
            }

            set
            {
                _bold = value;
            }
        }

        public bool Italic
        {
            get
            {
                return _italic;
            }

            set
            {
                _italic = value;
            }
        }

        public bool Strikeout
        {
            get
            {
                return _strikeout;
            }

            set
            {
                _strikeout = value;
            }
        }

        public bool Underline
        {
            get
            {
                return _underline;
            }

            set
            {
                _underline = value;
            }
        }
    }
}