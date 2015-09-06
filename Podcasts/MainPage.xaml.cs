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
    public sealed partial class MainPage : Page
    {
        public App App => (App)Application.Current;

        public AppViewModel ViewModel => App.AppViewModel;

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public class PresentationBindings : PropertyChangeBase
        {
            private UISettings UISettings { get; } = new UISettings();

            public PresentationBindings()
            {
                UpdateAppBarBrush();
                
                UISettings.ColorValuesChanged += UISettings_ColorValuesChanged;
            }

            private void UISettings_ColorValuesChanged(UISettings sender, object args)
            {
                UpdateAppBarBrush();
            }

            private void UpdateAppBarBrush()
            {
                AppBarBrush = new SolidColorBrush(UISettings.GetColorValue(UIColorType.AccentDark1));
            }
            
            private Brush _appBarBrush;
            public Brush AppBarBrush
            {
                get
                {
                    return _appBarBrush;
                }
                private set
                {
                    _appBarBrush = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public PresentationBindings Presentation { get; } = new PresentationBindings();

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
