using System;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using EasyImage.UnmanagedToolkit;

namespace EasyImage.Behaviors
{
    public class AutoHideElementBehavior: Behavior<FrameworkElement> 
    {
        private enum HideState
        {
            None,
            PreviewTopHidden,
            TopHidden,
            PreviewRightHidden,
            RightHidden,
            PreviewBottomHidden,
            BottomHidden,
            PreviewLeftHidden,
            LeftHidden,
        }

        private enum MoveDirection
        {
            Top,
            Right,
            Bottom,
            Left
        }

        #region Private fields
        private HideState _hideStatus;
        private bool _lockTimer;
        private TranslateTransform _cacheTranslateTransform;
        private const double Factor = 3;
        private Win32.Point _curPosition;

        #endregion

        #region Constructor
        public AutoHideElementBehavior()
        {
            _lockTimer = false;
            _hideStatus = HideState.None;
            var autoHideTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            autoHideTimer.Tick += AutoHideTimer_Tick;
            autoHideTimer.Start();
        }

        #endregion

        #region Protected methods

        protected override void OnAttached()
        {
            base.OnAttached();
            _cacheTranslateTransform = AssociatedObject.GetTransform<TranslateTransform>();
        }

        #endregion

        #region Public properties and methods

        public bool IsHide => _hideStatus != HideState.None;
            
        public void Show()
        {
            if (_hideStatus != HideState.None)
            {
                _lockTimer = true;
                switch (_hideStatus)
                {
                    case HideState.PreviewTopHidden:
                        _cacheTranslateTransform.Y = Factor + 1;
                        _hideStatus = HideState.None;
                        _lockTimer = false;
                        break;
                    case HideState.TopHidden:
                        AnimationTranslate(MoveDirection.Bottom, AssociatedObject.ActualHeight + Factor, () =>
                        {
                            _cacheTranslateTransform.Y = Factor + 1;
                            _hideStatus = HideState.None;
                            _lockTimer = false;
                        });
                        break;
                    case HideState.PreviewRightHidden:
                        _cacheTranslateTransform.X = SystemParameters.VirtualScreenWidth - AssociatedObject.ActualWidth - Factor - 1;
                        _hideStatus = HideState.None;
                        _lockTimer = false;
                        break;
                    case HideState.RightHidden:
                        AnimationTranslate(MoveDirection.Left, AssociatedObject.ActualWidth + Factor, () =>
                        {
                            _cacheTranslateTransform.X = SystemParameters.VirtualScreenWidth - AssociatedObject.ActualWidth - Factor - 1;
                            _hideStatus = HideState.None;
                            _lockTimer = false;
                        });
                        break;
                    case HideState.PreviewBottomHidden:
                        _cacheTranslateTransform.Y = SystemParameters.VirtualScreenHeight - AssociatedObject.ActualHeight - Factor - 1;
                        _hideStatus = HideState.None;
                        _lockTimer = false;
                        break;
                    case HideState.BottomHidden:
                        AnimationTranslate(MoveDirection.Top, AssociatedObject.ActualHeight + Factor, () =>
                        {
                            _cacheTranslateTransform.Y = SystemParameters.VirtualScreenHeight - AssociatedObject.ActualHeight - Factor - 1;
                            _hideStatus = HideState.None;
                            _lockTimer = false;
                        });
                        break;
                    case HideState.PreviewLeftHidden:
                        _cacheTranslateTransform.X = Factor + 1;
                        _hideStatus = HideState.None;
                        _lockTimer = false;
                        break;
                    case HideState.LeftHidden:
                        AnimationTranslate(MoveDirection.Right, AssociatedObject.ActualWidth + Factor, () =>
                        {
                            _cacheTranslateTransform.X = Factor + 1;
                            _hideStatus = HideState.None;
                            _lockTimer = false;
                        });
                        break;
                }
            }
        }

        #endregion


        #region Events and Private methods

