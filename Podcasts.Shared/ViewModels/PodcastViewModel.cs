using System;
using System.Threading;
using System.Threading.Tasks;

namespace Podcasts.ViewModels
{
    using Collections;
    using Dom;
    using Models;
    using Commands;
    using Utilities;
    using DataObjects;

    public class EpisodeList : IncrementalLoadingCollection<EpisodeViewModel>
    {
        private PodcastFeed Feed;
        private CancellationTokenSource TokenSource = new CancellationTokenSource();

        public EpisodeList(PodcastFeed feed)
        {
            Feed = feed;
        }

        protected override uint MaxItems
        {
            get
            {
                return (uint)Feed.Items.Count;
            }
        }

        protected override Task<EpisodeViewModel> LoadItemAsync(uint index)
        {
            var episodeViewModel = new EpisodeViewModel(Feed.Items[(int)index]);
            // don't care to block on this - would rather display available information while waiting
            episodeViewModel.UpdateEpisodeAsync(TokenSource.Token).Ignore();
            return Task.FromResult(episodeViewModel);
        }

        public void CancelUpdating()
        {
            TokenSource.Cancel();
        }
    }

    public class PodcastViewModel : BaseViewModel
    {
        private SubscriptionModel Model;

        private EpisodeList _episodes;

        public EpisodeList Episodes
        {
            get
            {
                return _episodes;
            }
            private set
            {
                _episodes = value;
                NotifyPropertyChanged();
            }
        }

        public PodcastViewModel(SubscriptionModel subscriptionModel)
        {
            if (subscriptionModel.Subscription.Id == null || subscriptionModel.Subscription.Id == "")
            {
                throw new ArgumentException("Subscription must have an id!", nameof(subscriptionModel));
            }
            
            Model = subscriptionModel;
            PlayEpisode = new PlayEpisodeCommand(this);
        }

        public string Title => Model.Title;

        public Uri Image => Model.Image;

        private bool _isRefreshing = false;

        public bool IsRefreshing
        {
            get
            {
                return _isRefreshing;
            }
            private set
            {
                _isRefreshing = value;
                NotifyPropertyChanged();
            }
        }

        public async Task RefreshAsync()
        {
            IsRefreshing = true;

            try
            {
                await Model.InitializeAsync();

                Episodes = new EpisodeList(Model.Feed);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        public PlayEpisodeCommand PlayEpisode { get; private set; }
    }
}