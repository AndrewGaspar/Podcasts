using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Podcasts.Models;
using Podcasts.Storage;

namespace Podcasts.Tests
{
    [TestClass]
    public class PodcastFileTests
    {
        private PodcastsFile PodcastsFile = new PodcastsFile("testfile.podcasts-db");

        [TestInitialize]
        public async Task CleanDatabase()
        {
            await PodcastsFile.EraseFileAsync();
        }

        [TestMethod]
        public async Task AddPodcastTest()
        {
            const string title = "My Great Podcast";
            var location = new Uri("http://www.newssite.net/our-podcast");
            var image = new Uri("http://www.newssite-cdn.net/picture.jpg");

            var podcast = new Podcast
            {
                Title = title,
                Location = location,
                Image = image
            };

            await PodcastsFile.AddPodcastAsync(podcast);

            var podcasts = await PodcastsFile.ReadPodcastsAsync();

            Assert.AreEqual(1, podcasts.Count);

            var deserializedPodcast = podcasts[0];

            Assert.AreEqual(podcast.Id, deserializedPodcast.Id);
            Assert.AreEqual(podcast.Title, deserializedPodcast.Title);
            Assert.AreEqual(podcast.Location, deserializedPodcast.Location);
            Assert.AreEqual(podcast.Image, deserializedPodcast.Image);
        }

        private IEnumerable<char> GetRandomChars(Random r, int numChars)
        {
            return Enumerable.Range(0, numChars).Select(_ => (char)('a' + r.Next(0, 26)));
        }

        private string GetRandomString(Random random, int minimumSize = 10, int maximumSize = 32)
        {
            return string.Concat(GetRandomChars(random, random.Next(minimumSize, maximumSize)));
        }

        private Uri GetRandomUrl(Random random, string end)
        {
            Func<int, string> rs = up => GetRandomString(random, 3, up);
            
            return new Uri($"http://{rs(3)}.{rs(7)}.{rs(3)}/{end}");

        }

        private async Task<List<Podcast>> AddPodcastsSeriallyAsync(PodcastsFile file, int numPodcasts)
        {
            var podcasts = new List<Podcast>();
            var random = new Random();

            foreach (var i in Enumerable.Range(0, numPodcasts))
            {
                var podcast = new Podcast
                {
                    Title = GetRandomString(random),
                    Location = GetRandomUrl(random, "podcast.xml"),
                };

                // only set Image sometimes
                if (random.NextDouble() > 0.5)
                    podcast.Image = GetRandomUrl(random, "album.png");

                await file.AddPodcastAsync(podcast);

                podcasts.Add(podcast);
            }

            return podcasts;
        }

        public void AssertPodcastListsAreIdentical(IList<Podcast> p1, IList<Podcast> p2)
        {
            Assert.IsTrue(p1.All(podcast => p2.Contains(podcast)));
        }

        [TestMethod]
        public async Task RemovePodcastsTest()
        {
            var podcasts = await AddPodcastsSeriallyAsync(PodcastsFile, 30);

            var fivePodcasts = new[] { podcasts[0], podcasts[6], podcasts[12], podcasts[18], podcasts[24] };

            Predicate<Podcast> removeThesePodcasts = podcast => fivePodcasts.Any(thisPodcast => thisPodcast.Id == podcast.Id);

            podcasts.RemoveAll(removeThesePodcasts);
            await PodcastsFile.RemoveMatchingObjectsAsync(fivePodcasts);

            var writtenPodcasts = await PodcastsFile.ReadPodcastsAsync();

            AssertPodcastListsAreIdentical(podcasts, writtenPodcasts);
        }

        [TestMethod]
        public async Task MultithreadedTest()
        {
            var tasks = new List<Task<List<Podcast>>>();

            for (var i = 0; i < 4; i++)
            {
                tasks.Add(Task.Run(() => AddPodcastsSeriallyAsync(PodcastsFile, 50)));
            }

            var randomPodcasts = (await Task.WhenAll(tasks)).Aggregate(
                new Podcast[0] as IEnumerable<Podcast>, (acc, item) => acc.Concat(item)).ToList();

            var writtenPodcasts = await PodcastsFile.ReadPodcastsAsync();

            AssertPodcastListsAreIdentical(randomPodcasts, writtenPodcasts);
        }

        [TestMethod]
        public async Task StressTest()
        {
            var randomPodcasts = await AddPodcastsSeriallyAsync(PodcastsFile, 250);
            var writtenPodcasts = await PodcastsFile.ReadPodcastsAsync();

            AssertPodcastListsAreIdentical(randomPodcasts, writtenPodcasts);
        }
    }
}