        private void AutoHideTimer_Tick(object sender, EventArgs e)
        {
            if (!_lockTimer)
            {
                _lockTimer = true;
                if (Win32.GetCursorPos(out _curPosition))
                {
                    switch (_hideStatus)
                    {
                        case HideState.None:
                            if (_cacheTranslateTransform.Y <= Factor)
                            {
                                _hideStatus = HideState.PreviewTopHidden;
                            }
                            else if (_cacheTranslateTransform.X + AssociatedObject.ActualWidth >= SystemParameters.VirtualScreenWidth - Factor)
                            {
                                _hideStatus = HideState.PreviewRightHidden;
                            }
                            else if (_cacheTranslateTransform.Y + AssociatedObject.ActualHeight >= SystemParameters.VirtualScreenHeight - Factor)
                            {
                                _hideStatus = HideState.PreviewBottomHidden;
                            }
                            else if (_cacheTranslateTransform.X <= Factor)
                            {
                                _hideStatus = HideState.PreviewLeftHidden;
                            }
                            _lockTimer = false;
                            break;
                        case HideState.PreviewTopHidden:
                            if (_cacheTranslateTransform.Y <= Factor)
                            {
                                if (_curPosition.X < _cacheTranslateTransform.X ||
                                _curPosition.X > _cacheTranslateTransform.X + AssociatedObject.ActualWidth ||
                                _curPosition.Y > _cacheTranslateTransform.Y + AssociatedObject.ActualHeight)
                                {
                                    AnimationTranslate(MoveDirection.Top, AssociatedObject.ActualHeight + Factor, () =>
                                    {
                                        _cacheTranslateTransform.Y = -(AssociatedObject.ActualHeight + Factor);
                                        _hideStatus = HideState.TopHidden;
                                        _lockTimer = false;
                                    });
                                }
                                else
                                {
                                    _lockTimer = false;
                                }
                            }
                            else
                            {
                                _hideStatus = HideState.None;
                                _lockTimer = false;
                            }
                            break;
                        case HideState.TopHidden:
                            if (_curPosition.Y <= Factor &&
                                _curPosition.X >= _cacheTranslateTransform.X &&
                                _curPosition.X <= _cacheTranslateTransform.X + AssociatedObject.ActualWidth)
                            {
                                AnimationTranslate(MoveDirection.Bottom, AssociatedObject.ActualHeight + Factor, () =>
                                {
                                    _cacheTranslateTransform.Y = 0;
                                    _hideStatus = HideState.PreviewTopHidden;
                                    _lockTimer = false;
                                });

                            }
                            else
                            {
                                _lockTimer = false;
                            }
                            break;
                        case HideState.PreviewRightHidden:
                            if (_cacheTranslateTransform.X + AssociatedObject.ActualWidth >= SystemParameters.VirtualScreenWidth - Factor)
                            {
                                if (_curPosition.X < _cacheTranslateTransform.X ||
                                _curPosition.Y < _cacheTranslateTransform.Y ||
                                _curPosition.Y > _cacheTranslateTransform.Y + AssociatedObject.ActualHeight)
                                {
                                    AnimationTranslate(MoveDirection.Right, AssociatedObject.ActualHeight + Factor, () =>
                                    {
                                        _cacheTranslateTransform.X = (SystemParameters.VirtualScreenWidth + Factor);
                                        _hideStatus = HideState.RightHidden;
                                        _lockTimer = false;
                                    });
                                }
                                else
                                {
                                    _lockTimer = false;
                                }
                            }
                            else
                            {
                                _hideStatus = HideState.None;
                                _lockTimer = false;
                            }
                            break;
                        case HideState.RightHidden:
                            if (_curPosition.X >= SystemParameters.VirtualScreenWidth - Factor &&
                                _curPosition.Y >= _cacheTranslateTransform.Y &&
                                _curPosition.Y <= _cacheTranslateTransform.Y + AssociatedObject.ActualHeight)
                            {
                                AnimationTranslate(MoveDirection.Left, AssociatedObject.ActualWidth + Factor, () =>
                                {
                                    _cacheTranslateTransform.X = SystemParameters.VirtualScreenWidth - AssociatedObject.ActualWidth;
                                    _hideStatus = HideState.PreviewRightHidden;
                                    _lockTimer = false;
                                });

                            }
                            else
                            {
                                _lockTimer = false;
                            }
                            break;
                        case HideState.PreviewBottomHidden:
                            if (_cacheTranslateTransform.Y + AssociatedObject.ActualHeight >= SystemParameters.VirtualScreenHeight - Factor)
                            {
                                if (_curPosition.Y < _cacheTranslateTransform.Y ||
                               _curPosition.X < _cacheTranslateTransform.X ||
                               _curPosition.X > _cacheTranslateTransform.X + AssociatedObject.ActualWidth)
                                {
                                    AnimationTranslate(MoveDirection.Bottom, AssociatedObject.ActualHeight + Factor, () =>
                                    {
                                        _cacheTranslateTransform.Y = (SystemParameters.VirtualScreenHeight + Factor);
                                        _hideStatus = HideState.BottomHidden;
                                        _lockTimer = false;
                                      
                                    });
                                }
                                else
                                {
                                    _lockTimer = false;
                                }
                            }
                            else
                            {
                                _hideStatus = HideState.None;
                                _lockTimer = false;
                            }
                            break;
                        case HideState.BottomHidden:
                            if (_curPosition.Y >= SystemParameters.VirtualScreenHeight - Factor &&
                               _curPosition.X >= _cacheTranslateTransform.X &&
                               _curPosition.X <= _cacheTranslateTransform.X + AssociatedObject.ActualWidth)
                            {
                                AnimationTranslate(MoveDirection.Top, AssociatedObject.ActualHeight + Factor, () =>
                                {
                                    _cacheTranslateTransform.Y = SystemParameters.VirtualScreenHeight - AssociatedObject.ActualHeight;
                                    _hideStatus = HideState.PreviewBottomHidden;
                                    _lockTimer = false;
                                });
                            }
                            else
                            {
                                _lockTimer = false;
                            }
                            break;
                        case HideState.PreviewLeftHidden:
                            if (_cacheTranslateTransform.X <= Factor)
                            {
                                if (_curPosition.X > _cacheTranslateTransform.X + AssociatedObject.ActualWidth ||
                                 _curPosition.Y < _cacheTranslateTransform.Y ||
                                 _curPosition.Y > _cacheTranslateTransform.Y + AssociatedObject.ActualHeight)
                                {
                                    AnimationTranslate(MoveDirection.Left, AssociatedObject.ActualWidth + Factor, () =>
                                    {
                                        _cacheTranslateTransform.X = -(AssociatedObject.ActualWidth + Factor);
                                        _hideStatus = HideState.LeftHidden;
                                        _lockTimer = false;
                                    });
                                }
                                else
                                {
                                    _lockTimer = false;
                                }
                            }
                            else
                            {
                                _hideStatus = HideState.None;
                                _lockTimer = false;
                            }
                            break;
                        case HideState.LeftHidden:
                            if (_curPosition.X <= Factor &&
                                 _curPosition.Y >= _cacheTranslateTransform.Y &&
                                 _curPosition.Y <= _cacheTranslateTransform.Y + AssociatedObject.ActualHeight)
                            {
                                AnimationTranslate(MoveDirection.Right, AssociatedObject.ActualWidth + Factor, () =>
                                {
                                    _cacheTranslateTransform.X = 0;
                                    _hideStatus = HideState.PreviewLeftHidden;
                                    _lockTimer = false;
                                });
                            }
                            else
                            {
                                _lockTimer = false;
                            }
                            break;
                    }
                }
            }
        }

