using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Podcasts.Views
{
    using System.Diagnostics;
    using ViewModels;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PodcastPage : Page
    {
        public PodcastViewModel ViewModel { get; private set; }

        public PodcastPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ViewModel = e.Parameter as PodcastViewModel;

            Debug.Assert(ViewModel != null);

            await ViewModel.RefreshAsync();

            this.DataContext = this;
        }
    }
}