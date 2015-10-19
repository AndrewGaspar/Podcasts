using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media.Playback;
using Windows.UI.Core;

namespace Podcasts.ViewModels
{
    using Commands;
    using Transport;
    using Utilities;

    public class AddPodcastCommand : CommandBase<string>
    {
        private bool _isAdding = false;

        public bool IsAdding
        {
            get
            {
                return _isAdding;
            }
            set
            {
                ReevaluateCanExecute(null, () =>
                {
                    _isAdding = value;
                    NotifyPropertyChanged();
                });
            }
        }

        private AppViewModel ViewModel { get; set; }

        public AddPodcastCommand(AppViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public override bool CanExecute(string _newPodcastUri)
        {
            if (_newPodcastUri == "" || _newPodcastUri == null)
            {
                return false;
            }

            Uri _;
            return Uri.TryCreate(_newPodcastUri, UriKind.Absolute, out _);
        }

        public override async void Execute(string _newPodcastUri)
        {
            IsAdding = true;

            try
            {
                await ViewModel.AddPodcastAsync(new Uri(_newPodcastUri, UriKind.Absolute)).ConfigureAwait(false);
            }
            finally
            {
                IsAdding = false;
            }
        }

        public Task AwaitsDoneAddingAsync()
        {
            return AwaitsPropertyChangeAsync(nameof(IsAdding), false, () => IsAdding);
        }
    }

    /// <summary>
    /// There should only be one instance of this object per app.
    /// </summary>
    public class AppViewModel : BaseViewModel
    {
        private const string V1PodcastDatabase = "podcasts-v1.json-db";

        public static AppViewModel Current { get; } = new AppViewModel(V1PodcastDatabase);

        private TimeSpan? _duration, _position;

        public TimeSpan? CurrentPodcastDuration
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

        public bool IsSettingPodcastPosition
        {
            get; set;
        }

        public TimeSpan? CurrentPodcastPosition
        {
            get
            {
                return _position;
            }
            private set
            {
                _position = value;
                NotifyPropertyChanged();
            }
        }

        private void UpdateMediaInfo(MediaPlayer player)
        {
            if (player.NaturalDuration == TimeSpan.MinValue)
            {
                CurrentPodcastDuration = null;
                CurrentPodcastPosition = null;
            }
            else
            {
                CurrentPodcastDuration = player.NaturalDuration;

                if (!IsSettingPodcastPosition)
                {
                    CurrentPodcastPosition = player.Position;
                }
            }
        }

        public SemaphoreSlim UpdatePositionMutex { get; } = new SemaphoreSlim(1, 1);

        private bool IsPositionUpdateWorkItemRunning { get; set; }

        private Task QueueUpdateWorkItem() =>
            CoreApplication.MainView.CoreWindow.Dispatcher.RunIdleAsync(_ => PositionUpdateWorkItem()).AsTask();

        private async void PositionUpdateWorkItem()
        {
            await UpdatePositionMutex.ExclusionRegionAsync(() =>
            {
                var player = BackgroundMediaPlayer.Current;

                if (player != null)
                {
                    UpdateMediaInfo(player);
                }

                if (!IsPositionUpdateWorkItemRunning)
                {
                    return;
                }
            });

            await Task.Delay(100);

            await QueueUpdateWorkItem();
        }

        private Task RunPositionUpdateBackgroundItem() =>
            UpdatePositionMutex.ExclusionRegionAsync(async () =>
            {
                if (IsPositionUpdateWorkItemRunning)
                {
                    return;
                }

                IsPositionUpdateWorkItemRunning = true;

                await QueueUpdateWorkItem();
            });

        private void StopUpdateBackgroundItem() =>
            UpdatePositionMutex.ExclusionRegionAsync(() =>
            {
                IsPositionUpdateWorkItemRunning = false;
            });

        public void ScrubTo(TimeSpan time)
        {
            BackgroundMediaPlayer.Current.Position = time;
        }

        public AppViewModel(string databaseName)
        {
            AddPodcastCommand = new AddPodcastCommand(viewModel: this);

            if (BackgroundMediaPlayer.Current != null)
            {
                UpdateMediaInfo(BackgroundMediaPlayer.Current);
            }

            BackgroundMediaPlayer.Current.MediaOpened += MediaOpened;
        }

        private async void MediaOpened(MediaPlayer sender, object args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                RunPositionUpdateBackgroundItem();
            });
        }

        private PodcastManager Manager = new PodcastManager(V1PodcastDatabase);
        internal ForeroundMessageTransport Transport { get; } = new ForeroundMessageTransport();

        private ObservableCollection<PodcastViewModel> _podcasts = new ObservableCollection<PodcastViewModel>();
        public ObservableCollection<PodcastViewModel> Podcasts => _podcasts;

        public AddPodcastCommand AddPodcastCommand { get; private set; }

        public EpisodeViewModel CurrentEpisode { get; private set; }

        private bool _isInitializing = false;

        public bool IsInitializing
        {
            get
            {
                return _isInitializing;
            }
            private set
            {
                _isInitializing = value;
                NotifyPropertyChanged();
            }
        }

        private bool IsInitialized { get; set; } = false;

        private void AppendPodcastsToList(IEnumerable<PodcastViewModel> podcasts)
        {
            foreach (var podcast in podcasts)
            {
                Podcasts.Add(podcast);
            }
        }

        private void AppendPodcastToList(PodcastViewModel podcast)
        {
            AppendPodcastsToList(new[] { podcast });
        }

        public async Task InitializeAsync()
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitializing = true;

            try
            {
                var podcasts = await Manager.GetPodcastsAsync().ConfigureAwait(false);

                AppendPodcastsToList(from podcast in podcasts select new PodcastViewModel(Manager, podcast));
            }
            finally
            {
                IsInitialized = true;
                IsInitializing = false;
            }
        }

        public async Task ClearAsync()
        {
            await Manager.ClearDatabaseAsync().ConfigureAwait(false);

            Podcasts.Clear();
        }

        public async Task AddPodcastAsync(Uri url)
        {
            // No ConfigureAwait(false) here because AppendPodcastToList must invoke on the UI thread
            var podcast = await Manager.AddPodcastAsync(url);

            AppendPodcastToList(new PodcastViewModel(Manager, podcast));
        }
    }
}