        private void AnimationTranslate(MoveDirection direction, double distance, Action completedEvent = null)
        {
            double fromValue = 0, toValue = 0;
            var dependencyProperty = TranslateTransform.YProperty;
            switch (direction)
            {
                case MoveDirection.Top:
                    dependencyProperty = TranslateTransform.YProperty;
                    fromValue = _cacheTranslateTransform.Y;
                    toValue = fromValue - distance;
                    break;
                case MoveDirection.Right:
                    dependencyProperty = TranslateTransform.XProperty;
                    fromValue = _cacheTranslateTransform.X;
                    toValue = fromValue + distance;
                    break;
                case MoveDirection.Bottom:
                    dependencyProperty = TranslateTransform.YProperty;
                    fromValue = _cacheTranslateTransform.Y;
                    toValue = fromValue + distance;
                    break;
                case MoveDirection.Left:
                    dependencyProperty = TranslateTransform.XProperty;
                    fromValue = _cacheTranslateTransform.X;
                    toValue = fromValue - distance;
                    break;
            }
            var animation = new DoubleAnimation(fromValue, toValue, new Duration(TimeSpan.FromMilliseconds(500)), FillBehavior.Stop);
            if (completedEvent != null)
            {
                animation.Completed += (sender, args) => { completedEvent.Invoke(); };
            }
            _cacheTranslateTransform.BeginAnimation(dependencyProperty, animation);
        }

