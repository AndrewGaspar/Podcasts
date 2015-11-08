using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Podcasts.Views
{
    using Models;
    using ViewModels;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Navigation;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PodcastsListPage : Page
    {
        public PodcastListViewModel ViewModel
        {
            get;
        } = new PodcastListViewModel();

        public NavigationCommand<PodcastPage, SubscriptionModel> NavigateToPodcastPage { get; }
            = new NavigationCommand<PodcastPage, SubscriptionModel>();

        public PodcastsListPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await SubscriptionManager.Current.RefreshAsync();
        }

        private void Subscription_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

            e.Handled = true;
        }
    }
}