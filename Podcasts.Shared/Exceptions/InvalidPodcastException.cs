using System;

namespace Podcasts.Exceptions
{
    public class InvalidPodcastException : Exception
    {
        public Uri Podcast { get; private set; }
        public string Reason { get; private set; }

        public InvalidPodcastException(Uri podcast, string reason) : base($"Podcast {podcast} is invalid because: {reason}")
        {
            Reason = reason;
            Podcast = podcast;
        }
    }
}