using Recommender.Common;
using Recommender.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Recommender
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private HttpClient httpClient;
        private Task<string> currentHttpTask;

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

        public SearchPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;

            //httpClient = new HttpClient();
        }

        ~SearchPage()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
                httpClient = null;
            }
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
            var reDataGroup = await RePopularDataSource.GetGroupAsync("Popular");
            if (reDataGroup == null)
            {
                Helpers.InternetFailureAlarm();
            }
            else
            {
                this.DefaultViewModel["SearchItems"] = reDataGroup;
            }
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

        ///// <summary>
        ///// Completes the retreval of suggestions from the web service
        ///// </summary>
        ///// <param name="str">User query string</param>
        ///// <param name="suggestions">Suggestions list to append new suggestions</param>
        ///// <returns></returns>
        //private async Task GetSuggestionsAsync(string str, SearchSuggestionCollection suggestions)
        //{
        //    // Cancel the previous suggestion request if it is not finished.
        //    if (currentHttpTask != null)
        //    {
        //        currentHttpTask.AsAsyncOperation<string>().Cancel();
        //    }

        //    // Get the suggestions from an open search service.
        //    currentHttpTask = httpClient.GetStringAsync(str);
        //    string response = await currentHttpTask;

        //    JsonObject jsonObject = JsonObject.Parse(response);
        //    jsonObject.ContainsKey("data");
        //    String test1 = jsonObject["data"].GetString();

        //    string[] test = test1.Split('|');

        //    foreach (string value in test)
        //    {
        //        suggestions.AppendQuerySuggestion(value);
        //    }
        //}

        ///// <summary>
        ///// Populates SearchBox with Suggestions when user enters text
        ///// </summary>
        ///// <param name="sender">Xaml SearchBox</param>
        ///// <param name="e">Event when user changes text in SearchBox</param>
        //private async void SearchBoxEventsSuggestionsRequested(Object sender, SearchBoxSuggestionsRequestedEventArgs e)
        //{
        //    var queryText = e.QueryText;
        //    if (string.IsNullOrEmpty(queryText))
        //    {
        //        //MainPage.Current.NotifyUser("Use the search control to submit a query", NotifyType.StatusMessage);
        //    }
        //    else
        //    {
        //        Random random = new Random();
        //        int r = random.Next();
        //        string key = System.Net.WebUtility.UrlEncode(queryText);
        //        string requestUrl = "http://replatform.cloudapp.net:8000/getsearchrec?v={0}&uuid={1}&type={2}&from={3}&to={4}&r={5}";
        //        requestUrl = String.Format(requestUrl, key, App.gDeviceName, App.gDeviceType, App.NavigationRoadmap.GetFrom(), App.NavigationRoadmap.GetTo(), r);

        //        // The deferral object is used to supply suggestions asynchronously for example when fetching suggestions from a web service.
        //        var request = e.Request;
        //        var deferral = request.GetDeferral();

        //        try
        //        {
        //            Task task = GetSuggestionsAsync(requestUrl, request.SearchSuggestionCollection);
        //            await task;

        //            if (task.Status == TaskStatus.RanToCompletion)
        //            {
        //                if (request.SearchSuggestionCollection.Size > 0)
        //                {
        //                    //
        //                }
        //                else
        //                {
        //                    //
        //                }
        //            }
        //        }
        //        catch (TaskCanceledException)
        //        {
        //            // We have canceled the task.
        //        }
        //        catch (FormatException)
        //        {
        //            //
        //        }
        //        catch (Exception)
        //        {
        //            //
        //        }
        //        finally
        //        {
        //            deferral.Complete();
        //        }
        //    }
        //}

        ///// <summary>
        ///// Called when query submitted in SearchBox
        ///// </summary>
        ///// <param name="sender">The Xaml SearchBox</param>
        ///// <param name="e">Event when user submits query</param>
        //private async void SearchBoxEventsQuerySubmitted(object sender, SearchBoxQuerySubmittedEventArgs e)
        //{
        //    //MainPage.Current.NotifyUser(e.QueryText, NotifyType.StatusMessage);
        //    string key = e.QueryText;
        //    if ((e.QueryText != null) && !(e.QueryText.Equals(string.Empty)))
        //    {
        //        var searchGroup = await ReSearchDataSource.GetSearchGroupAsync("search", key);
        //        if (searchGroup == null)
        //        {
        //            Helpers.InternetFailureAlarm();
        //        }
        //        else
        //        {
        //            this.DefaultViewModel["SearchItems"] = searchGroup;
        //        }
        //    }
        //}

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
            App.NavigationRoadmap.SetTo((int)App.NavaigationPages.SearchPage);
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App.NavigationRoadmap.SetFrom((int)App.NavaigationPages.SearchPage);
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void SearchBoxEventsQuerySubmitted(object sender, RoutedEventArgs e)
        {
            string key = SearchTextBox.Text;
            if ((key != null) && !(key.Equals(string.Empty)))
            {
                var searchGroup = await ReSearchDataSource.GetSearchGroupAsync("search", key);
                if (searchGroup == null)
                {
                    Helpers.InternetFailureAlarm();
                }
                else
                {
                    this.DefaultViewModel["SearchItems"] = searchGroup;
                }
            }
        }
    }
}
