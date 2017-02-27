using System;
using System.Windows.Input;

namespace EasyImage.Config
{
    [Serializable]
    public class Shortcut : ICloneable
    {
        private bool _isCtrl;
        private bool _isAlt;
        private bool _isShift;
        private Key _key;

        public Shortcut()
        {
            _isCtrl = _isAlt = true;
            _isShift = false;
            _key = Key.N;
        }

        public bool IsCtrl
        {
            get { return _isCtrl; }
            set { _isCtrl = value; }
        }

        public bool IsAlt
        {
            get { return _isAlt; }
            set { _isAlt = value; }
        }

        public bool IsShift
        {
            get { return _isShift; }
            set { _isShift = value; }
        }

        public Key Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public object Clone()
        {
            return new Shortcut
            {
                IsCtrl = _isCtrl,
                IsAlt = _isAlt,
                IsShift = _isShift,
                Key = _key,
            };
        }
    }

    [Serializable]
    public class ShortcutSetting
    {
        private Shortcut _globelAddShortcut;
        private Shortcut _globelPasteShortcut;

        public ShortcutSetting()
        {
            _globelAddShortcut = new Shortcut
            {
                IsCtrl = true,
                IsAlt = true,
                IsShift = false,
                Key = Key.N,
            };
            _globelPasteShortcut = new Shortcut
            {
                IsCtrl = true,
                IsAlt = true,
                IsShift = false,
                Key = Key.V,
            };
        }

        public Shortcut GlobelAddShortcut
        {
            get { return _globelAddShortcut; }
            set { _globelAddShortcut = value; }
        }

        public Shortcut GlobelPasteShortcut
        {
            get { return _globelPasteShortcut; }
            set { _globelPasteShortcut = value; }
        }


    }

}