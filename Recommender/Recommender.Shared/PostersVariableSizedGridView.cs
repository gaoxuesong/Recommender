using Recommender.DataModel;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Recommender
{
    public class PostersVariableSizedGridView : GridView
    {
        private int rowVal;
        private int colVal;
        private List<Size> _sequence;

        public PostersVariableSizedGridView()
        {
            _sequence = new List<Size> { 
                LayoutSizes.PrimaryItem, LayoutSizes.WideItem,
                LayoutSizes.WideItem, LayoutSizes.RectangleItem, LayoutSizes.RectangleItem
            };
        }

        protected override void PrepareContainerForItemOverride(Windows.UI.Xaml.DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            ReDataItem dataItem = item as ReDataItem;
            int index =-1;

            if (dataItem != null)
            {
                index = dataItem.IndexInGroup;
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
