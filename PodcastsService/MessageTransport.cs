using System;
using System.Threading;
using Windows.Foundation.Collections;
using Windows.Media.Playback;

namespace Podcasts
{
    public abstract class MessageTransport
    {
        public DateTime? LastMessageReceivedUtc { get; private set; } = null;
        public DateTime? LastMessageReceivedLocalized => LastMessageReceivedUtc?.ToLocalTime();
        public TimeSpan? TimeSinceLastMessage 
            => LastMessageReceivedUtc.HasValue ? DateTime.UtcNow - LastMessageReceivedUtc : null;

        protected Mutex Mutex = new Mutex();
        private bool EventHandlersAttached = false;
        
        protected abstract void HandleMessage(ValueSet message);
        protected void OnMessageReceived(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            LastMessageReceivedUtc = DateTime.UtcNow;
            
            HandleMessage(e.Data);
        }

        protected abstract void AttachEventHandlersInternal();
        protected void AttachEventHandlers()
        {
            if (!EventHandlersAttached)
            {
                Mutex.Acquire(() =>
                {
                    if (!EventHandlersAttached)
                    {
                        AttachEventHandlersInternal();
                    }

                    EventHandlersAttached = true;
                });
            }
        }

        protected abstract void DetachEventHandlersInternal();
        protected void DetachEventHandlers()
        {
            if (EventHandlersAttached)
            {
                Mutex.Acquire(() =>
                {
                    if (EventHandlersAttached)
                    {
                        DetachEventHandlersInternal();
                    }

                    EventHandlersAttached = false;
                });
            }
        }

        protected abstract void SendMessageRaw(ValueSet message);

        protected void SendMessage<T>(T message)
        {
            SendMessageRaw(MessageHelper.ToMessage(message));
        }
    }
}
