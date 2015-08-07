using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace PodcastsBackground
{
    public sealed class BackgroundAudioTask : IBackgroundTask
    {
        private SystemMediaTransportControls mediaTransportControls;
        private BackgroundTaskDeferral deferral;
        private ManualResetEvent backgroundTaskStarted = new ManualResetEvent(false);

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            mediaTransportControls = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            mediaTransportControls.ButtonPressed += MediaButtonPressed;
            mediaTransportControls.PropertyChanged += MediaPropertyChanged;
            mediaTransportControls.IsEnabled = true;
            mediaTransportControls.IsPauseEnabled = true;
            mediaTransportControls.IsPlayEnabled = true;
            mediaTransportControls.IsNextEnabled = true;
            mediaTransportControls.IsPreviousEnabled = true;

            BackgroundMediaPlayer.Current.CurrentStateChanged += MediaPlayerStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromForeground += OnMessageReceived;


            BackgroundMediaPlayer.SendMessageToForeground(new ValueSet());

            deferral = taskInstance.GetDeferral();

            backgroundTaskStarted.Set();

            taskInstance.Task.Completed += TaskCompleted;
            taskInstance.Canceled += OnCanceled;
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //throw new NotImplementedException();
        }

        private void TaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            deferral.Complete();
        }

        private void OnMessageReceived(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            var url = e.Data["podcast"] as string;

            var source = MediaSource.CreateFromUri(new Uri(url));
            BackgroundMediaPlayer.Current.Source = source;
        }

        private void MediaPlayerStateChanged(MediaPlayer sender, object args)
        {
            //throw new NotImplementedException();
        }

        private void MediaPropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            //throw new NotImplementedException();
        }

        private void MediaButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            //throw new NotImplementedException();
        }
    }
}
