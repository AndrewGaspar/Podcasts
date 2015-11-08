using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Podcasts.DataObjects
{
    public interface ISubscription : IUserOwnedEntityData
    {
        string LocationHref { get; set; }
    }

    public class Subscription : ISubscription
    {
        public string Id { get; set; }

        public string LocationHref { get; set; }

        public string UserId { get; set; }
    }
}
