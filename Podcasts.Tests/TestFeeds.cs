using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
