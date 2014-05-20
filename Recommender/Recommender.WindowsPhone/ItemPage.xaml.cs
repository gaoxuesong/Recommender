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
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace Recommender
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class ItemPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private string gMediaId = "";

        public ItemPage()
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
        /// Gets the view model for this <see cref="Page"/>. This can be changed to a strongly typed view model.
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
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            gMediaId = e.NavigationParameter as string;
            var group = await ReDetailDataSource.GetWebServiceDetailAsync(gMediaId);
            if (group == null)
            {
                Helpers.InternetFailureAlarm();
            }
            else
            {
                this.DefaultViewModel["Group"] = group;
                this.DefaultViewModel["Items"] = group.Items;
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
            // TODO: Save the unique state of the page here.
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
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.NavigationRoadmap.SetTo((int)App.NavaigationPages.ItemPage);
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App.NavigationRoadmap.SetFrom((int)App.NavaigationPages.ItemPage);
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

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

                Random random = new Random();
                int r = random.Next();

                // for download resource
                if (tempPlayUrl.StartsWith(specialUrlStarts, StringComparison.OrdinalIgnoreCase))
                {
                    String switchUrl = "http://replatform.cloudapp.net:8000/getplay?id={0}&v={1}&uuid={2}&type={3}&from={4}&to={5}&r={6}";
                    playUrl = String.Format(switchUrl, gMediaId, tempPlayUrl, App.gDeviceName, App.gDeviceType, App.NavigationRoadmap.GetFrom(), App.NavigationRoadmap.GetTo(), r);

                    string realUrl = await ReDetailDataSource.GetHLSPlayUrlAsync(playUrl);
                    playUrl = realUrl;
                }
                // for .m3u8 resource
                // in the future maybe play the media in my app
                else if (tempPlayUrl.EndsWith(specialUrlEnds, StringComparison.OrdinalIgnoreCase))
                {
                    String switchUrl = "http://replatform.cloudapp.net:8000/getplay?id={0}&v={1}&uuid={2}&type={3}&from={4}&to={5}&r={6}";
                    playUrl = String.Format(switchUrl, gMediaId, tempPlayUrl, App.gDeviceName, App.gDeviceType, App.NavigationRoadmap.GetFrom(), App.NavigationRoadmap.GetTo(), r);

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
    }
}