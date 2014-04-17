using Recommender.Common;
using Recommender.Data;
using Recommender.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls;

// The Hub Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=321224

namespace Recommender
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public HubPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
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
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var reDataGroup = await ReDataSource.GetGroupAsync("Guess");            
            if (reDataGroup == null)
            {
                InternetFailureAlarm();
            }
            else
            {
                this.DefaultViewModel["GuessItems"] = reDataGroup;
            }
        }

        /// <summary>
        /// Invoked when a HubSection header is clicked.
        /// </summary>
        /// <param name="sender">The Hub that contains the HubSection whose header was clicked.</param>
        /// <param name="e">Event data that describes how the click was initiated.</param>
        void Hub_SectionHeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            HubSection section = e.Section;
            var group = section.DataContext;
            this.Frame.Navigate(typeof(SectionPage), ((ReDataGroup)group).UniqueId);
        }

        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>
        /// <param name="sender">The GridView or ListView
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((ReDataItem)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(ItemPage), itemId);
        }

        private async void ReGuess_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            ((CascadingImageControl)sender).Cascade();
            var reDataGroup = await ReDataSource.GetGroupAsync("Guess");
            if (reDataGroup == null)
            {
                InternetFailureAlarm();
            }
            else
            {
                this.DefaultViewModel["GuessItems"] = reDataGroup;
            }
        }

        private async void ReGuessBtn_Click(object sender, RoutedEventArgs e)
        {
            var reDataGroup = await ReDataSource.GetGroupAsync("Guess");
            if (reDataGroup == null)
            {
                InternetFailureAlarm();
            }
            else
            {
                this.DefaultViewModel["GuessItems"] = reDataGroup;
            }
        }

        void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SearchPage));
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                SettingsFlyoutAbout sfa = new SettingsFlyoutAbout();
                sfa.ShowIndependent();
            }
        }

        private void SettingBtn_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                SettingsFlyoutPrivacyPolicy sfpp = new SettingsFlyoutPrivacyPolicy();
                sfpp.ShowIndependent();
            }
        }

        private void TopicsBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(TopicsPage));
        }

        private async void InternetFailureAlarm()
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string connectionFailureAlarm = loader.GetString("ConnectionFailureAlarm");
            string confirmStr = loader.GetString("Confirm");

            // Create the message dialog and set its content
            var messageDialog = new MessageDialog(connectionFailureAlarm);

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
            messageDialog.Commands.Add(new UICommand(confirmStr, new UICommandInvokedHandler(this.CommandInvokedHandler)));

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 0;

            // Show the message dialog
            await messageDialog.ShowAsync();
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            //
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.NavigationRoadmap.SetTo((int)App.NavaigationPages.MainPage);
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App.NavigationRoadmap.SetFrom((int)App.NavaigationPages.MainPage);
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
