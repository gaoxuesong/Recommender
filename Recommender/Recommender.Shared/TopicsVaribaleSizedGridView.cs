using Recommender.DataModel;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Recommender
{
    public class TopicsVaribaleSizedGridView : GridView
    {
        private int rowVal;
        private int colVal;
        private List<Size> _sequence;
        private const int VirtualGroupMembers = 5;
        private static int index = 0;
        public TopicsVaribaleSizedGridView()
        {
            _sequence = new List<Size> { 
                LayoutSizes.PrimaryItem, 
                LayoutSizes.WideItem, LayoutSizes.NormalItem, LayoutSizes.NormalItem,
                LayoutSizes.PrimaryItem
            };

            index = 0;
        }

        protected override void PrepareContainerForItemOverride(Windows.UI.Xaml.DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            ReDataGroup dataItem = item as ReDataGroup;

            if (dataItem != null)
            {
                index = index % VirtualGroupMembers;
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

            index++;     
        }
    }
}