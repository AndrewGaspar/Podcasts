using System;
using System.Threading.Tasks;

namespace Podcasts.Models
{
    using DataObjects;
    using Dom;
    using Utilities;

    public class SubscriptionModel : PropertyChangeBase
    {
        public Subscription Subscription { get; private set; }
        public PodcastFeed Feed { get; private set; }

        private bool _isLoaded;
        public bool IsLoaded
        {
            get
            {
                return _isLoaded;
            }
            private set
            {
                _isLoaded = value;
                NotifyPropertyChanged();
            }
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            private set
            {
                _title = value;
                NotifyPropertyChanged();
            }
        }

        private Uri _image;
        public Uri Image
        {
            get
            {
                return _image;
            }
            private set
            {
                _image = value;
                NotifyPropertyChanged();
            }
        }

        public SubscriptionModel(Subscription subscription, PodcastFeed feed = null)
        {
            Subscription = subscription;
            Feed = feed;

            InitializeAsync().Ignore();
        }

        private Task _initializeTask;
        public Task InitializeAsync() => _initializeTask ?? (_initializeTask = InitializeAsyncInner());

        private async Task InitializeAsyncInner()
        {
            try
            {
                Feed = Feed ?? await PodcastFeed.LoadFeedAsync(new Uri(Subscription.LocationHref));

                Title = Feed.Title;
                Image = Feed?.Image?.Url;

                IsLoaded = true;
            }
            catch
            {
                Feed = null;
                Title = null;
                Image = null;
                
                IsLoaded = false;
            }
        }
    }
}
