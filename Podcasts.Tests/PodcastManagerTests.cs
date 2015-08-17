using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Podcasts.Tests
{
    [TestClass]
    public class PodcastManagerTests
    {
        private PodcastManager Manager = new PodcastManager("test-podcasts.json-db");

        [TestInitialize]
        public async Task InitializeAsync()
        {
            await Manager.ClearDatabase();
        }

        [TestMethod]
        public async Task AddPodcastTest()
        {
            var uri = TestFeeds.GetFeedUri("beastcast.xml");

            var beastcast = await Manager.AddPodcastAsync(uri);

            Assert.AreNotEqual(Guid.Empty, beastcast.Id);
            Assert.AreEqual("The Giant Beastcast", beastcast.Title);
            Assert.AreEqual(new Uri("http://static.giantbomb.com/uploads/original/0/31/2750982-beastcast_itunes.png"), beastcast.Image);
            Assert.AreEqual(uri, beastcast.Location);


        }
    }
}
