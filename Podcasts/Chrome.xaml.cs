using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Media.Playback;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Podcasts
{
    using Commands;
    using Dom;
    using Messages;
    using Models;
    using ViewModels;

    public enum SplitViewState
    {
        Open,
        Close,
        Toggle,
    };

    public class OpenSplitViewCommand : CommandBase<SplitViewState?>
    {
        private SplitView _splitView;

        internal SplitView SplitView
        {
            get
            {
                return _splitView;
            }
            set
            {
                _splitView = value;
            }
        }

        public OpenSplitViewCommand()
        {
        }

        private SplitViewState CurrentState => SplitView.IsPaneOpen ? SplitViewState.Open : SplitViewState.Close;

        public override bool CanExecute(SplitViewState? parameter)
        {
            return parameter != null;
        }

        public override void Execute(SplitViewState? desiredState)
        {
            if (!desiredState.HasValue)
            {
                return;
            }

            if (desiredState == SplitViewState.Toggle)
            {
                desiredState = CurrentState == SplitViewState.Open ? SplitViewState.Close : SplitViewState.Open;
            }

            SplitView.IsPaneOpen = (desiredState == SplitViewState.Open);
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Chrome : Page
    {
        public App App => (App)Application.Current;

        public AppViewModel ViewModel => App.ViewModel;

        public readonly SplitViewState HamburgerCommandParameter = SplitViewState.Toggle;
        public OpenSplitViewCommand HamburgerCommand { get; private set; } = new OpenSplitViewCommand();

        public bool NavigateTo(Type destination, object arguments) => RootFrame.Navigate(destination, arguments);

        public Chrome()
        {
            this.InitializeComponent();

            this.HamburgerCommand.SplitView = MainSplitView;

            this.DataContext = this;

            this.RootFrame.NavigationFailed += this.OnNavigationFailed;
        }

        public void AppLaunched(LaunchActivatedEventArgs e)
        {
            if (this.RootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                this.RootFrame.Navigate(typeof(Views.PodcastsListPage), e.Arguments);
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private async Task AddPodcastAsync(Uri url)
        {
            var document = await PodcastFeed.LoadFeedAsync(url);

            var title = document.Title;

            //if(title != null) CurrentPodcastName.Text = title;

            var firstItem = document.Items.First();

            var enclosure = firstItem.Enclosure;

            if (enclosure == null)
            {
                var dialog = new MessageDialog("Item has no content.", "Error!");
                await dialog.ShowAsync();
                return;
            }

            var itunesImage = firstItem.ITunes.Image;

            var episodeTitle = firstItem.Title;

            var location = enclosure.Url;

            var image = itunesImage?.Href;

            var episode = new Episode()
            {
                Title = episodeTitle,
                Location = location,
                Image = image,
                PodcastName = firstItem.Title,
            };

            if (MediaPlayerState.Closed == BackgroundMediaPlayer.Current.CurrentState)
            {
                await App.MessageService.PingServiceAsync();
            }

            App.MessageService.RequestPlayback(new PlayEpisodeRequest { Episode = episode });
        }
    }
}