        #endregion
    }

    public class AutoHideWindowBehavior : Behavior<Window>
    {
        private enum HideState
        {
            None,
            PreviewTopHidden,
            TopHidden,
            PreviewRightHidden,
            RightHidden,
            PreviewBottomHidden,
            BottomHidden,
            PreviewLeftHidden,
            LeftHidden,
        }

        private enum MoveDirection
        {
            Top,
            Right,
            Bottom,
            Left
        }

        #region Private fields
        private HideState _hideStatus;
        private bool _lockTimer;
        private const double Factor = 3;
        private Win32.Point _curPosition;

        #endregion

        #region Constructor
        public AutoHideWindowBehavior()
        {
            _lockTimer = false;
            _hideStatus = HideState.None;
            var autoHideTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            autoHideTimer.Tick += AutoHideTimer_Tick;
            autoHideTimer.Start();
        }

        #endregion

        #region Public properties and methods

        public bool IsHide => _hideStatus != HideState.None;

        public void Show()
        {
            if (_hideStatus != HideState.None)
            {
                _lockTimer = true;
                switch (_hideStatus)
                {
                    case HideState.PreviewTopHidden:
                        AssociatedObject.Top = Factor + 1;
                        _hideStatus = HideState.None;
                        _lockTimer = false;
                        break;
                    case HideState.TopHidden:
                        AnimationTranslate(MoveDirection.Bottom, AssociatedObject.ActualHeight + Factor, () =>
                        {
                            AssociatedObject.Top = Factor + 1;
                            _hideStatus = HideState.None;
                            _lockTimer = false;
                        });
                        break;
                    case HideState.PreviewRightHidden:
                        AssociatedObject.Left = SystemParameters.VirtualScreenWidth - AssociatedObject.ActualWidth - Factor - 1;
                        _hideStatus = HideState.None;
                        _lockTimer = false;
                        break;
                    case HideState.RightHidden:
                        AnimationTranslate(MoveDirection.Left, AssociatedObject.ActualWidth + Factor, () =>
                        {
                            AssociatedObject.Left = SystemParameters.VirtualScreenWidth - AssociatedObject.ActualWidth - Factor - 1;
                            _hideStatus = HideState.None;
                            _lockTimer = false;
                        });
                        break;
                    case HideState.PreviewBottomHidden:
                        AssociatedObject.Top = SystemParameters.VirtualScreenHeight - AssociatedObject.ActualHeight - Factor - 1;
                        _hideStatus = HideState.None;
                        _lockTimer = false;
                        break;
                    case HideState.BottomHidden:
                        AnimationTranslate(MoveDirection.Top, AssociatedObject.ActualHeight + Factor, () =>
                        {
                            AssociatedObject.Top = SystemParameters.VirtualScreenHeight - AssociatedObject.ActualHeight - Factor - 1;
                            _hideStatus = HideState.None;
                            _lockTimer = false;
                        });
                        break;
                    case HideState.PreviewLeftHidden:
                        AssociatedObject.Left = Factor + 1;
                        _hideStatus = HideState.None;
                        _lockTimer = false;
                        break;
                    case HideState.LeftHidden:
                        AnimationTranslate(MoveDirection.Right, AssociatedObject.ActualWidth + Factor, () =>
                        {
                            AssociatedObject.Left = Factor + 1;
                            _hideStatus = HideState.None;
                            _lockTimer = false;
                        });
                        break;
                }
            }
        }

        #endregion


        #region Events and Private methods

