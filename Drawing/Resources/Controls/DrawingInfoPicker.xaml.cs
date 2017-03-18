using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Point = System.Windows.Point;

namespace Drawing.Resources.Controls
{
    /// <summary>
    /// DrawingInfoPicker.xaml 的交互逻辑
    /// </summary>
    public partial class DrawingInfoPicker
    {
        #region Private fields

        private bool _leftSliderBlockCaptured;
        private bool _rightSliderBlockCaptured;
        private double _minRightSliderValue;
        private double _maxRightSliderValue;

        #endregion


        #region Constructor

        public DrawingInfoPicker()
        {
            InitializeComponent();
            _minRightSliderValue = 1.0;
            _maxRightSliderValue = 20.0;
            CurrentFont = new Font(new FontFamily("宋体"), 20);
            CommandBindings.Add(new CommandBinding(SelectColorCommand, SelectColorCommandExecute));
        }

        #endregion

        #region Public properties

        public bool SelectedMosaic => MosaicCheckBox.IsChecked.GetValueOrDefault();

        public bool SelectedLeftRadioBtn => LeftRadioButton.IsChecked.GetValueOrDefault();

        public Font CurrentFont { get;private set;}

        public double MinRightSliderValue => _minRightSliderValue;

        public double MaxRightSliderValue => _maxRightSliderValue;

        public object LeftRadioBtnContent {
            get { return LeftRadioButton.Content; }
            set { LeftRadioButton.Content = value; }
        }

        public object RightRadioBtnContent
        {
            get { return RightRadioButton.Content; }
            set { RightRadioButton.Content = value; }
        }

        public object RightSliderLblContent
        {
            get { return RightSliderLbl.Content; }
            set { RightSliderLbl.Content = value; }
        }

        public double RightSliderValue
        {
            get { return (double)GetValue(RightSliderValueProperty); }
            set
            {
                double moveX;
                if (value < _minRightSliderValue)
                {
                    SetValue(RightSliderValueProperty, _minRightSliderValue);
                    moveX = 0;
                }
                else if (value > _maxRightSliderValue)
                {
                    SetValue(RightSliderValueProperty, _maxRightSliderValue);
                    moveX = RightSliderBlock.Width;
                }
                else
                {
                    SetValue(RightSliderValueProperty, Math.Round(value, 1));
                    moveX = (value - _minRightSliderValue) * RightSliderBlock.Width / (_maxRightSliderValue - _minRightSliderValue);
                }

                RightSlider.Margin = new Thickness(moveX + RightSliderBlock.Margin.Left, RightSlider.Margin.Top, 0, 0);
            }
        }

        public static DependencyProperty RightSliderValueProperty = DependencyProperty.Register("RightSliderValue", typeof(double), typeof(DrawingInfoPicker), new PropertyMetadata(1.0));

