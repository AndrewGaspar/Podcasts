using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        private AutoResetEvent backgroundAudioTaskStarted = new AutoResetEvent(false);

        public MainPage()
        {
            this.InitializeComponent();

            LoadPodcast();
        }

        public async void LoadPodcast()
        {
            var document = await XmlDocument.LoadFromUriAsync(new Uri("http://www.giantbomb.com/podcast-xml/giant-bombcast/"));

            var title = document.SelectNodes("/rss/channel/title").First();

            CurrentPodcastName.Text = title.InnerText;

            var firstItem = document.SelectSingleNode("/rss/channel/item");

            var enclosure = firstItem.SelectSingleNode("enclosure");
            var urlItem = enclosure.Attributes.GetNamedItem("url").InnerText;

            if (MediaPlayerState.Closed == BackgroundMediaPlayer.Current.CurrentState)
            {
                await StartBackgroundAudioTask();
            }

            BackgroundMediaPlayer.SendMessageToBackground(new ValueSet() {["podcast"] = urlItem });
        }

        private void AddMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        private async Task StartBackgroundAudioTask()
        {
            AddMediaPlayerEventHandlers();

            var tcs = new TaskCompletionSource<int>();
            
            var startResult = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                var result = backgroundAudioTaskStarted.WaitOne(10000);
                if(result)
                {
                    tcs.SetResult(0);
                }
                else
                {
                    tcs.SetException(new Exception("Background Audio Task didn't start"));
                }
            });

            await tcs.Task;


        }

        private void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            backgroundAudioTaskStarted.Set();
        }

        private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            //throw new NotImplementedException();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }
    }
}
