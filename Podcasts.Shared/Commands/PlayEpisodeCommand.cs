using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Podcasts.Commands
{
    using ViewModels;

    public class PlayEpisodeCommand : CommandBase<EpisodeViewModel>
    {
        private PodcastViewModel _podcast;

        public PlayEpisodeCommand(PodcastViewModel podcast)
        {
            _podcast = podcast;
        }

        public override bool CanExecute(EpisodeViewModel parameter) => true;

        public override void Execute(EpisodeViewModel episode)
        {
            AppViewModel.Current.Transport.RequestPlayback(new Messages.PlayEpisodeRequest()
            {
                Episode = new Models.Episode
                {
                    Title = episode.Title,
                    Location = episode.Source,
                    Image = episode.Image,
                    PodcastName = _podcast.Title,
                },
            });
        }
    }
}