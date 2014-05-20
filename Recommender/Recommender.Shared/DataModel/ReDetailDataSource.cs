using Recommender.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Recommender.DataModel
{
    public sealed class ReDetailDataSource
    {
        private static ReDetailDataSource _reDetailDataSource = new ReDetailDataSource();

        private const int iMinInclusivePageValue = 1;
        private const int iMaxExclusivePageValue = 4;

        private ObservableCollection<ReDataGroup> _groups = new ObservableCollection<ReDataGroup>();
        public ObservableCollection<ReDataGroup> Groups
        {
            get { return this._groups; }
        }

        //public static async Task<IEnumerable<ReDataGroup>> GetGroupsAsync()
        //{
        //    //await _reDataSource.GetReDataAsync();
        //    await _reDetailDataSource.GetWebServiceDataAsync();
        //    return _reDetailDataSource.Groups;
        //}

        public static async Task<ReDataGroup> GetWebServiceDetailAsync(string uniqueId)
        {
            var matches = _reDetailDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() >= 1)
            {
                return matches.First();
            }
            else
            {
                //await _reDataSource.GetReDataAsync();
                await _reDetailDataSource.GetWebServiceDetailDataAsync(uniqueId);
                // Simple linear search is acceptable for small data sets
                matches = _reDetailDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
                if (matches.Count() >= 1) return matches.First();
            }
            
            return null;
        }

        public static async Task<string> GetHLSPlayUrlAsync(string orignalUrl)
        {
            string playUrl = await _reDetailDataSource.GetWebServiceHLSPlayUrlDataAsync(orignalUrl);
            if (!string.IsNullOrEmpty(playUrl)) return playUrl;
            return null;
        }

        //public static async Task<ReDataItem> GetItemAsync(string uniqueId)
        //{
        //    //await _reDataSource.GetReDataAsync();
        //    await _reDetailDataSource.GetWebServiceDetailDataAsync(uniqueId);

        //    // Simple linear search is acceptable for small data sets
        //    var matches = _reDetailDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
        //    if (matches.Count() == 1) return matches.First();
        //    return null;
        //}

        //private async Task GetReDetailDataAsync()
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

        private async Task<string> GetWebServiceHLSPlayUrlDataAsync(string originalUrl)
        {
            string resultUrl = null;
            try
            {
                if (string.IsNullOrEmpty(originalUrl))
                {
                    return resultUrl;
                }

                HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
                filter.AutomaticDecompression = true;
                HttpClient httpClient = new HttpClient(filter);
                CancellationTokenSource cts = new CancellationTokenSource();

                filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.Default;
                filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.Default;

                Uri resourceUri;
                if (!Uri.TryCreate(originalUrl.Trim(), UriKind.Absolute, out resourceUri))
                {
                    return null;
                }

                HttpResponseMessage response = await httpClient.GetAsync(resourceUri).AsTask(cts.Token);
                resultUrl = await Helpers.GetResultAsync(response, cts.Token);
            }
            catch (Exception) { }

            return resultUrl;
        }


        private async Task GetWebServiceDetailDataAsync(string uniqueId)
        {
            try
            {
                if (uniqueId.Equals(string.Empty))
                {
                    return;
                }

                string requestUrl = "http://replatform.cloudapp.net:8000/getmedia/?id={0}&uuid={1}&type={2}&from={3}&to={4}"; 
//                #if DEBUG
//                //string requestUrl = "http://192.168.1.215:9999/getmedia/?id={0}";
//#endif

                requestUrl = String.Format(requestUrl, uniqueId, App.gDeviceName, App.gDeviceType, App.NavigationRoadmap.GetFrom(), App.NavigationRoadmap.GetTo());

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
                await GetDetailResultJsonData(jsonText);                
            }
            catch(Exception) {}
        }

        // create media detail group
        private async Task GetDetailResultJsonData(string jsonData)
        {
            if ((jsonData == null) || (jsonData.Equals(String.Empty)))
            {
                return;
            }

            ReDataGroup group = null;

            JsonArray groupJsonArray = JsonArray.Parse(jsonData);
            foreach (JsonValue groupValue in groupJsonArray)
            {
                JsonObject jsonObject = groupValue.GetObject();

                string playUrl = "";
                JsonArray jsonArray = jsonObject["playlist"].GetArray();

                foreach (JsonValue itemValue in jsonArray)
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

                string uniqueId = Helpers.GetJosnObjectStringValue(jsonObject, "_id");//jsonObject.ContainsKey("_id") ? jsonObject["_id"].GetString() : "";
                string title = Helpers.GetJosnObjectStringValue(jsonObject, "title");//jsonObject.ContainsKey("title") ? jsonObject["title"].GetString() : "";
                string actor = Helpers.GetJosnObjectStringValue(jsonObject, "actor");//jsonObject.ContainsKey("actor") ? jsonObject["actor"].GetString() : "";
                string director = Helpers.GetJosnObjectStringValue(jsonObject, "director");//jsonObject.ContainsKey("director") ? jsonObject["director"].GetString() : "";
                string cate = Helpers.GetJosnObjectStringValue(jsonObject, "cate");//jsonObject.ContainsKey("cate") ? jsonObject["cate"].GetString() : "";
                string imagePath = Helpers.GetJosnObjectStringValue(jsonObject, "img");//jsonObject.ContainsKey("img") ? jsonObject["img"].GetString() : "";
                string description = Helpers.GetJosnObjectStringValue(jsonObject, "info");//jsonObject.ContainsKey("info") ? jsonObject["info"].GetString() : "";
                group = new ReDataGroup(uniqueId, title, actor, director, imagePath, description, playUrl);
            }
            if (group != null)
            {
                await _reDetailDataSource.GetWebServiceRelatedDataAsync(group);
            }
        }

        private async Task GetWebServiceRelatedDataAsync(ReDataGroup group)
        {
            try
            {
                if ((group == null) && (group.UniqueId.Equals(string.Empty)))
                {
                    return;
                }

                string requestUrl = "http://replatform.cloudapp.net:8000/getrelated/?id={0}&uuid={1}&type={2}&from={3}&to={4}";
                //string requestUrl = "http://192.168.1.215:9999/getrelated/?id={0}";
                requestUrl = String.Format(requestUrl, group.UniqueId, App.gDeviceName, App.gDeviceType, App.NavigationRoadmap.GetFrom(), App.NavigationRoadmap.GetTo());

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

                GetRelatedResultJsonData(group, jsonText);                
            }
            catch (Exception e) 
            {
                String ex = e.Message;
            }
        }

        // add related medias to group's items
        private void GetRelatedResultJsonData(ReDataGroup group, string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                this._groups.Add(group);
                return;
            }

            if (jsonData.Equals("null"))
            {
                this._groups.Add(group);
                return;
            }

            JsonArray groupJsonArray = JsonArray.Parse(jsonData);
            int indexInGroup = 0;
            foreach (JsonValue groupValue in groupJsonArray)
            {
                JsonObject jsonObject = groupValue.GetObject();

                string playUrl = "";
                JsonArray jsonArray = jsonObject["playlist"].GetArray();

                foreach (JsonValue itemValue in jsonArray)
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

                string uniqueId = Helpers.GetJosnObjectStringValue(jsonObject, "_id");//jsonObject.ContainsKey("_id") ? jsonObject["_id"].GetString() : "";
                string title = Helpers.GetJosnObjectStringValue(jsonObject, "title");//jsonObject.ContainsKey("title") ? jsonObject["title"].GetString() : "";
                string actor = Helpers.GetJosnObjectStringValue(jsonObject, "actor");//jsonObject.ContainsKey("actor") ? jsonObject["actor"].GetString() : "";
                string director = Helpers.GetJosnObjectStringValue(jsonObject, "director");//jsonObject.ContainsKey("director") ? jsonObject["director"].GetString() : "";
                string cate = Helpers.GetJosnObjectStringValue(jsonObject, "cate");//jsonObject.ContainsKey("cate") ? jsonObject["cate"].GetString() : "";
                string imagePath = Helpers.GetJosnObjectStringValue(jsonObject, "img");//jsonObject.ContainsKey("img") ? jsonObject["img"].GetString() : "";
                string description = Helpers.GetJosnObjectStringValue(jsonObject, "info");//jsonObject.ContainsKey("info") ? jsonObject["info"].GetString() : "";
                group.Items.Add(new ReDataItem(indexInGroup, uniqueId, title, actor, director, imagePath, description, playUrl));

                indexInGroup++;
            }
            this._groups.Add(group);
        }
    }
}