﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace AnimatedImage
{

    /// <summary>
    /// 动态的图像
    /// </summary>
    public class AnimatedImage : Image
    {
        /// <summary>
        /// 是否为动态图像
        /// </summary>
        public bool Animatable => Tag != null;

        /// <summary>
        /// 获取所有位图帧的集合
        /// </summary>
        public List<BitmapSource> BitmapFrames
        {
            get
            {
                var animation = Tag as ObjectAnimationUsingKeyFrames;
                var bitmapSources = new List<BitmapSource>();
                if (animation == null)
                {
                    if (base.Source != null)
                    {
                        var bitmapSource = base.Source as BitmapSource;
                        if (bitmapSource != null)
                        {
                            bitmapSources.Add(bitmapSource);
                        }
                    }
                }
                else
                {
                    bitmapSources.AddRange((from ObjectKeyFrame item in animation.KeyFrames select item.Value).OfType<BitmapSource>());
                    if (bitmapSources.Count > 1)
                    {
                        bitmapSources.Reverse();
                    }
                }
                return bitmapSources;
            }
        }

        /// <summary>
        /// 图像的 ImageSource 发生改变时，执行相应操作
        /// </summary>
        /// <param name="args">依赖属性的改变参数</param>
        protected virtual void OnSourceChanged(DependencyPropertyChangedEventArgs args)
        {
            base.Source = args.NewValue as ImageSource;
            ImageBehavior.SetAnimatedSource(this, base.Source);
           
        }

        /// <summary>
        /// 图像的 ImageSource 发生改变时，执行相应操作
        /// </summary>
        /// <param name="obj">依赖对象</param>
        /// <param name="args">依赖属性的改变参数</param>
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
