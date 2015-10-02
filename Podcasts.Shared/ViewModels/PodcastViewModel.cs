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
        private Podcast Podcast;
        private PodcastManager Manager;
        internal PodcastFeed Feed;

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

        internal PodcastViewModel(PodcastManager manager, Podcast podcast)
        {
            if (podcast.Id == Guid.Empty)
            {
                throw new ArgumentException("Podcast must have an id!", nameof(podcast));
            }

            Manager = manager;
            Podcast = podcast;
            PlayEpisode = new PlayEpisodeCommand(this);
        }

        public string Title => Podcast.Title;

        public Uri Image => Podcast.Image;

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
                Feed = await Manager.GetPodcastFeedAsync(Podcast);

                Episodes = new EpisodeList(Feed);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        public PlayEpisodeCommand PlayEpisode { get; private set; }
    }
}