        public byte LeftSliderValue
        {
            get { return (byte)GetValue(LeftSliderValueProperty); }
            set
            {
                SetValue(LeftSliderValueProperty, value);
                SetValue(CurrentArgbColorProperty, Color.FromArgb(value, CurrentColor.R, CurrentColor.G, CurrentColor.B));
                LeftSlider.Margin = new Thickness(value / 255.0 * LeftSliderBlock.Width + LeftSliderBlock.Margin.Left, LeftSlider.Margin.Top, 0, 0);
                ColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public static DependencyProperty LeftSliderValueProperty = DependencyProperty.Register("LeftSliderValue", typeof(byte), typeof(DrawingInfoPicker), new PropertyMetadata((byte)255));

        public Color CurrentColor
        {
            get { return (Color)GetValue(CurrentColorProperty); }
            set
            {
                SetValue(CurrentColorProperty, Color.FromArgb(255, value.R, value.G, value.B));
                SetValue(CurrentArgbColorProperty, Color.FromArgb(LeftSliderValue, value.R, value.G, value.B));
                ColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public static DependencyProperty CurrentColorProperty = DependencyProperty.Register("CurrentColor", typeof(Color), typeof(DrawingInfoPicker), new PropertyMetadata(Brushes.Red.Color));

        public Color CurrentArgbColor => (Color)GetValue(CurrentArgbColorProperty);

        public static DependencyProperty CurrentArgbColorProperty = DependencyProperty.Register("CurrentArgbColor", typeof(Color), typeof(DrawingInfoPicker), new PropertyMetadata(Brushes.Red.Color));

        public static RoutedUICommand SelectColorCommand = new RoutedUICommand("SelectColorCommand", "SelectColorCommand", typeof(DrawingInfoPicker));

        public event EventHandler FontChanged;

        public event EventHandler ColorChanged;

        public event EventHandler SelectedRadioChanged;

        #endregion

        #region Public methods

        public void SelectFontVisible(bool visible)
        {
            if (visible)
            {
                SelectFont.Visibility = Visibility.Visible;
                MosaicCheckBox.Visibility = Visibility.Hidden;
                RightRadioButton.Visibility = Visibility.Hidden;
                LeftRadioButton.Visibility = Visibility.Hidden;
            }
            else
            {
                SelectFont.Visibility = Visibility.Hidden;
                MosaicCheckBox.Visibility = Visibility.Visible;
                RightRadioButton.Visibility = Visibility.Visible;
                LeftRadioButton.Visibility = Visibility.Visible;
            }
        }

        public bool SetLeftSliderRange(double minValue, double maxValue)
        {
            if (minValue < maxValue)
            {
                _minRightSliderValue = Math.Round(minValue, 1);
                _maxRightSliderValue = Math.Round(maxValue, 1);
                SetRightSlider(new Point(RightSlider.Margin.Left - RightSliderBlock.Margin.Left, 0));
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Event

        private void SelectColorCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var convertColor = ColorConverter.ConvertFromString(e.Parameter.ToString());
            if (convertColor == null) return;
            CurrentColor = (Color) convertColor;
        }

        private void SelectColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new ColorDialog();
            if (dialog.ShowDialog() != DialogResult.OK) return;
            var color = dialog.Color;
            CurrentColor = Color.FromArgb(255, color.R, color.G, color.B);
        }

        private void SelectFont_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new FontDialog {Font = CurrentFont };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            CurrentFont = dialog.Font;
            FontChanged?.Invoke(this, EventArgs.Empty);
        }

        private void LeftSliderBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _leftSliderBlockCaptured = LeftSliderBlock.CaptureMouse();
            SetLeftSlider(e.GetPosition(LeftSliderBlock));
        }

        private void LeftSliderBlock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_leftSliderBlockCaptured || e.LeftButton != MouseButtonState.Pressed) return;
            SetLeftSlider(e.GetPosition(LeftSliderBlock));
        }

        private void LeftSliderBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_leftSliderBlockCaptured) return;
            LeftSliderBlock.ReleaseMouseCapture();
            _leftSliderBlockCaptured = false;
        }

        private void RightSliderBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _rightSliderBlockCaptured = RightSliderBlock.CaptureMouse();
            SetRightSlider(e.GetPosition(RightSliderBlock));
        }

        private void RightSliderBlock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_rightSliderBlockCaptured || e.LeftButton != MouseButtonState.Pressed) return;
            SetRightSlider(e.GetPosition(RightSliderBlock));
        }

        private void RightSliderBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(!_rightSliderBlockCaptured)return;
            RightSliderBlock.ReleaseMouseCapture();
            _rightSliderBlockCaptured = false;
        }

        private void RightRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SelectedRadioChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RightRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            SelectedRadioChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Private methods

        private void SetLeftSlider(Point position)
        {
            if (position.X <= 0)
            {
                LeftSliderValue = 0;
            }
            else if (position.X >= LeftSliderBlock.Width)
            {
                LeftSliderValue = 255;
            }
            else
            {
                LeftSliderValue = (byte)(position.X / LeftSliderBlock.Width * 255);
            }
        }

        private void SetRightSlider(Point position)
        {
            RightSliderValue = Math.Round(position.X / RightSliderBlock.Width * (_maxRightSliderValue - _minRightSliderValue) + _minRightSliderValue, 1);
        }

        #endregion

       
    }
}
