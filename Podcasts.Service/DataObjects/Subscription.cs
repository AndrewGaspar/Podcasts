using Microsoft.WindowsAzure.Mobile.Service;
using Podcasts.DataObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace Podcasts.Service.DataObjects
{
    [Table("Subscriptions")]
    public class SubscriptionEntity : EntityData, ISubscription
    {
        public string UserId { get; set; }
        
        public string LocationHref { get; set; }
    }
}