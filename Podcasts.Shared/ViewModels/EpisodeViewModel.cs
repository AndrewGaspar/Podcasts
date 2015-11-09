using System;
using System.Threading;
using System.Threading.Tasks;

namespace Podcasts.ViewModels
{
    using Dom;

    public class EpisodeViewModel : BaseViewModel
    {
        #region Readonly Properties

        public Uri Source { get; private set; }

        public string Guid { get; private set; }

        public string Title { get; private set; }

        public Uri Image { get; private set; }

        #endregion Readonly Properties

        #region Awaits additional information

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

        #endregion Awaits additional information

        public EpisodeViewModel(PodcastFeedItem item)
        {
            Guid = item.Guid.Value;
            Title = item.Title;
            Image = item.ITunes?.Image?.Href;
            Source = item.Enclosure.Url;
            
            var duration = item.ITunes?.Duration;
            if(duration == null || duration?.Ticks == 0)
            {
                Duration = null;
            }
            else
            {
                Duration = duration;
            }
        }

        private Task<TimeSpan?> GetDurationAsync(CancellationToken token)
        {
            return Task.Run<TimeSpan?>(async () =>
            {
                if (token.IsCancellationRequested) return null;

                using (var sourceReader = await MediaFoundation.SourceReader.CreateFromUriAsync(Source))
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