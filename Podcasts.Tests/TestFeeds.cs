using System;

namespace Podcasts.Tests
{
    public static class TestFeeds
    {
        public static Uri BaseUri = new Uri("ms-appx:///TestFeeds/");

        public static Uri GetFeedUri(string feedName)
        {
            return new Uri(BaseUri, feedName);
        }
    }
}