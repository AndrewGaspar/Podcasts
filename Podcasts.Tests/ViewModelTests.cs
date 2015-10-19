using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Podcasts.Tests
{
    using ViewModels;

    [TestClass]
    public class ViewModelTests
    {
        private AppViewModel AppViewModel = new AppViewModel("test-database.xml");

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
            AppViewModel.Podcasts.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
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