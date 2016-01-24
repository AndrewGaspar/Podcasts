using System.Runtime.Serialization;

namespace Podcasts.Service.Models
{
    [DataContract]
    public class Podcast
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "href")]
        public string Location { get; set; }
    }
}