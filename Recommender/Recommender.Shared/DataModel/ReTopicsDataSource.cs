using Recommender.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Recommender.DataModel
{
    public sealed class ReTopicsDataSource
    {
        private static ReTopicsDataSource _reTopicsDataSource = new ReTopicsDataSource();

        private const int iMinInclusivePageValue = 1;
        private const int iMaxExclusivePageValue = 4;

        private ObservableCollection<ReDataGroup> _groups = new ObservableCollection<ReDataGroup>();
        public ObservableCollection<ReDataGroup> Groups
        {
            get { return this._groups; }
        }

        private ObservableCollection<ReDataGroup> _contents = new ObservableCollection<ReDataGroup>();
        public ObservableCollection<ReDataGroup> Contents
        {
            get { return this._contents; }
        }

        public static async Task<IEnumerable<ReDataGroup>> GetGroupsAsync()
        {
            await _reTopicsDataSource.GetWebServiceDataAsync();
            return _reTopicsDataSource.Groups;
        }

        public static async Task<ReDataGroup> GetGroupAsync(string uniqueId)
        {
            await _reTopicsDataSource.GetWebServiceDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _reTopicsDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() >= 1) return matches.First();
            return null;
        }

        public static async Task<ReDataGroup> GetContentsAsync(string uniqueId)
        {
            await _reTopicsDataSource.GetContentsWebServiceDataAsync(uniqueId);

            // Simple linear search is acceptable for small data sets
            var matches = _reTopicsDataSource.Contents.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() >= 1) return matches.First();
            return null;
            //var matches = _reTopicsSummaryDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            //if (matches.Count() >= 1) return matches.First();
            //return null;
        }

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
            if (this._groups.Count != 0)
                return;

            try
            {
                string requestUrl = "http://replatform.cloudapp.net:8000/getTopicalSummary/?uuid={0}&type={1}&from={2}&to={3}";
                requestUrl = String.Format(requestUrl, App.gDeviceName, App.gDeviceType, App.NavigationRoadmap.GetFrom(), App.NavigationRoadmap.GetTo());

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
            catch(Exception) {}
        }

        private void GetResultJsonData(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                return;
            }

            if (jsonData.Equals("null"))
            {
                return;
            }

            _groups = new ObservableCollection<ReDataGroup>();

            JsonObject originalJsonObject = JsonObject.Parse(jsonData);
            JsonArray groupJsonArray = originalJsonObject["data"].GetArray();

            int groupIndex = 0;
            foreach (JsonValue groupValue in groupJsonArray)
            {
                JsonObject jsonObject = groupValue.GetObject();

                string groupId = Helpers.GetJosnObjectStringValue(jsonObject, "_id");
                string groupTitle = Helpers.GetJosnObjectStringValue(jsonObject, "name");
                string description = Helpers.GetJosnObjectIntegerValue(jsonObject, "count");
                string imagePath = Helpers.GetJosnObjectStringValue(jsonObject, "img");
                ReDataGroup group = new ReDataGroup(groupId, groupTitle, imagePath, description);

                JsonArray collectionJsonArray = jsonObject["cor"].GetArray();
                int indexInGroup = 0;
                foreach (JsonValue collectionValue in collectionJsonArray)
                {
                    JsonObject collectionObject = collectionValue.GetObject();
                    string uniqueId = Helpers.GetJosnObjectStringValue(collectionObject, "id");
                    string itemTitle = Helpers.GetJosnObjectStringValue(collectionObject, "name");
                    string itemDescription = Helpers.GetJosnObjectIntegerValue(collectionObject, "count");
                    string itemImagePath = Helpers.GetJosnObjectStringValue(collectionObject, "img");
                    group.Items.Add(new ReDataItem(indexInGroup, uniqueId, itemTitle, itemImagePath, itemDescription));
                    indexInGroup++;
                }
                
                this._groups.Add(group);
                groupIndex++;
            }
        }

        private async Task GetContentsWebServiceDataAsync(string uniqueId)
        {
            if (String.IsNullOrWhiteSpace(uniqueId))
                return;

            try
            {
                string requestUrl = "http://replatform.cloudapp.net:8000/getTopical/?id={0}&uuid={1}&type={2}&from={3}&to={4}&r={5}";
                Random random = new Random();
                int r = random.Next();
                requestUrl = String.Format(requestUrl, uniqueId, App.gDeviceName, App.gDeviceType, App.NavigationRoadmap.GetFrom(), App.NavigationRoadmap.GetTo(), r);

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

                GetContentResultJsonData(jsonText);

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

        private void GetContentResultJsonData(string jsonData)
        {
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return;
            }

            if (jsonData.Equals("null"))
            {
                return;
            }

            _contents = new ObservableCollection<ReDataGroup>();

            JsonArray originalGroupJsonArray = JsonArray.Parse(jsonData);
            foreach (JsonValue originalGroupValue in originalGroupJsonArray)
            {
                JsonObject originalJsonObject = originalGroupValue.GetObject();


                string groupId = Helpers.GetJosnObjectStringValue(originalJsonObject, "_id");
                string groupTitle = Helpers.GetJosnObjectStringValue(originalJsonObject, "name");
                ReDataGroup group = new ReDataGroup(groupId, groupTitle);

                JsonArray groupJsonArray = originalJsonObject["cor"].GetArray();
                int indexInGroup = 0;
                foreach (JsonValue groupValue in groupJsonArray)
                {
                    JsonObject contentJsonObject = groupValue.GetObject();

                    string playUrl = "";
                    JsonArray playUrlJsonArray = contentJsonObject["playlist"].GetArray();

                    foreach (JsonValue itemValue in playUrlJsonArray)
                    {
                        JsonObject itemObject = itemValue.GetObject();
                        JsonArray playlistJsonArray = itemObject["list"].GetArray();

                        foreach (JsonValue playUrlValue in playlistJsonArray)
                        {
                            JsonObject playUrlObject = playUrlValue.GetObject();
                            playUrl = playUrlObject.ContainsKey("url") ? playUrlObject["url"].GetString() : "";
                            break;
                        }
                    }

                    string uniqueId = Helpers.GetJosnObjectStringValue(contentJsonObject, "_id");
                    string title = Helpers.GetJosnObjectStringValue(contentJsonObject, "title");//jsonObject.ContainsKey("title") ? jsonObject["title"].GetString() : "";
                    string actor = Helpers.GetJosnObjectStringValue(contentJsonObject, "actor");//jsonObject.ContainsKey("actor") ? jsonObject["actor"].GetString() : "";
                    string director = Helpers.GetJosnObjectStringValue(contentJsonObject, "director");//jsonObject.ContainsKey("director") ? jsonObject["director"].GetString() : "";
                    string cate = Helpers.GetJosnObjectStringValue(contentJsonObject, "cate");//jsonObject.ContainsKey("cate") ? jsonObject["cate"].GetString() : "";
                    string imagePath = Helpers.GetJosnObjectStringValue(contentJsonObject, "img");//jsonObject.ContainsKey("img") ? jsonObject["img"].GetString() : "";
                    string description = Helpers.GetJosnObjectStringValue(contentJsonObject, "info");//jsonObject.ContainsKey("info") ? jsonObject["info"].GetString() : "";
                    string score = Helpers.GetJosnObjectStringValue(contentJsonObject, "banben");
                    if (string.IsNullOrEmpty(score))
                    {
                        var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                        score = loader.GetString("DefaultScore");
                    }
                    else
                    {
                        score = score.Substring(1);
                    }
                    string year = Helpers.GetJosnObjectStringValue(contentJsonObject, "year");
                    group.Items.Add(new ReDataItem(indexInGroup, uniqueId, title, actor, director, imagePath, description, playUrl, score, year));
                
                    indexInGroup++;
                }
                this._contents.Add(group);
            }            
        }
    }
}