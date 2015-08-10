using System;
using System.Threading.Tasks;
using Podcasts.Messages;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;

namespace Podcasts
{
    using ServiceReadyHandler = TypedEventHandler<ForeroundMessageTransport, ServiceReadyNotification>;

    public class ForeroundMessageTransport : MessageTransport
    {
        public void Start()
        {
            AttachEventHandlers();
        }

        public void Stop()
        {
            DetachEventHandlers();
        }


        // Internal events
        private event ServiceReadyHandler ServiceReady;
        
        protected override void AttachEventHandlersInternal()
        {
            BackgroundMediaPlayer.MessageReceivedFromBackground += OnMessageReceived;
        }

        protected override void DetachEventHandlersInternal()
        {
            BackgroundMediaPlayer.MessageReceivedFromBackground -= OnMessageReceived;
        }
        
        private void SendPing()
        {
            SendMessage(new ServiceReadyRequest());
        }

        public void RequestPlayback(PlayEpisodeRequest request)
        {
            SendMessage(request);
        }
        
        public async Task PingServiceAsync()
        {
            var tcs = new TaskCompletionSource<int>();
            
            ServiceReadyHandler callback = (sender, e) => tcs.SetResult(1);
            
            ServiceReady += callback;
            SendPing();
            var result = await Task.WhenAny(tcs.Task, Task.Delay(10000));
            ServiceReady -= callback;

            if(result != tcs.Task)
            {
                throw new Exception("Background audio service ping timed out.");
            }
        }

        protected override void HandleMessage(ValueSet message)
        {
            {
                ServiceReadyNotification sr;
                if (MessageHelper.TryParseMessage(message, out sr))
                {
                    if (ServiceReady != null) ServiceReady(this, sr);
                    return;
                }
            }
        }

        private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            throw new NotImplementedException();
        }

        protected override void SendMessageRaw(ValueSet message) => BackgroundMediaPlayer.SendMessageToBackground(message);
    }
}
