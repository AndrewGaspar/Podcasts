using System.Runtime.Serialization;
using Podcasts.Models;

namespace Podcasts.Messages
{
    [DataContract]
    public class PlayEpisodeRequest
    {
        [DataMember]
        public Episode Episode { get; set; }
    }
}