        private void AutoHideTimer_Tick(object sender, EventArgs e)
        {
            if (!_lockTimer)
            {
                _lockTimer = true;
                if (Win32.GetCursorPos(out _curPosition))
                {
                    switch (_hideStatus)
                    {
                        case HideState.None:
                            if (AssociatedObject.Top <= Factor)
                            {
                                _hideStatus = HideState.PreviewTopHidden;
                            }
                            else if (AssociatedObject.Left + AssociatedObject.ActualWidth >= SystemParameters.VirtualScreenWidth - Factor)
                            {
                                _hideStatus = HideState.PreviewRightHidden;
                            }
                            else if (AssociatedObject.Top + AssociatedObject.ActualHeight >= SystemParameters.VirtualScreenHeight - Factor)
                            {
                                _hideStatus = HideState.PreviewBottomHidden;
                            }
                            else if (AssociatedObject.Left <= Factor)
                            {
                                _hideStatus = HideState.PreviewLeftHidden;
                            }
                            _lockTimer = false;
                            break;
                        case HideState.PreviewTopHidden:
                            if (AssociatedObject.Top <= Factor)
                            {
                                if (_curPosition.X < AssociatedObject.Left ||
                                _curPosition.X > AssociatedObject.Left + AssociatedObject.ActualWidth ||
                                _curPosition.Y > AssociatedObject.Top + AssociatedObject.ActualHeight)
                                {
                                    AnimationTranslate(MoveDirection.Top, AssociatedObject.ActualHeight + Factor, () =>
                                    {
                                        AssociatedObject.Top = -(AssociatedObject.ActualHeight + Factor);
                                        _hideStatus = HideState.TopHidden;
                                        _lockTimer = false;
                                    });
                                }
                                else
                                {
                                    _lockTimer = false;
                                }
                            }
                            else
                            {
                                _hideStatus = HideState.None;
                                _lockTimer = false;
                            }
                            break;
                        case HideState.TopHidden:
                            if (_curPosition.Y <= Factor &&
                                _curPosition.X >= AssociatedObject.Left &&
                                _curPosition.X <= AssociatedObject.Left + AssociatedObject.ActualWidth)
                            {
                                AnimationTranslate(MoveDirection.Bottom, AssociatedObject.ActualHeight + Factor, () =>
                                {
                                    AssociatedObject.Top = 0;
                                    _hideStatus = HideState.PreviewTopHidden;
                                    _lockTimer = false;
                                });

                            }
                            else
                            {
                                _lockTimer = false;
                            }
                            break;
                        case HideState.PreviewRightHidden:
                            if (AssociatedObject.Left + AssociatedObject.ActualWidth >= SystemParameters.VirtualScreenWidth - Factor)
                            {
                                if (_curPosition.X < AssociatedObject.Left ||
                                _curPosition.Y < AssociatedObject.Top ||
                                _curPosition.Y > AssociatedObject.Top + AssociatedObject.ActualHeight)
                                {
                                    AnimationTranslate(MoveDirection.Right, AssociatedObject.ActualHeight + Factor, () =>
                                    {
                                        AssociatedObject.Left = (SystemParameters.VirtualScreenWidth + Factor);
                                        _hideStatus = HideState.RightHidden;
                                        _lockTimer = false;
                                    });
                                }
                                else
                                {
                                    _lockTimer = false;
                                }
                            }
                            else
                            {
                                _hideStatus = HideState.None;
                                _lockTimer = false;
                            }
                            break;
                        case HideState.RightHidden:
                            if (_curPosition.X >= SystemParameters.VirtualScreenWidth - Factor &&
                                _curPosition.Y >= AssociatedObject.Top &&
                                _curPosition.Y <= AssociatedObject.Top + AssociatedObject.ActualHeight)
                            {
                                AnimationTranslate(MoveDirection.Left, AssociatedObject.ActualWidth + Factor, () =>
                                {
                                    AssociatedObject.Left = SystemParameters.VirtualScreenWidth - AssociatedObject.ActualWidth;
                                    _hideStatus = HideState.PreviewRightHidden;
                                    _lockTimer = false;
                                });

                            }
                            else
                            {
                                _lockTimer = false;
                            }
                            break;
                        case HideState.PreviewBottomHidden:
                            if (AssociatedObject.Top + AssociatedObject.ActualHeight >= SystemParameters.VirtualScreenHeight - Factor)
                            {
                                if (_curPosition.Y < AssociatedObject.Top ||
                               _curPosition.X < AssociatedObject.Left ||
                               _curPosition.X > AssociatedObject.Left + AssociatedObject.ActualWidth)
                                {
                                    AnimationTranslate(MoveDirection.Bottom, AssociatedObject.ActualHeight + Factor, () =>
                                    {
                                        AssociatedObject.Top = (SystemParameters.VirtualScreenHeight + Factor);
                                        _hideStatus = HideState.BottomHidden;
                                        _lockTimer = false;

                                    });
                                }
                                else
                                {
                                    _lockTimer = false;
                                }
                            }
                            else
                            {
                                _hideStatus = HideState.None;
                                _lockTimer = false;
                            }
                            break;
                        case HideState.BottomHidden:
                            if (_curPosition.Y >= SystemParameters.VirtualScreenHeight - Factor &&
                               _curPosition.X >= AssociatedObject.Left &&
                               _curPosition.X <= AssociatedObject.Left + AssociatedObject.ActualWidth)
                            {
                                AnimationTranslate(MoveDirection.Top, AssociatedObject.ActualHeight + Factor, () =>
                                {
                                    AssociatedObject.Top = SystemParameters.VirtualScreenHeight - AssociatedObject.ActualHeight;
                                    _hideStatus = HideState.PreviewBottomHidden;
                                    _lockTimer = false;
                                });
                            }
                            else
                            {
                                _lockTimer = false;
                            }
                            break;
                        case HideState.PreviewLeftHidden:
                            if (AssociatedObject.Left <= Factor)
                            {
                                if (_curPosition.X > AssociatedObject.Left + AssociatedObject.ActualWidth ||
                                 _curPosition.Y < AssociatedObject.Top ||
                                 _curPosition.Y > AssociatedObject.Top + AssociatedObject.ActualHeight)
                                {
                                    AnimationTranslate(MoveDirection.Left, AssociatedObject.ActualWidth + Factor, () =>
                                    {
                                        AssociatedObject.Left = -(AssociatedObject.ActualWidth + Factor);
                                        _hideStatus = HideState.LeftHidden;
                                        _lockTimer = false;
                                    });
                                }
                                else
                                {
                                    _lockTimer = false;
                                }
                            }
                            else
                            {
                                _hideStatus = HideState.None;
                                _lockTimer = false;
                            }
                            break;
                        case HideState.LeftHidden:
                            if (_curPosition.X <= Factor &&
                                 _curPosition.Y >= AssociatedObject.Top &&
                                 _curPosition.Y <= AssociatedObject.Top + AssociatedObject.ActualHeight)
                            {
                                AnimationTranslate(MoveDirection.Right, AssociatedObject.ActualWidth + Factor, () =>
                                {
                                    AssociatedObject.Left = 0;
                                    _hideStatus = HideState.PreviewLeftHidden;
                                    _lockTimer = false;
                                });
                            }
                            else
                            {
                                _lockTimer = false;
                            }
                            break;
                    }
                }
            }
        }

