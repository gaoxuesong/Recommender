using Recommender.Common;
using Recommender.Data;
using Recommender.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
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


// The Item Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace Recommender
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class ItemPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private string gMediaId = "";

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

        public ItemPage()
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
            gMediaId = e.NavigationParameter as string;
            var group = await ReDetailDataSource.GetWebServiceDetailAsync(gMediaId);
            if (group == null)
            {
                InternetFailureAlarm();
            }
            else
            {
                this.DefaultViewModel["Group"] = group;
                this.DefaultViewModel["Items"] = group.Items;
            }
        }

        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        /// <param name="sender">The GridView displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((ReDataItem)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(ItemPage), itemId);
        }

        private async void Button_Click_Play(object sender, RoutedEventArgs e)
        {
            var sendBtn = sender as Button;
            string playUrl = sendBtn.CommandParameter.ToString();

            if (!string.IsNullOrEmpty(gMediaId))
            {
                await ReUserActionDataSource.SetUserAction(gMediaId);
            }

            if (!String.IsNullOrEmpty(playUrl))
            {
                string tempPlayUrl = playUrl;
                String specialUrlStarts = "baidu://";
                String specialUrlEnds = ".m3u8";

                // for download resource
                if (tempPlayUrl.StartsWith(specialUrlStarts, StringComparison.OrdinalIgnoreCase))
                {
                    String switchUrl = "http://replatform.cloudapp.net:8000/getplay?id={0}&v={1}";
                    playUrl = String.Format(switchUrl, gMediaId, tempPlayUrl);                    

                    string realUrl = await ReDetailDataSource.GetHLSPlayUrlAsync(playUrl);
                    playUrl = realUrl;                    
                }
                // for .m3u8 resource
                // in the future maybe play the media in my app
                else if (tempPlayUrl.EndsWith(specialUrlEnds, StringComparison.OrdinalIgnoreCase)) 
                {
                    String switchUrl = "http://replatform.cloudapp.net:8000/getplay?id={0}&v={1}";
                    playUrl = String.Format(switchUrl, gMediaId, tempPlayUrl);

                    string realUrl = await ReDetailDataSource.GetHLSPlayUrlAsync(playUrl);
                    playUrl = realUrl;
                }

                // Create the URI to launch from a string.
                Uri resourceUri;
                if (!Uri.TryCreate(playUrl.Trim(), UriKind.Absolute, out resourceUri))
                {
                    return;
                }
                // Launch the URI.
                LaunchIE(resourceUri);
            }
        }

        private async void LaunchIE(Uri uri)
        {
            // Set the option to show a warning
            var options = new Windows.System.LauncherOptions();
            options.TreatAsUntrusted = true;

            // Launch the URI with a warning prompt
            var success = await Windows.System.Launcher.LaunchUriAsync(uri, options);

            if (success)
            {
                // URI launched
            }
            else
            {
                // URI launch failed
            }
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
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}