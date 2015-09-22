using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Podcasts.ViewModels;

namespace Podcasts.Tests
{
    [TestClass]
    public class ViewModelTests
    {
        private AppViewModel AppViewModel = new AppViewModel();

        [TestInitialize]
        public async Task Initialize()
        {
            await AppViewModel.ClearAsync();
        }

        [TestMethod]
        public async Task EnsureNotificationOnAdd()
        {
            var podcastUri = TestFeeds.GetFeedUri("beastcast.xml").ToString();

            Assert.IsTrue(AppViewModel.AddPodcastCommand.CanExecute(podcastUri));

            var podcastsChanged = false;
            AppViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(AppViewModel.Podcasts))
                {
                    podcastsChanged = true;
                }
            };

            AppViewModel.AddPodcastCommand.Execute(podcastUri);

            await AppViewModel.AddPodcastCommand.AwaitsDoneAddingAsync();

            Assert.IsTrue(podcastsChanged);
        }
    }
}