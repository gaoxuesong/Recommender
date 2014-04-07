using Recommender.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Recommender
{
    public class VariableSizedGridView : GridView
{
        private int rowVal;
        private int colVal;
        private List<Size> _sequence;
        private const int VirtualGroupMembers = 20;

        public VariableSizedGridView()
        {
            _sequence = new List<Size> { 
                LayoutSizes.PrimaryItem, LayoutSizes.NormalItem, LayoutSizes.NormalItem, 
                LayoutSizes.NormalItem, LayoutSizes.NormalItem, LayoutSizes.NormalItem, LayoutSizes.NormalItem, LayoutSizes.NormalItem, LayoutSizes.NormalItem, 
                LayoutSizes.SecondWideItem, LayoutSizes.PrimaryItem
            };
        }

        protected override void PrepareContainerForItemOverride(Windows.UI.Xaml.DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            ReDataItem dataItem = item as ReDataItem;
            int index =-1;

            if (dataItem != null)
            {
                index = dataItem.IndexInGroup % VirtualGroupMembers;
            }

            if (index >= 0 && index < _sequence.Count)
            {
                colVal = (int)_sequence[index].Width;
                rowVal = (int)_sequence[index].Height;
            }
            else
            {
                colVal = (int)LayoutSizes.OtherSmallItem.Width;
                rowVal = (int)LayoutSizes.OtherSmallItem.Height;
            }
            VariableSizedWrapGrid.SetRowSpan(element as UIElement, rowVal);
            VariableSizedWrapGrid.SetColumnSpan(element as UIElement, colVal);
        }
    }

    public static class LayoutSizes {
        public static Size PrimaryItem = new Size(2, 2);
        public static Size SecondWideItem = new Size(2, 1);
        public static Size NormalItem = new Size(1, 1);
        public static Size OtherSmallItem = new Size(1, 1);
    }
}
