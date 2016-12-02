using System.Windows.Controls;

namespace EasyImage
{
    public class ImageControl : UserControl
    {
        public ControlManager<ImageControl> ControlManager { get; }

        public ImageControl(ControlManager<ImageControl> controlManager)
        {
            ControlManager = controlManager;
        }
    }
}
