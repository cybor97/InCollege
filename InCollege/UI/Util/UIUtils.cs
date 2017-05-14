using System.Windows;
using System.Windows.Media;

namespace InCollege.Client.UI.Util
{
    class UIUtils
    {
        public static T FindAncestorOrSelf<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                if (obj is T objTest)
                    return objTest;
                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }
    }
}
