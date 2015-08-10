using System;
using System.Runtime.Serialization;

namespace Podcasts.Models
{
    [DataContract]
    public class Podcast
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public Uri Location { get; set; }

        [DataMember]
        public Uri Image { get; set; }
    }
}
