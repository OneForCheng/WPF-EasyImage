using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace WindowTemplate
{
    public enum ClickResult
    {
        LeftBtn,
        MiddleBtn,
        RightBtn,
        Close,
    }

    public enum MessageBoxMode
    {
        SingleMode,
        DoubleMode,
        ThreeMode,
    }

    public class Message
    {
        
        public ClickResult Result;
        public string Content { get; }
        public MessageBoxMode MessageBoxMode { get; }

        public Message(string content, MessageBoxMode  mode)
        {
            Content = content;
            MessageBoxMode = mode;
        }

    }

    /// <summary>
    /// MessageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageWindow
    {
        #region Field and Property
        private readonly Message _message;
        private Storyboard _storyboard;//故事面板

        public string LeftBtnContent
        {
            set
            {
                switch (_message.MessageBoxMode)
                {
                    case MessageBoxMode.DoubleMode:
                        DoubleModeLeftBtn.Content = value;
                        break;
                    case MessageBoxMode.ThreeMode:
                        ThreeModeLeftBtn.Content = value;
                        break;
                }
            }
        }

        public string MiddleBtnContent
        {
            set
            {
                switch (_message.MessageBoxMode)
                {
                    case MessageBoxMode.SingleMode:
                        SingleModeBtn.Content = value;
                        break;
                    case MessageBoxMode.ThreeMode:
                        ThreeModeMiddleBtn.Content = value;
                        break;
                }
            }
        }

        public string RightBtnContent
        {
            set
            {
                switch (_message.MessageBoxMode)
                {
                    case MessageBoxMode.DoubleMode:
                        DoubleModeRightBtn.Content = value;
                        break;
                    case MessageBoxMode.ThreeMode:
                        ThreeModeRightBtn.Content = value;
                        break;
                }
            }
        }

        #endregion


        #region Constructor

        public MessageWindow(Message message)
        {
            InitializeComponent();
            _message = message;
            switch (message.MessageBoxMode)
            {
                case MessageBoxMode.SingleMode:
                    SingleMode.Visibility = Visibility.Visible;
                    break;
                case MessageBoxMode.DoubleMode:
                    DoubleMode.Visibility = Visibility.Visible;
                    break;
                case MessageBoxMode.ThreeMode:
                    ThreeMode.Visibility = Visibility.Visible;
                    break;
            }
            MessageTbx.Text = message.Content;
        }

        #endregion

        #region Method and Event
        private void MessageWin_Loaded(object sender, RoutedEventArgs e)
        {
            this.RemoveSystemMenuItems(Win32.SystemMenuItems.All); //去除窗口指定的系统菜单
        }

        private void MessageWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Y)
            {
                if (SingleMode.Visibility == Visibility.Visible)
                {
                    MiddleBtn_Click(null, null);
                }
                else
                {
                    LeftBtn_Click(null, null);
                }
            }
            else if (e.Key == Key.N)
            {
                if (DoubleMode.Visibility == Visibility.Visible)
                {
                    RightBtn_Click(null, null);
                }
                else if (ThreeMode.Visibility == Visibility.Visible)
                {
                    MiddleBtn_Click(null, null);
                }
            }
        }

        private void TitleLbl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            _message.Result = ClickResult.Close;
            Close();
        }

        private void LeftBtn_Click(object sender, RoutedEventArgs e)
        {
            _message.Result = ClickResult.LeftBtn;
            Close();
        }

        private void MiddleBtn_Click(object sender, RoutedEventArgs e)
        {
            _message.Result = ClickResult.MiddleBtn;
            Close();
        }

        private void RightBtn_Click(object sender, RoutedEventArgs e)
        {
            _message.Result = ClickResult.RightBtn;
            Close();
        }

        public void SetPropertyAnimation(DependencyProperty property, DoubleAnimation animation, Action completedEvent = null)
        {
            //设置动画时段
            Storyboard.SetTargetName(animation, "DynamicByProperty");
            Storyboard.SetTargetProperty(animation, new PropertyPath(property));

            MouseEnter += delegate
            {
                _storyboard?.Pause(this);
            };
            MouseLeave += delegate
            {
                _storyboard?.Resume(this);
            };

            //创建Storyboard
            if (_storyboard == null)
            {
                _storyboard = new Storyboard();
            }
            _storyboard.FillBehavior = FillBehavior.Stop;
            _storyboard.Completed += delegate
            {
                completedEvent?.Invoke();
            };
            _storyboard.Children.Add(animation);
            RegisterName("DynamicByProperty", this);
            _storyboard.Begin(this, true);
        }

        public void SetOpacityAnimation(DoubleAnimation animation, Action completedEvent = null)
        {
            //设置动画时段
            Storyboard.SetTargetName(animation, "DynamicByProperty");
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));

            MouseEnter += delegate
            {
                _storyboard?.Stop(this);
                Opacity = 1;
            };
            MouseLeave += delegate
            {
                _storyboard?.Begin(this, true);
            };

            //创建Storyboard
            if (_storyboard == null)
            {
                _storyboard = new Storyboard();
            }
            _storyboard.FillBehavior = FillBehavior.Stop;
            _storyboard.Completed += delegate
            {
                completedEvent?.Invoke();
            };
            _storyboard.Children.Add(animation);
            RegisterName("DynamicByProperty", this);
            _storyboard.Begin(this, true);
        }

        #endregion

       
    }
}
