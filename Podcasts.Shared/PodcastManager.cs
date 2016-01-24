using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Podcasts.Dom;
using Podcasts.Exceptions;
using Podcasts.Models;
using Podcasts.Storage;
using Podcasts.Service;

namespace Podcasts
{
    /// <summary>
    /// There should only be one live instance of PodcastManager per file on disk.
    /// </summary>
    public class PodcastManager
    {
        private PodcastsFile File;
        private List<Podcast> PodcastCache;

        public PodcastManager(string dbName)
        {
            File = new PodcastsFile(dbName);
        }

        private async Task LoadFileAsync()
        {
            if (PodcastCache != null)
            {
                return;
            }

            PodcastCache = (await File.ReadPodcastsAsync()).ToList();
        }

        private async Task SaveNewPodcastToFileAsync(Podcast podcast)
        {
            if (PodcastCache.Any(cast => cast.Location == podcast.Location))
            {
                throw new DuplicatePodcastException(podcast.Location);
            }

            await File.AddPodcastAsync(podcast);

            PodcastCache.Add(new Podcast(podcast));
        }

        private async Task SaveUpdatedPodcastToFileAsync(Podcast podcast)
        {
            await File.UpdateAnObjectAsync(cast => cast.Id == podcast.Id, cast =>
            {
                cast.Update(podcast);
            });

            PodcastCache.First(cast => cast.Id == podcast.Id).Update(podcast);
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

        public async Task<Podcast> AddPodcastAsync(Uri podcastUri)
        {
            await LoadFileAsync();

            var results = await EvaluatePodcastFeedAsync(podcastUri);

            if (results.RedirectUri != null)
            {
                podcastUri = results.RedirectUri;
            }

            var feed = results.Feed;

            var podcast = new Podcast
            {
                Title = feed.Title,
                Location = podcastUri,
                Image = feed.Image.Url,
            };

            await SaveNewPodcastToFileAsync(podcast);

            using (var subscription = new SubscriptionService(new Uri("http://localhost:3333"), new BasicAuthStrategy("my_user", "password")))
            {
                await subscription.PostSubscriptionAsync(podcast.Location.AbsoluteUri);
            }

            return podcast;
        }

        public async Task<IList<Podcast>> GetPodcastsAsync()
        {
            await LoadFileAsync();

            return PodcastCache.Select(p => new Podcast(p)).ToList();
        }

        public async Task<PodcastFeed> GetPodcastFeedAsync(Podcast podcast)
        {
            await LoadFileAsync();

            var results = await EvaluatePodcastFeedAsync(podcast.Location);

            if (results.RedirectUri != null)
            {
                var record = new Podcast(podcast);
                record.Location = results.RedirectUri;
                await SaveUpdatedPodcastToFileAsync(record);
                podcast.Update(record);
            }

            return results.Feed;
        }

        public async Task ClearDatabaseAsync()
        {
            await File.EraseFileAsync();
        }
    }
}