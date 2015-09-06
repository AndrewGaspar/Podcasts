using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Podcasts.Dom;
using Podcasts.Models;

namespace Podcasts.ViewModels
{
    public class PodcastViewModel : BaseViewModel
    {
        private Podcast Podcast;
        private PodcastManager Manager;
        private PodcastFeed Feed;

        internal PodcastViewModel(PodcastManager manager, Podcast podcast)
        {
            if(podcast.Id == Guid.Empty)
            {
                throw new ArgumentException("Podcast must have an id!", nameof(podcast));
            }

            Manager = manager;
            Podcast = podcast;
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
            }
            finally
            {
                IsRefreshing = false;
            }
        }

    }
}
