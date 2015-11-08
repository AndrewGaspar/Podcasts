using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Web.Http;
using Podcasts.Service.DataObjects;
using Podcasts.Service.Models;
using Microsoft.WindowsAzure.Mobile.Service;

namespace Podcasts.Service
{
    public static class WebApiConfig
    {
        public static void Register()
        {
            // Use this class to set configuration options for your mobile service
            var options = new ConfigOptions();

            // Use this class to set WebAPI configuration options
            var config = ServiceConfig.Initialize(new ConfigBuilder(options));

            // To display errors in the browser during development, uncomment the following
            // line. Comment it out again when you deploy your service for production use.
            // config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            Database.SetInitializer(new MobileServiceInitializer());
        }
    }

    public class MobileServiceInitializer : DropCreateDatabaseIfModelChanges<MobileServiceContext>
    {
        protected override void Seed(MobileServiceContext context)
        {
            var todoItems = new List<SubscriptionEntity>
            {
                new SubscriptionEntity { Id = Guid.NewGuid().ToString(), UserId="test_user" },
                new SubscriptionEntity { Id = Guid.NewGuid().ToString(), UserId="test_user" },
            };

            foreach (SubscriptionEntity todoItem in todoItems)
            {
                context.Set<SubscriptionEntity>().Add(todoItem);
            }

            base.Seed(context);
        }
    }
}

