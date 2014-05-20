using Recommender.DataModel;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Recommender
{
    class PhoneVariableSizedGridView : GridView
    {
        private int rowVal;
        private int colVal;
        private List<Size> _sequence;
        private const int VirtualGroupMembers = 16;

        public PhoneVariableSizedGridView()
        {
            _sequence = new List<Size> { 
                LayoutSizes.PrimaryItem, LayoutSizes.WideItem, LayoutSizes.NormalItem, LayoutSizes.NormalItem, LayoutSizes.NormalItem, LayoutSizes.NormalItem,
                LayoutSizes.PrimaryItem, LayoutSizes.NormalItem, LayoutSizes.NormalItem, LayoutSizes.WideItem, LayoutSizes.NormalItem, LayoutSizes.NormalItem,
                LayoutSizes.PrimaryItem, LayoutSizes.RectangleItem, LayoutSizes.RectangleItem, LayoutSizes.WideItem
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
}
