using System;
using System.Threading;
using Podcasts;
using Podcasts.Messages;
using Windows.ApplicationModel.Background;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace PodcastsBackground
{
    public sealed class BackgroundAudioTask : IBackgroundTask
    {
        private SystemMediaTransportControls mediaTransportControls;
        private BackgroundTaskDeferral deferral;
        private BackgroundMessageTransport messageTransport = new BackgroundMessageTransport();
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

            messageTransport.OnPlaybackRequested += OnPlaybackRequested;

            messageTransport.Start();
            messageTransport.NotifyForeground();

            deferral = taskInstance.GetDeferral();

            backgroundTaskStarted.Set();

            taskInstance.Task.Completed += TaskCompleted;
            taskInstance.Canceled += OnCanceled;
        }

        private void OnPlaybackRequested(BackgroundMessageTransport sender, PlayEpisodeRequest args)
        {
            var episode = args.Episode;

            var source = MediaSource.CreateFromUri(episode.Location);
            BackgroundMediaPlayer.Current.AutoPlay = true;

            mediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
            mediaTransportControls.DisplayUpdater.Type = MediaPlaybackType.Music;
            mediaTransportControls.DisplayUpdater.MusicProperties.Title = episode.Title;
            mediaTransportControls.DisplayUpdater.MusicProperties.Artist = episode.PodcastName;
            if (episode.Image != null)
            {
                mediaTransportControls.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(episode.Image);
            }

            mediaTransportControls.DisplayUpdater.Update();

            BackgroundMediaPlayer.Current.Source = source;
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            messageTransport.Stop();
            
            deferral.Complete();
        }

        private void TaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            deferral.Complete();
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
            switch(args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    BackgroundMediaPlayer.Current.Play();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    BackgroundMediaPlayer.Current.Pause();
                    break;
            }
        }
    }
}
