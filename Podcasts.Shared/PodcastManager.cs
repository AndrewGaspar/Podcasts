using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Podcasts.Dom;
using Podcasts.Models;
using Podcasts.Storage;

namespace Podcasts
{
    public class PodcastManager
    {
        private PodcastsFile File;

        public PodcastManager(string dbName)
        {
            File = new PodcastsFile(dbName);
        }

        public async Task<Podcast> AddPodcastAsync(Uri podcastUri)
        {
            var feed = await PodcastFeed.LoadFeedAsync(podcastUri);

            if(feed.ITunes.NewFeedUrl != null)
            {
                return await AddPodcastAsync(feed.ITunes.NewFeedUrl);
            }

            var podcast = new Podcast
            {
                Title = feed.Title,
                Location = podcastUri,
                Image = feed.Image.Url,
            };

            await File.AddObjectAsync(podcast);

            return podcast;
        }

        public async Task ClearDatabase()
        {
            await File.EraseFileAsync();
        }
    }
}
