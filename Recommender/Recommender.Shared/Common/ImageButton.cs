using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Telerik.UI.Xaml.Controls.Primitives;
//using Telerik.UI.Xaml.Controls.Primitives.Menu;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Recommender.Common
{
    public class ImageButton : Button
    {
        /// <summary>
        /// Identifies the <see cref="NormalImage"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NormalImageProperty =
            DependencyProperty.Register("NormalImage", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="HoverImage"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HoverImageProperty =
            DependencyProperty.Register("HoverImage", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(null));

        public ImageSource NormalImage
        {
            get
            {
                return (ImageSource)this.GetValue(NormalImageProperty);
            }
            set
            {
                this.SetValue(NormalImageProperty, value);
            }
        }

        public ImageSource HoverImage
        {
            get
            {
                return (ImageSource)this.GetValue(HoverImageProperty);
            }
            set
            {
                this.SetValue(HoverImageProperty, value);
            }
        }

        public ImageButton()
        {
            this.DefaultStyleKey = typeof(ImageButton);
        }
    }
}
