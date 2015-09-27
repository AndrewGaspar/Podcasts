using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Podcasts.ViewModels
{
    using Dom;
    using Models;
    using Collections;

    public class EpisodeList : IncrementalLoadingCollection<EpisodeViewModel>
    {
        private PodcastFeed Feed;

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

        protected override async Task<EpisodeViewModel> LoadItemAsync(uint index)
        {
            var episodeViewModel = new EpisodeViewModel(Feed.Items[(int)index]);
            await episodeViewModel.UpdateEpisodeAsync();
            return episodeViewModel;
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
    }
}