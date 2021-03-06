﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Podcasts.Dom;

namespace Podcasts.Tests
{
    [TestClass]
    public class PodcastFeedTests
    {
        [TestMethod]
        public async Task ParseBasicPodcastFeedTest()
        {
            var feed = await PodcastFeed.LoadFeedAsync(TestFeeds.GetFeedUri("basic01.xml"));

            Assert.IsTrue(feed.PubDate.HasValue);

            var date = feed.PubDate.Value;

            Assert.AreEqual("A Basic RSS Feed", feed.Title);
            Assert.AreEqual("This is a super simple RSS feed", feed.Description);
            Assert.AreEqual(new DateTime(2002, 9, 7, 0, 0, 1, DateTimeKind.Utc), date.ToUniversalTime());
        }

        [TestMethod]
        public async Task BeastcastTest()
        {
            var beastcast = await PodcastFeed.LoadFeedAsync(TestFeeds.GetFeedUri("beastcast.xml"));

            Assert.AreEqual("The Giant Beastcast", beastcast.Title);
            Assert.AreEqual(12, beastcast.Items.Count());
            Assert.AreEqual("Giant Bomb", beastcast.ITunes.Author);
        }
    }
}