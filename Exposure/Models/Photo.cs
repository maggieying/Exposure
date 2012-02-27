using System;
namespace Exposure.Controllers
{
    public class Photo
    {
        public string Owner { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public int Views { get; set; }
        public string Src { get; set; }
        public string Url { get; set; }
        public DateTime? UploadedTime { get; set; }

        public string Secret { get; set; }
        public string Server { get; set; }
        public string Farm { get; set; }
    }

    public enum PhotoSize
    {
        default500 = 0,
        smallSquare = 's',
        thumbnail = 't',
        small = 'm',
        medium = 'z',
        large = 'l',
        orginal = 'o'
    }
}
