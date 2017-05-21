using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InCollege.Client.UI.Base.Styles
{
    public class WindowButton : Button
    {
        public static DependencyProperty AnimateColorProperty = DependencyProperty.Register("AnimateColor", typeof(Color?), typeof(WindowButton),
            new FrameworkPropertyMetadata(Colors.Black,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public Color? AnimateColor
        {
            get
            {
                return (Color?)GetValue(AnimateColorProperty);
            }

            set
            {
                SetValue(AnimateColorProperty, value);
            }
        }
    }
}
