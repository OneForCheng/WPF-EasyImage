using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using UnmanagedToolkit;

namespace EasyImage.Behaviors
{
    public class AutoHideElementBehavior<T> : Behavior<FrameworkElement> where T : FrameworkElement
    {
        public AutoHideElementBehavior()
        {
            _lockTimer = false;
            _hideStatus = HideState.None;
            _autoHideTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(100)};
            _autoHideTimer.Tick += AutoHideTimer_Tick;
        }

        public bool IsHide => _hideStatus != HideState.None;

        private enum HideState
        {
            None,
            Top,
            Right,
            Bottom,
            Left,
        }

        private HideState _hideStatus;
        private readonly DispatcherTimer _autoHideTimer;//一个触发器
        private bool _lockTimer;
        private T _targetElement;
        private TranslateTransform _cacheTranslateTransform;
        private const double Factor = 3;
        private Win32.Point _curPosition;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
        }

        private void AnimationTranslate(HideState hideDirection, double distance, Action completedEvent = null)
        {
            double fromValue  = 0, toValue = 0;
            var dependencyProperty = TranslateTransform.YProperty;
            switch (hideDirection)
            {
                case HideState.Top:
                    dependencyProperty = TranslateTransform.YProperty;
                    fromValue = _cacheTranslateTransform.Y;
                    toValue = fromValue - distance;
                    break;
                case HideState.Right:
                    dependencyProperty = TranslateTransform.XProperty;
                    fromValue = _cacheTranslateTransform.X;
                    toValue = fromValue + distance;
                    break;
                case HideState.Bottom:
                    dependencyProperty = TranslateTransform.YProperty;
                    fromValue = _cacheTranslateTransform.Y;
                    toValue = fromValue + distance;
                    break;
                case HideState.Left:
                    dependencyProperty = TranslateTransform.XProperty;
                    fromValue = _cacheTranslateTransform.X;
                    toValue = fromValue - distance;
                    break;
            }
            var animation = new DoubleAnimation(fromValue, toValue, new Duration(TimeSpan.FromMilliseconds(500)), FillBehavior.Stop);
            if (completedEvent != null)
            {
                animation.Completed += (sender, args) =>
                {
                    completedEvent.Invoke();
                };
            }
            _cacheTranslateTransform.BeginAnimation(dependencyProperty, animation);
            
        }

        #region Event
        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            if(_hideStatus != HideState.None) return;
            _targetElement = e.Source as T;
            if(_targetElement == null) return;
            _cacheTranslateTransform = _targetElement.GetTransform<TranslateTransform>();
            if (_cacheTranslateTransform.Y < Factor)
            {
                _hideStatus = HideState.Top;
            }
            else if (_cacheTranslateTransform.X + _targetElement.ActualWidth + Factor > SystemParameters.VirtualScreenWidth)
            {
                _hideStatus = HideState.Right;
            }
            else if (_cacheTranslateTransform.Y + _targetElement.ActualHeight + Factor > SystemParameters.VirtualScreenHeight)
            {
                _hideStatus = HideState.Bottom;
            }
            else if (_cacheTranslateTransform.X < Factor)
            {
                _hideStatus = HideState.Left;
            }
            if (_hideStatus == HideState.None) return;
            switch (_hideStatus)
            {
                case HideState.Top:
                    AnimationTranslate(_hideStatus, _targetElement.ActualHeight + Factor, () =>
                    {
                        _cacheTranslateTransform.Y = -(_targetElement.ActualHeight +  Factor);
                        _lockTimer = false;
                        _autoHideTimer.Start();
                    });
                    break;
                case HideState.Right:
                    AnimationTranslate(_hideStatus, _targetElement.ActualHeight + Factor, () =>
                    {
                        _cacheTranslateTransform.X = (SystemParameters.VirtualScreenWidth + Factor);
                        _lockTimer = false;
                        _autoHideTimer.Start();
                    });
                    break;
                case HideState.Bottom:
                    AnimationTranslate(_hideStatus, _targetElement.ActualHeight + Factor, () =>
                    {
                        _cacheTranslateTransform.Y = (SystemParameters.VirtualScreenHeight + Factor);
                        _lockTimer = false;
                        _autoHideTimer.Start();
                    });
                    break;
                case HideState.Left:
                    AnimationTranslate(_hideStatus, _targetElement.ActualWidth + Factor, () =>
                    {
                        _cacheTranslateTransform.X = -(_targetElement.ActualWidth + Factor);
                        _lockTimer = false;
                        _autoHideTimer.Start();
                    });
                    break;
            }
            
        }

        private void AutoHideTimer_Tick(object sender, EventArgs e)
        {
            if (!_lockTimer)
            {
                _lockTimer = true;
                if (Win32.GetCursorPos(out _curPosition))
                {
                    switch (_hideStatus)
                    {
                        case HideState.Top:
                            if (_curPosition.Y <= Factor &&
                                _curPosition.X >= _cacheTranslateTransform.X &&
                                _curPosition.X <= _cacheTranslateTransform.X + _targetElement.ActualWidth)
                            {
                                _autoHideTimer.Stop();
                                AnimationTranslate(HideState.Bottom, _targetElement.ActualHeight + Factor, () =>
                                {
                                    _cacheTranslateTransform.Y = 0;
                                    _hideStatus = HideState.None;

                                });
                                
                            }
                            break;
                        case HideState.Right:
                            if (_curPosition.X >= SystemParameters.VirtualScreenWidth - Factor &&
                                _curPosition.Y >= _cacheTranslateTransform.Y &&
                                _curPosition.Y <= _cacheTranslateTransform.Y + _targetElement.ActualHeight)
                            {
                                _autoHideTimer.Stop();
                                AnimationTranslate(HideState.Left, _targetElement.ActualWidth + Factor, () =>
                                {
                                    _cacheTranslateTransform.X = SystemParameters.VirtualScreenWidth - _targetElement.ActualWidth;
                                    _hideStatus = HideState.None;
                                });
                                
                            }
                            break;
                        case HideState.Bottom:
                            if (_curPosition.Y >= SystemParameters.VirtualScreenHeight - Factor &&
                               _curPosition.X >= _cacheTranslateTransform.X &&
                               _curPosition.X <= _cacheTranslateTransform.X + _targetElement.ActualWidth)
                            {
                                _autoHideTimer.Stop();
                                AnimationTranslate(HideState.Top, _targetElement.ActualHeight + Factor, () =>
                                {
                                    _cacheTranslateTransform.Y = SystemParameters.VirtualScreenHeight - _targetElement.ActualHeight;
                                    _hideStatus = HideState.None;
                                });
                            }
                            break;
                        case HideState.Left:
                            if (_curPosition.X <= Factor &&
                                 _curPosition.Y >= _cacheTranslateTransform.Y &&
                                 _curPosition.Y <= _cacheTranslateTransform.Y + _targetElement.ActualHeight)
                            {
                                _autoHideTimer.Stop();
                                AnimationTranslate(HideState.Right, _targetElement.ActualWidth + Factor, () =>
                                {
                                    _cacheTranslateTransform.X = 0;
                                    _hideStatus = HideState.None;
                                });
                            }
                            break;
                    }
                }
                _lockTimer = false;
            }
        }

        #endregion
    }
}
