using System.Runtime.Serialization;
using PodcastsService.Models;

namespace PodcastsService.Messages
{
    [DataContract]
    public class PlayEpisodeRequest
    {
        [DataMember]
        public Episode Episode { get; set; }
    }
}
