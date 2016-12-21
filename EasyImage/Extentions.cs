using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WindowTemplate;

namespace EasyImage
{
    public static class Extentions
    {
        /// <summary>
        /// 从指定UI元素中获取指定位置转换信息
        /// </summary>
        /// <typeparam name="T">位置转换信息</typeparam>
        /// <param name="element">UI元素</param>
        /// <returns></returns>
        public static T GetTransform<T>(this UIElement element) where T : Transform, new()
        {
            var transform = element.RenderTransform;
            var targetTransform = transform as T;
            if (targetTransform != null)
            {
                return targetTransform;
            }
            else
            {
                var group = transform as TransformGroup;
                if (group != null)
                {
                    var count = group.Children.Count;
                    for (var i = count - 1; i >= 0; i--)
                    {
                        targetTransform = group.Children[i] as T;
                        if (targetTransform != null)
                        {
                            break;
                        }
                    }
                    if (targetTransform != null) return targetTransform;
                    targetTransform = new T();
                    group.Children.Add(targetTransform);
                    return targetTransform;
                }
                else
                {
                    group = new TransformGroup();
                    if (transform != null)
                    {
                        group.Children.Add(transform);
                    }
                    targetTransform = new T();
                    group.Children.Add(targetTransform);
                    element.RenderTransform = group;

                    return targetTransform;
                }
            }
        }

        public static void ShowMessageBox(string content)
        {
            var msgWin = new MessageWindow(new Message(content, MessageBoxMode.SingleMode))
            {
                //Owner = Owner,
                MiddleBtnContent = "确定",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            msgWin.SetOpacityAnimation(new DoubleAnimation(1, 0.1, new Duration(TimeSpan.FromSeconds(10))), msgWin.Close);
            msgWin.ShowDialog();
        }

        public static BitmapImage GetBitmapImage(this BitmapSource bitmapSource)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            var stream = new MemoryStream();
            encoder.Save(stream);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        public static BitmapImage GetBitmapImage(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            var stream = new MemoryStream();
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(stream);
            }
            stream.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            return bitmapImage;
        }

    }
}
