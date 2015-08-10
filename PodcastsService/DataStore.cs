using Podcasts.Models;
using System;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Podcasts
{
    internal class PodcastsFile
    {
        const string DatabaseFileName = "podcasts.stuff";

        private Task<StorageFile> OpenFileAsync()
            => ApplicationData.Current.RoamingFolder.CreateFileAsync(DatabaseFileName, CreationCollisionOption.OpenIfExists).AsTask();

        public async Task<IList<Podcast>> ReadPodcastsAsync()
        {
            var file = await OpenFileAsync();

            var contents = await FileIO.ReadTextAsync(file);

            return JsonHelper.FromJson<List<Podcast>>(contents);
        }

        public async Task AddPodcastAsync(Podcast p)
        {
            var podcasts = await ReadPodcastsAsync();
            podcasts.Add(p);

            var contents = JsonHelper.ToJson(podcasts);

            var file = await OpenFileAsync();

            await FileIO.WriteTextAsync(file, contents);
        }
    }

    public class DataStore
    {
        private PodcastsFile File = new PodcastsFile();

        public Task AddPodcastAsync(Podcast podcast) => File.AddPodcastAsync(podcast);

        public Task<IList<Podcast>> GetPodcastsAsync(Podcast podcast) => File.ReadPodcastsAsync();
    }
}
