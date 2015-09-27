using System;
using Windows.Media.Core;

namespace Podcasts.ViewModels
{
    using System.Threading.Tasks;
    using Dom;
    using Utilities;

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

        public EpisodeViewModel(PodcastFeedItem item)
        {
            Guid = item.Guid;
            Title = item.Title;
            _source = item.Enclosure.Url;
        }

        private Task<TimeSpan> GetDurationAsync()
        {
            return Task.Run(async () =>
            {
                using (var sourceReader = await MediaFoundation.SourceReader.CreateFromUriAsync(_source))
                {
                    return sourceReader.GetDurationBlocking();
                }
            });
        }

        internal async Task UpdateEpisodeAsync()
        {
            if (!Duration.HasValue)
            {
                Duration = await GetDurationAsync();
            }
        }
    }
}