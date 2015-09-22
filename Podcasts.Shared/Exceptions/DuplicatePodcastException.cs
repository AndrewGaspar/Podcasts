using System;

namespace Podcasts.Exceptions
{
    public class DuplicatePodcastException : Exception
    {
        public Uri Location { get; private set; }

        public DuplicatePodcastException(Uri location) :
            base($"Attempted to add a new podcast at {location.ToString()} even though one already existed from there!")
        {
            Location = location;
        }
    }
}