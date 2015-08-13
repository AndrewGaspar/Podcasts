using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Podcasts.Models;

namespace Podcasts
{
    public class PodcastsFile : JsonObjectsFile<Podcast>
    {
        public PodcastsFile(string databaseName) : base(databaseName)
        {

        }

        public Task<IList<Podcast>> ReadPodcastsAsync() => ReadObjectsAsync();

        public Task AddPodcastAsync(Podcast podcast) => AddObjectAsync(podcast);

        protected override Podcast PrepareAddObject(Podcast obj)
        {
            var newPodcast = new Podcast(obj);

            if(newPodcast.Id == Guid.Empty)
            {
                newPodcast.Id = Guid.NewGuid();
            }

            return newPodcast;
        }

        protected override void OnAddObjectSuccess(Podcast original, Podcast addedObject)
        {
            original.Id = addedObject.Id;
        }

        public Task RemovePodcastAsync(Podcast podcast) => RemoveObjectAsync(podcast);

        public Task RemovePodcastAsync(Guid id) => RemovePodcastAsync(new Podcast { Id = id });
    }
}
