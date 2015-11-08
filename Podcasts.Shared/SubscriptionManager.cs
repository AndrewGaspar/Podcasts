using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Podcasts
{
    using DataObjects;
    using Dom;
    using Exceptions;
    using Models;
    using Services;
    using Utilities;

    /// <summary>
    /// There should only be one live instance of PodcastManager per file on disk.
    /// </summary>
    public class SubscriptionManager
    {
        public MobileServiceCollection<Subscription, SubscriptionModel> Subscriptions { get; private set; }
        
        private SubscriptionManager()
        {
        }

        public static SubscriptionManager Current = new SubscriptionManager();

        private Task _initTask;
        public Task InitializeAsync()
        {
            return _initTask ?? (_initTask = InitializeAsyncInner());
        }

        public async Task RefreshAsync()
        {
            await InitializeAsync();

            await MobileService.Current.SyncAsync();

            foreach(var sub in await MobileService.Current.SubscriptionsTable.ToListAsync())
            {
                if (!Subscriptions.Any(model => model.Subscription.Id == sub.Id))
                {
                    Subscriptions.Add(Subscriptions.PrepareDataForCollection(sub));
                }
            }
        }

        private async Task InitializeAsyncInner()
        {
            await MobileService.Current.InitializeAsync();

            Subscriptions = new MobileServiceCollection<Subscription, SubscriptionModel>(
                MobileService.Current.SubscriptionsTable.CreateQuery(), sub => new SubscriptionModel(sub));

            await Subscriptions.LoadMoreItemsAsync((int)Subscriptions.TotalCount);
            
            await Task.WhenAll(from sub in Subscriptions select sub.InitializeAsync());
        }

        private async Task<SubscriptionModel> AddNewSubscriptionToDbAsync(Subscription sub, PodcastFeed feed)
        {
            if (Subscriptions.Any(cast => cast.Subscription.LocationHref == sub.LocationHref))
            {
                throw new DuplicatePodcastException(new Uri(sub.LocationHref));
            }

            await MobileService.Current.SubscriptionsTable.InsertAsync(sub);

            // don't block on push - just let server know changes were made
            MobileService.Current.PushAsync().Ignore();

            var model = new SubscriptionModel(sub, feed);

            Subscriptions.Add(model);

            return model;
        }

        private class PodcastFeedEvaluation
        {
            public PodcastFeed Feed;
            public Uri RedirectUri;
        }

        private async Task<PodcastFeedEvaluation> EvaluatePodcastFeedAsync(Uri podcastUri, List<Uri> previouslyTriedUris = null)
        {
            previouslyTriedUris = previouslyTriedUris ?? new List<Uri>();

            if (previouslyTriedUris.Count > 5)
            {
                throw new InvalidPodcastException(previouslyTriedUris[0], "Too many redirects.");
            }

            var feed = await PodcastFeed.LoadFeedAsync(podcastUri);

            if (feed.ITunes.NewFeedUrl != null)
            {
                previouslyTriedUris.Add(podcastUri);

                return await EvaluatePodcastFeedAsync(feed.ITunes.NewFeedUrl, previouslyTriedUris);
            }

            return new PodcastFeedEvaluation()
            {
                Feed = feed,
                RedirectUri = previouslyTriedUris.Count > 0 ? podcastUri : null
            };
        }

        public async Task<SubscriptionModel> AddSubscriptionAsync(Uri podcastUri)
        {
            await InitializeAsync();

            var results = await EvaluatePodcastFeedAsync(podcastUri);

            if (results.RedirectUri != null)
            {
                podcastUri = results.RedirectUri;
            }
            
            return await AddNewSubscriptionToDbAsync(new Subscription { LocationHref = podcastUri.ToString() }, results.Feed);
        }
    }
}