﻿using System;
using System.Threading.Tasks;

namespace Podcasts.ViewModels
{
    using System.Threading;
    using Dom;

    public class EpisodeViewModel : BaseViewModel
    {
        private Uri _source;

        public string Guid { get; private set; }

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

        private TimeSpan? _duration;

        public TimeSpan? Duration
        {
            get
            {
                return _duration;
            }
            private set
            {
                _duration = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isUpdating = false;

        public bool IsUpdating
        {
            get
            {
                return _isUpdating;
            }
            private set
            {
                _isUpdating = value;
                NotifyPropertyChanged();
            }
        }

        public EpisodeViewModel(PodcastFeedItem item)
        {
            Guid = item.Guid;
            Title = item.Title;
            _source = item.Enclosure.Url;
        }

        private Task<TimeSpan?> GetDurationAsync(CancellationToken token)
        {
            return Task.Run<TimeSpan?>(async () =>
            {
                if (token.IsCancellationRequested) return null;

                using (var sourceReader = await MediaFoundation.SourceReader.CreateFromUriAsync(_source))
                {
                    if (token.IsCancellationRequested) return null;

                    return sourceReader.GetDurationBlocking();
                }
            });
        }

        internal async Task UpdateEpisodeAsync(CancellationToken token)
        {
            if (IsUpdating)
            {
                return;
            }

            IsUpdating = true;

            try
            {
                if (!Duration.HasValue)
                {
                    Duration = await GetDurationAsync(token);
                }
            }
            finally
            {
                IsUpdating = false;
            }
        }
    }
}