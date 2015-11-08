using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Podcasts.Service.DataObjects;
using Podcasts.Service.Models;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Podcasts.Service.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)]
    public class SubscriptionController : TableController<SubscriptionEntity>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<SubscriptionEntity>(context, Request, Services, enableSoftDelete: true);
        }

        private string GetUserId()
        {
            var user = User as ServiceUser;

            return user.Id;
        }

        private bool IsAuthorized(SubscriptionEntity sub)
        {
            var userId = GetUserId();

            return sub.UserId == userId;
        }

        private IQueryable<SubscriptionEntity> UserAuthedQuery()
        {
            var userId = GetUserId();

            return Query().Where(sub => sub.UserId == userId);
        }

        // GET tables/Subscription
        public IQueryable<SubscriptionEntity> GetAllSubscriptions() => UserAuthedQuery();

        // GET tables/Subscription/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<SubscriptionEntity> GetSubscription(string id)
        {
            return SingleResult.Create(
                UserAuthedQuery().Where(sub => sub.Id == id));
        }

        // POST tables/Subscription
        public async Task<IHttpActionResult> PostSubscription(SubscriptionEntity item)
        {
            if (item.UserId == null)
            {
                item.UserId = GetUserId();
            }
            else if (item.UserId != GetUserId())
            {
                return Unauthorized();
            }
            
            var current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Subscription/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task DeleteSubscription(string id)
        {
            var subscription = await GetSubscription(id).Queryable.ToListAsync();

            if(subscription.Count == 0)
            {
                return;
            }
            
            if(IsAuthorized(subscription[0]))
            {
                await DeleteAsync(id);
            }
        }
    }
}