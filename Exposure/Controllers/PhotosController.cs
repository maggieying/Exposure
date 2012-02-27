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
        const int pageSize = 10;

        public Task<IQueryable<Photo>> Search(string text, int page = 1)
        {
            var url = string.Format(flickrUriBase + "?method={0}&text={1}&per_page={2}&page={3}&format=json&nojsoncallback=1&extras=views&api_key={4}",
                "flickr.photos.search", text, pageSize, page, flickrApiKey);

            return CallFlickrAPI(url).ContinueWith((task) =>
            {
                var jsonResponse = task.Result;
                return ParseJson(jsonResponse).AsQueryable();
            });
        }

        public Task<IQueryable<Photo>> GetInteresting(DateTime? date, int page = 1)
        {
            var exploredDate = (date != null) ? date.Value : DateTime.Now.Subtract(TimeSpan.FromDays(1));

            var url = string.Format(flickrUriBase + "?method={0}&date={1}&per_page={2}&page={3}&format=json&nojsoncallback=1&extras=views&api_key={4}",
                "flickr.interestingness.getList", exploredDate.ToString("yyyy-MM-dd"), pageSize, page, flickrApiKey);

            return CallFlickrAPI(url).ContinueWith((task) =>
            {
                var jsonResponse = task.Result;
                return ParseJson(jsonResponse).AsQueryable();
            });
        }

        private IEnumerable<Photo> ParseJson(string jsonResponse)
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
