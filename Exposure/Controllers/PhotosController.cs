using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Exposure.Controllers
{
    public class PhotosController : ApiController
    {
        readonly string flickrApiKey = "59bf3505dc6456e5ec4f899bba33e20d";
        readonly string flickrSecret = "77a00ec0d65b055a";
        readonly string flickrUriBase = "http://api.flickr.com/services/rest/";
        const int pageSize = 20;

        // GET api/Photos/Search?text=seattle
        // GET api/Photos/Search?text=seattle&page=3
        public Task<IQueryable<Photo>> Search(string text, DateTime? minTakenDate = null, DateTime? maxTakenDate = null, int page = 1)
        {
            var url = string.Format(flickrUriBase + "?method=flickr.photos.search" +
                "&text={0}&per_page={1}&page={2}{3}{4}&extras=views,owner_name,date_upload{5}" +
                "&format=json&nojsoncallback=1&api_key=" + flickrApiKey,
                text, pageSize, page,
                (minTakenDate == null) ? string.Empty : "&min_taken_date=" + minTakenDate.Value.ToString("yyyy-MM-dd"),
                (maxTakenDate == null) ? string.Empty : "&max_taken_date=" + maxTakenDate.Value.ToString("yyyy-MM-dd"),
                (minTakenDate == null && maxTakenDate == null) ? string.Empty : ",date_taken");

            return CallFlickrAPI(url).ContinueWith((task) =>
            {
                var jsonResponse = task.Result;
                return ParseJson(jsonResponse).AsQueryable();
            });
        }

        // GET api/Photos/GetInteresting
        // GET api/Photos/GetInteresting?date=2012-01-30
        // GET api/Photos/GetInteresting?date=2012-01-30&page=3
        public Task<IQueryable<Photo>> GetInteresting(DateTime? date = null, int page = 1)
        {
            var url = string.Format(flickrUriBase + "?method=flickr.interestingness.getList" +
                "{0}&per_page={1}&page={2}&extras=views,owner_name,date_upload" +
                "&format=json&nojsoncallback=1&api_key=" + flickrApiKey,
                (date == null)? string.Empty : "&date=" + date.Value.ToString("yyyy-MM-dd"), 
                pageSize, page);

            return CallFlickrAPI(url).ContinueWith((task) =>
            {
                var jsonResponse = task.Result;
                return ParseJson(jsonResponse).AsQueryable();
            });
        }

        // GET api/Photos/GetFavorite?userId=56795034@N00
        // GET api/Photos/GetFavorite?userId=56795034@N00&page=3
        public Task<IQueryable<Photo>> GetFavorites(string userId, int page = 1)
        {
            var url = string.Format(flickrUriBase + "?method=flickr.favorites.getPublicList" +
                "&user_id={0}&per_page={1}&page={2}&extras=views" +
                "&format=json&nojsoncallback=1&api_key=" + flickrApiKey,
                userId, pageSize, page);

            return CallFlickrAPI(url).ContinueWith((task) =>
            {
                var jsonResponse = task.Result;
                return ParseJson(jsonResponse).AsQueryable();
            });
        }

        private IEnumerable<Photo> ParseJson(string jsonResponse)
        {
            try
            {
                dynamic photos = JsonValue.Parse(jsonResponse);
                JsonArray photosArray = photos.photos.photo;

                return (from dynamic photo in photosArray
                        select new Photo
                        {
                            Id = photo.id,
                            Title = photo.title,
                            Owner = photo.owner,
                            Views = photo.views,
                            Url = string.Format("http://farm{0}.staticflickr.com/{1}/{2}_{3}.jpg",
                                                photo.farm, (int)photo.server, (string)photo.id, (string)photo.secret),
                            Farm = photo.farm,
                            Server = photo.server,
                            Secret = photo.secret
                        });
            }
            catch (Exception ex)
            {
                throw new HttpResponseException("An exception occurs when trying to parse this response: " + jsonResponse, ex);
            }
        }

        private Task<string> CallFlickrAPI(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url)
                };

                var response = httpClient.SendAsync(request).Result;

                return response.Content.ReadAsStringAsync().ContinueWith(task =>
                {
                    return task.Result;
                });
            }
        }
    }
}
