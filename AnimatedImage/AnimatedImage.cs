using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AnimatedImage
{

    /// <summary>
    /// 动态的图像
    /// </summary>
    public class AnimatedImage : Image
    {
        ///// <summary>
        ///// 开始动画
        ///// </summary>
        //public void StartAnimation()
        //{
        //    if (_timer == null || IsAnimationWorking) return;
        //    IsAnimationWorking = true;
        //    _timer.Start();
        //}

        ///// <summary>
        ///// 停止动画
        ///// </summary>
        //public void StopAnimation()
        //{
        //    IsAnimationWorking = false;
        //}

        ///// <summary>
        ///// 是否正在进行动画
        ///// </summary>
        //public bool IsAnimationWorking { get; private set; }

        /// <summary>
        /// 当前图像帧
        /// </summary>
        public ImageSource CurrentImageFrame => base.Source;

        /// <summary>
        /// 图像的 ImageSource 发生改变时，执行相应操作
        /// </summary>
        /// <param name="args">依赖属性的参数</param>
        protected virtual void OnSourceChanged(DependencyPropertyChangedEventArgs args)
        {
            base.Source = args.NewValue as ImageSource;
            ImageBehavior.SetAnimatedSource(this, base.Source);
        }

        private static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as AnimatedImage)?.OnSourceChanged(args);
        }

        /// <summary>
        /// 获取或设置图像的 ImageSource
        /// </summary>
        public new ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Source 的依赖属性
        /// </summary>
        public new static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
                "Source",
                typeof(ImageSource),
                typeof(AnimatedImage),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnSourceChanged));

    }
}