        private void AnimationTranslate(MoveDirection direction, double distance, Action completedEvent = null)
        {
            double fromValue = 0, toValue = 0;
            var dependencyProperty = Window.TopProperty;
            switch (direction)
            {
                case MoveDirection.Top:
                    dependencyProperty = Window.TopProperty;
                    fromValue = AssociatedObject.Top;
                    toValue = fromValue - distance;
                    break;
                case MoveDirection.Right:
                    dependencyProperty = Window.LeftProperty;
                    fromValue = AssociatedObject.Left;
                    toValue = fromValue + distance;
                    break;
                case MoveDirection.Bottom:
                    dependencyProperty = Window.TopProperty;
                    fromValue = AssociatedObject.Top;
                    toValue = fromValue + distance;
                    break;
                case MoveDirection.Left:
                    dependencyProperty = Window.LeftProperty;
                    fromValue = AssociatedObject.Left;
                    toValue = fromValue - distance;
                    break;
            }
            var animation = new DoubleAnimation(fromValue, toValue, new Duration(TimeSpan.FromMilliseconds(500)), FillBehavior.Stop);
            if (completedEvent != null)
            {
                animation.Completed += (sender, args) => { completedEvent.Invoke(); };
            }
            AssociatedObject.BeginAnimation(dependencyProperty, animation);
        }

        #endregion
    }


}
