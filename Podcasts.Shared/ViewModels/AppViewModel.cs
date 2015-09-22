using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Podcasts.ViewModels
{
    using Commands;

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

        public AppViewModel(string databaseName = V1PodcastDatabase)
        {
            AddPodcastCommand = new AddPodcastCommand(viewModel: this);
        }

        private PodcastManager Manager = new PodcastManager(V1PodcastDatabase);

        private ObservableCollection<PodcastViewModel> _podcasts = new ObservableCollection<PodcastViewModel>();
        public ObservableCollection<PodcastViewModel> Podcasts => _podcasts;

        public AddPodcastCommand AddPodcastCommand { get; private set; }

        public PodcastViewModel CurrentPodcast { get; private set; }

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