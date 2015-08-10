﻿using System;
using PodcastsService.Messages;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;

namespace PodcastsService
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

        public event TypedEventHandler<BackgroundMessageTransport, PlayEpisodeRequest> OnPlaybackRequested;

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
            {
                ServiceReadyRequest request;
                if(MessageHelper.TryParseMessage(message, out request))
                {
                    OnServiceReady();
                    return;
                }
            }

            {
                PlayEpisodeRequest request;
                if(MessageHelper.TryParseMessage(message, out request))
                {
                    OnPlaybackRequested(this, request);
                    return;
                }
            }
        }
    }
}
