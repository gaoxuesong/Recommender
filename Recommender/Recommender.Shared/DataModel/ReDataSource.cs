using Recommender.Common;
//using Recommender.DataModel;
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
    public sealed class ReDataSource
    {
        private static ReDataSource _reDataSource = new ReDataSource();

        private const int defaultPageValue = 0;
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
            await _reDataSource.GetWebServiceDataAsync();
            return _reDataSource.Groups;
        }

        public static async Task<ReDataGroup> GetGroupAsync(string uniqueId)
        {
            //await _reDataSource.GetReDataAsync();
            await _reDataSource.GetWebServiceDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _reDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() >= 1) return matches.First();
            return null;
        }

        public static async Task<ReDataItem> GetItemAsync(string uniqueId)
        {
            //await _reDataSource.GetReDataAsync();
            await _reDataSource.GetWebServiceDataAsync();

            // Simple linear search is acceptable for small data sets
            var matches = _reDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() >= 1) return matches.First();
            return null;
        }

        private async Task GetReDataAsync()
        {
            if (this._groups.Count != 0)
                return;

            Uri dataUri = new Uri("ms-appx:///DataModel/ReData.json");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);
            JsonObject jsonObject = JsonObject.Parse(jsonText);
            JsonArray jsonArray = jsonObject["Groups"].GetArray();
            
            foreach (JsonValue groupValue in jsonArray)
            {
                JsonObject groupObject = groupValue.GetObject();
                ReDataGroup group = new ReDataGroup(groupObject["UniqueId"].GetString(),
                                                            groupObject["Title"].GetString());
                int index = 0;
                foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                {                    
                    JsonObject itemObject = itemValue.GetObject();

                    string uniqueIdTmp = itemObject["UniqueId"].GetString();
                    string titleTmp = itemObject["Title"].GetString();
                    string subTitleTmp = itemObject["Subtitle"].GetString();
                    string imagePathTmp = itemObject["ImagePath"].GetString();
                    string descriptionTmp = itemObject["Description"].GetString();
                    string playUrlTmp = itemObject["PlayUrl"].GetString();
                    ReDataItem ri = new ReDataItem(index, uniqueIdTmp);
                    group.Items.Add(ri);
                    index++;
                }
                this.Groups.Add(group);
            }
        }

        private async Task GetWebServiceDataAsync()
        {
            try
            {
                string requestUrl = "http://replatform.cloudapp.net:8000/getguess/?uuid={0}&type={1}&page={2}&from={3}&to={4}&r={5}";
                //string requestUrl = "http://192.168.1.215:9999/getguess/?uuid={0}&page={1}";
                string userid = App.gDeviceName;
                string type = App.gDeviceType;
                Random random = new Random();
                int page = defaultPageValue;//random.Next(iMinInclusivePageValue, iMaxExclusivePageValue);
                int r = random.Next();
                requestUrl = String.Format(requestUrl, userid, type, page, App.NavigationRoadmap.GetFrom(), App.NavigationRoadmap.GetTo(), r);

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

            string groupId = "Guess";
            string groupTitle = "Guess";
            ReDataGroup group = new ReDataGroup(groupId, groupTitle);

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

                string uniqueId = Helpers.GetJosnObjectStringValue(jsonObject, "_id");
                string title = Helpers.GetJosnObjectStringValue(jsonObject, "title");
                string actor = Helpers.GetJosnObjectStringValue(jsonObject, "actor");
                string director = Helpers.GetJosnObjectStringValue(jsonObject, "director");
                string cate = Helpers.GetJosnObjectStringValue(jsonObject, "cate");
                string imagePath = Helpers.GetJosnObjectStringValue(jsonObject, "img");
                string description = Helpers.GetJosnObjectStringValue(jsonObject, "info");
                string score = Helpers.GetJosnObjectStringValue(jsonObject, "banben");
                if (string.IsNullOrEmpty(score))
                {
                    var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                    score = loader.GetString("DefaultScore");
                }
                else
                {
                    score = score.Substring(1);
                }
                group.Items.Add(new ReDataItem(indexInGroup, uniqueId, title, actor, director, imagePath, description, playUrl, score));

                indexInGroup++;
            }
            this._groups.Add(group);
        }        
    }
}