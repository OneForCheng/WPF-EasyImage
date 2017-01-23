using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Point = System.Windows.Point;

namespace Drawing.Resources.Controls
{
    /// <summary>
    /// ColorPicker.xaml 的交互逻辑
    /// </summary>
    public partial class DrawingInfoPicker
    {
        #region Private fields

        private bool _leftSliderBlockCaptured;
        private bool _rightSliderBlockCaptured;


        #endregion


        #region Constructor

        public DrawingInfoPicker()
        {
            InitializeComponent();
            MinRightSliderValue = 1.0;
            MaxRightSliderValue = 20.0;
            CurrentFont = new Font(new System.Drawing.FontFamily("宋体"), 20);
            CommandBindings.Add(new CommandBinding(SelectColorCommand, SelectColorCommandExecute));
        }

        #endregion

        #region Public properties

        public bool SelectedMosaic => MosaicCheckBox.IsChecked.GetValueOrDefault();

        public bool SelectedLeftBtn => LeftRadioButton.IsChecked.GetValueOrDefault();

        public Font CurrentFont { get;private set;}

        public double MinRightSliderValue { get; set; }

        public double MaxRightSliderValue { get; set; }

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
            set { SetValue(RightSliderValueProperty, value); }
        }

        public static DependencyProperty RightSliderValueProperty = DependencyProperty.Register("RightSliderValue", typeof(double), typeof(DrawingInfoPicker), new PropertyMetadata(1.0));

        public byte LeftSliderValue
        {
            get { return (byte)GetValue(LeftSliderValueProperty); }
            set { SetValue(LeftSliderValueProperty, value); }
        }

        public static DependencyProperty LeftSliderValueProperty = DependencyProperty.Register("LeftSliderValue", typeof(byte), typeof(DrawingInfoPicker), new PropertyMetadata((byte)255));


        public SolidColorBrush CurrentColor
        {
            get { return (SolidColorBrush)GetValue(CurrentColorProperty); }
            set { SetValue(CurrentColorProperty, value); }
        }

        public static DependencyProperty CurrentColorProperty = DependencyProperty.Register("CurrentColor", typeof(SolidColorBrush), typeof(DrawingInfoPicker), new PropertyMetadata(Brushes.Red));

        public SolidColorBrush CurrentArgbColor
        {
            get { return (SolidColorBrush)GetValue(CurrentArgbColorProperty); }
            set
            {
                SetValue(CurrentArgbColorProperty, value);
                ColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public static DependencyProperty CurrentArgbColorProperty = DependencyProperty.Register("CurrentArgbColor", typeof(SolidColorBrush), typeof(DrawingInfoPicker), new PropertyMetadata(Brushes.Red));

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

        public void UpdateLeftSliderValue()
        {
            SetRightSlider(new Point(RightSlider.Margin.Left - RightSliderBlock.Margin.Left, 0));
        }

        #endregion

        #region Event

        private void SelectColorCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var convertColor = ColorConverter.ConvertFromString(e.Parameter.ToString());
            if (convertColor == null) return;
            var color = (Color) convertColor;
            CurrentColor = new SolidColorBrush(color);
            CurrentArgbColor = new SolidColorBrush(Color.FromArgb(LeftSliderValue, color.R, color.G, color.B));
        }

        private void SelectColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new ColorDialog();
            if (dialog.ShowDialog() != DialogResult.OK) return;
            var color = dialog.Color;
            CurrentColor = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
            CurrentArgbColor = new SolidColorBrush(Color.FromArgb(LeftSliderValue, color.R, color.G, color.B));
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
            double moveX;
            if (position.X <= 0)
            {
                LeftSliderValue = 0;
                moveX = 0;
            }
            else if (position.X >= LeftSliderBlock.Width)
            {
                LeftSliderValue = 255;
                moveX = LeftSliderBlock.Width;
            }
            else
            {
                LeftSliderValue = (byte)(position.X / LeftSliderBlock.Width * 255);
                moveX = position.X;
            }

            var color = CurrentArgbColor.Color;
            CurrentArgbColor = new SolidColorBrush(Color.FromArgb(LeftSliderValue, color.R, color.G, color.B));
            LeftSlider.Margin = new Thickness(moveX + LeftSliderBlock.Margin.Left, LeftSlider.Margin.Top, 0, 0);
        }

        private void SetRightSlider(Point position)
        {
            double moveX;
            if (position.X <= 0)
            {
                RightSliderValue = Math.Round(MinRightSliderValue, 1);
                moveX = 0;
            }
            else if (position.X >= RightSliderBlock.Width)
            {
                RightSliderValue = Math.Round(MaxRightSliderValue, 1);
                moveX = RightSliderBlock.Width;
            }
            else
            {
                RightSliderValue = Math.Round(position.X / RightSliderBlock.Width * MaxRightSliderValue, 1);
                moveX = position.X;
            }

            RightSlider.Margin = new Thickness(moveX + RightSliderBlock.Margin.Left, RightSlider.Margin.Top, 0, 0);
        }

        #endregion

       
    }
}
