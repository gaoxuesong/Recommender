using Recommender.Common;
using Recommender.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Recommender
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TopicSplitPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public TopicSplitPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            String para = (String)e.NavigationParameter;
            if (!String.IsNullOrWhiteSpace(para))
            {
                var group = await ReTopicsDataSource.GetGroupAsync((String)e.NavigationParameter);
                this.DefaultViewModel["Group"] = group;
                this.DefaultViewModel["Items"] = group.Items;
            }

            if (e.PageState == null)
            {
                this.itemListView.SelectedItem = null;
                // When this is a new page, select the first item automatically unless logical page
                // navigation is being used (see the logical page navigation #region below.)
                if (this.itemsViewSource.View != null)
                {
                    this.itemsViewSource.View.MoveCurrentToFirst();
                }
            }
            else
            {
                // Restore the previously saved state associated with this page
                if (e.PageState.ContainsKey("SelectedItem") && this.itemsViewSource.View != null)
                {
                    var selectedItem = await ReTopicsDataSource.GetContentsAsync((String)e.PageState["SelectedItem"]);
                    this.itemsViewSource.View.MoveCurrentTo(selectedItem);
                }
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        /// <summary>
        /// Invoked when an item within the list is selected.
        /// </summary>
        /// <param name="sender">The GridView displaying the selected item.</param>
        /// <param name="e">Event data that describes how the selection was changed.</param>
        private async void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var itemId = ((ReDataItem)send).UniqueId;
            //var contents = await ReTopicsDataSource.GetContentsAsync();
            //this.DefaultViewModel["SelectedContents"] = 
            var listView = (ListView)sender;
            var seletedItem = (ReDataItem)listView.SelectedItem;
            if (seletedItem != null)
            {
                var seletedId = seletedItem.UniqueId;
                if (!String.IsNullOrWhiteSpace(seletedId))
                {
                    var contents = await ReTopicsDataSource.GetContentsAsync(seletedId);
                    this.DefaultViewModel["SelectedContents"] = contents.Items;
                }
                CategoryPivot.SelectedIndex = 1;
            }
        }

        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>
        /// <param name="sender">The GridView or ListView
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void SelectedItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((ReDataItem)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(ItemPage), itemId);
        }   

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.NavigationRoadmap.SetTo((int)App.NavaigationPages.TopicDetailPage);
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App.NavigationRoadmap.SetFrom((int)App.NavaigationPages.TopicDetailPage);
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
