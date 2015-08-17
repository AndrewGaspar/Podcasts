using System;
using Podcasts.Messages;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;

namespace Podcasts.Transport
{
    public class BackgroundMessageTransport : MessageTransport
    {
        public void Start()
        {
            AttachEventHandlers();
        }
        
        public void Stop()
        {
            DetachEventHandlers();
        }
        
        private void OnServiceReady()
        {
            NotifyForeground();
        }

        public void NotifyForeground()
        {
            SendMessage(new ServiceReadyNotification());
        }

        public event TypedEventHandler<BackgroundMessageTransport, PlayEpisodeRequest> PlaybackRequested;
        private void NotifyPlaybackRequested(PlayEpisodeRequest request)
        {
            var callback = PlaybackRequested;
            if(callback != null)
            {
                callback(this, request);
            }
        }

        protected override void SendMessageRaw(ValueSet message) => 
            BackgroundMediaPlayer.SendMessageToForeground(message);

        protected override void AttachEventHandlersInternal()
        {
            BackgroundMediaPlayer.MessageReceivedFromForeground += OnMessageReceived;
        }

        protected override void DetachEventHandlersInternal()
        {
            BackgroundMediaPlayer.MessageReceivedFromForeground -= OnMessageReceived;
        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void HandleMessage(ValueSet message)
        {
            new MessageParseHelper()
                .Try<ServiceReadyRequest>(_ => OnServiceReady())
                .Try<PlayEpisodeRequest>(NotifyPlaybackRequested)
                .Invoke(message);
        }
    }
}
