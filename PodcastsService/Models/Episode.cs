using System;
using System.Runtime.Serialization;

namespace Podcasts.Models
{
    [DataContract]
    public class Episode
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string PodcastName { get; set; }

        [DataMember]
        public Uri Location { get; set; }

        [DataMember]
        public Uri Image { get; set; }
    }
}
