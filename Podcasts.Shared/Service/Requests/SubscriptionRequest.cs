using System.Runtime.Serialization;

namespace Podcasts.Service.Requests
{
    [DataContract]
    public class SubscriptionRequest
    {
        [DataMember(Name = "href")]
        public string Href { get; set; }
    }
}