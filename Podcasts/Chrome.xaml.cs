using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Podcasts.Dom;
using Podcasts.Messages;
using Podcasts.Models;
using Podcasts.ViewModels;
using Windows.ApplicationModel.Activation;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.Input;
using Windows.Media.Playback;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Podcasts
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Chrome : Page
    {
        public App App => (App)Application.Current;

        public AppViewModel ViewModel => App.ViewModel;

        public bool NavigateTo(Type destination, object arguments) => RootFrame.Navigate(destination, arguments);

        public Chrome()
        {
            this.InitializeComponent();
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
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }
        
        
        private async Task AddPodcastAsync(Uri url)
        {
            var document = await PodcastFeed.LoadFeedAsync(url);

            var title = document.Title;

            //if(title != null) CurrentPodcastName.Text = title;

            var firstItem = document.Items.First();

            var enclosure = firstItem.Enclosure;

            if(enclosure == null)
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
