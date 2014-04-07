using Recommender.Common;
using Recommender.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.Enumeration.Pnp;
using Windows.Storage;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Recommender.DataModel
{
    public sealed class ReUserActionDataSource
    {
        private static ReUserActionDataSource _reUserActionDataSource = new ReUserActionDataSource();

        private const int iMinInclusivePageValue = 1;
        private const int iMaxExclusivePageValue = 4;

        private ObservableCollection<ReDataGroup> _groups = new ObservableCollection<ReDataGroup>();
        public ObservableCollection<ReDataGroup> Groups
        {
            get { return this._groups; }
        }

        public static async Task<IEnumerable<ReDataGroup>> GetGroupsAsync()
        {
            //await _reDataSource.GetReDataAsync();
            await _reUserActionDataSource.GetWebServiceDataAsync();
            return _reUserActionDataSource.Groups;
        }

        public static async Task<ReDataGroup> GetGroupAsync(string uniqueId)
        {
            //await _reDataSource.GetReDataAsync();
            await _reUserActionDataSource.GetWebServiceDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _reUserActionDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() >= 1) return matches.First();
            return null;
        }

        public static async Task<ReDataItem> GetItemAsync(string uniqueId)
        {
            //await _reDataSource.GetReDataAsync();
            await _reUserActionDataSource.GetWebServiceDataAsync();

            // Simple linear search is acceptable for small data sets
            var matches = _reUserActionDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task SetUserAction(string mediaId)
        {
            await _reUserActionDataSource.PostWebServiceDataAsync(mediaId);
        }

        private async Task PostWebServiceDataAsync(string mediaId)
        {
            try
            {
                string requestUrl = "http://replatform.cloudapp.net:8000/userAction?uuid={0}&media_id={1}";
                string userid = App.gPhysicalAddress;
                requestUrl = String.Format(requestUrl, userid, mediaId);

                HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
                filter.AutomaticDecompression = true;
                HttpClient httpClient = new HttpClient(filter);
                CancellationTokenSource cts = new CancellationTokenSource();

                filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.Default;
                filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.Default;

                Uri resourceUri;
                if (!Uri.TryCreate(requestUrl.Trim(), UriKind.Absolute, out resourceUri))
                {
                    return;
                }

                HttpResponseMessage response = await httpClient.GetAsync(resourceUri).AsTask(cts.Token);
                string jsonText = await Helpers.GetResultAsync(response, cts.Token);

                GetPostUserActionResult(jsonText);

                if (filter != null)
                {
                    filter.Dispose();
                    filter = null;
                }

                if (httpClient != null)
                {
                    httpClient.Dispose();
                    httpClient = null;
                }

                if (cts != null)
                {
                    cts.Dispose();
                    cts = null;
                }
            }
            catch (Exception) { }
        }

        private void GetPostUserActionResult(string jsonData) { }

        //private async Task GetReDataAsync()
        //{
        //    if (this._groups.Count != 0)
        //        return;

        //    Uri dataUri = new Uri("ms-appx:///DataModel/ReData.json");

        //    StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
        //    string jsonText = await FileIO.ReadTextAsync(file);
        //    JsonObject jsonObject = JsonObject.Parse(jsonText);
        //    JsonArray jsonArray = jsonObject["Groups"].GetArray();

        //    foreach (JsonValue groupValue in jsonArray)
        //    {
        //        JsonObject groupObject = groupValue.GetObject();
        //        ReDataGroup group = new ReDataGroup(groupObject["UniqueId"].GetString(),
        //                                                    groupObject["Title"].GetString());
        //        int index = 0;
        //        foreach (JsonValue itemValue in groupObject["Items"].GetArray())
        //        {
        //            JsonObject itemObject = itemValue.GetObject();

        //            string uniqueIdTmp = itemObject["UniqueId"].GetString();
        //            string titleTmp = itemObject["Title"].GetString();
        //            string subTitleTmp = itemObject["Subtitle"].GetString();
        //            string imagePathTmp = itemObject["ImagePath"].GetString();
        //            string descriptionTmp = itemObject["Description"].GetString();
        //            string playUrlTmp = itemObject["PlayUrl"].GetString();
        //            ReDataItem ri = new ReDataItem(index, uniqueIdTmp);
        //            group.Items.Add(ri);
        //            index++;
        //        }
        //        this.Groups.Add(group);
        //    }
        //}

        private async Task GetWebServiceDataAsync()
        {
            try
            {
                string requestUrl = "http://replatform.cloudapp.net:8000/getfavor/?uuid={0}&page={1}";
                string userid = App.gPhysicalAddress;

                Random random = new Random();
                int page = random.Next(iMinInclusivePageValue, iMaxExclusivePageValue);
                requestUrl = String.Format(requestUrl, userid, page);

                HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
                filter.AutomaticDecompression = true;
                HttpClient httpClient = new HttpClient(filter);
                CancellationTokenSource cts = new CancellationTokenSource();

                filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.Default;
                filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.Default;

                Uri resourceUri;
                if (!Uri.TryCreate(requestUrl.Trim(), UriKind.Absolute, out resourceUri))
                {
                    return;
                }

                HttpResponseMessage response = await httpClient.GetAsync(resourceUri).AsTask(cts.Token);
                string jsonText = await Helpers.GetResultAsync(response, cts.Token);

                GetResultJsonData(jsonText);

                if (filter != null)
                {
                    filter.Dispose();
                    filter = null;
                }

                if (httpClient != null)
                {
                    httpClient.Dispose();
                    httpClient = null;
                }

                if (cts != null)
                {
                    cts.Dispose();
                    cts = null;
                }
            }
            catch (Exception) { }
        }

        private void GetResultJsonData(string jsonData)
        {
            //if ((jsonData == null) || (jsonData.Equals(String.Empty)))
            //{
            //    return;
            //}

            //_groups = new ObservableCollection<ReDataGroup>();

            //string groupId = "Guess";
            //string groupTitle = "Guess";
            //ReDataGroup group = new ReDataGroup(groupId, groupTitle);

            //JsonArray groupJsonArray = JsonArray.Parse(jsonData);
            //int indexInGroup = 0;
            //foreach (JsonValue groupValue in groupJsonArray)
            //{
            //    JsonObject jsonObject = groupValue.GetObject();

            //    string playUrl = "";
            //    JsonArray jsonArray = jsonObject["playlist"].GetArray();

            //    foreach (JsonValue itemValue in jsonArray)
            //    {
            //        JsonObject itemObject = itemValue.GetObject();
            //        JsonArray playlistJsonArray = itemObject["list"].GetArray();

            //        foreach (JsonValue playUrlValue in playlistJsonArray)
            //        {
            //            JsonObject playUrlObject = playUrlValue.GetObject();
            //            playUrl = playUrlObject.ContainsKey("url") ? playUrlObject["url"].GetString() : "";
            //        }
            //    }

            //    string uniqueId = jsonObject.ContainsKey("_id") ? jsonObject["_id"].GetString() : "";
            //    string title = jsonObject.ContainsKey("title") ? jsonObject["title"].GetString() : "";
            //    string actor = jsonObject.ContainsKey("actor") ? jsonObject["actor"].GetString() : "";
            //    string director = jsonObject.ContainsKey("director") ? jsonObject["director"].GetString() : "";
            //    string cate = jsonObject.ContainsKey("cate") ? jsonObject["cate"].GetString() : "";
            //    string imagePath = jsonObject.ContainsKey("img") ? jsonObject["img"].GetString() : "";
            //    string description = jsonObject.ContainsKey("info") ? jsonObject["info"].GetString() : "";
            //    group.Items.Add(new ReDataItem(indexInGroup, uniqueId, title, actor, director, imagePath, description, playUrl));

            //    indexInGroup++;
            //}
            //this._groups.Add(group);
        }
    }
}