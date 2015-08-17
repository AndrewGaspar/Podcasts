using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Podcasts.Dom;
using Podcasts.Messages;
using Podcasts.Models;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.Input;
using Windows.Media.Playback;
using Windows.UI.Popups;
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
    public sealed partial class MainPage : Page
    {
        private App PodcastsApp => (App)Application.Current;

        public MainPage()
        {
            this.InitializeComponent();
            
            //LoadPodcast();
        }

        public async void LoadPodcast()
        {
            var document = await XmlDocument.LoadFromUriAsync(new Uri("http://www.giantbomb.com/podcast-xml/giant-bombcast/"));
            
        }
        
        private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            //throw new NotImplementedException();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }

        private async void AddPodcastButton_Click(object sender, RoutedEventArgs e)
        {
            Uri location;
            if(Uri.TryCreate(AddPodcastUrl.Text, UriKind.Absolute, out location))
            {
                await AddPodcastAsync(location);
            }
        }
        
        
        private async Task AddPodcastAsync(Uri url)
        {
            var document = await PodcastFeed.LoadFeedAsync(url);

            var title = document.Title;

            if(title != null) CurrentPodcastName.Text = title;

            var firstItem = document.Items.First();

            var enclosure = firstItem.Enclosure;

            if(enclosure == null)
            {
                var dialog = new MessageDialog("Item has no content.", "Error!");
                await dialog.ShowAsync();
                return;
            }

            var itunesImage = firstItem.Item.ChildNodes.FirstOrDefault(node => node.NodeName == "itunes:image");
            
            var episodeTitle = firstItem.Title;

            var location = enclosure.Url;
            
            var image = itunesImage == null ? null : new Uri(itunesImage.Attributes.GetNamedItem("href").InnerText);

            var episode = new Episode()
            {
                Title = episodeTitle,
                Location = location,
                Image = image,
                PodcastName = firstItem.Title,
            };

            if (MediaPlayerState.Closed == BackgroundMediaPlayer.Current.CurrentState)
            {
                await PodcastsApp.MessageService.PingServiceAsync();
            }

            PodcastsApp.MessageService.RequestPlayback(new PlayEpisodeRequest { Episode = episode });
        }
    }
}
