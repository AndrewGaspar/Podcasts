using Windows.UI.Xaml.Controls;

namespace Podcasts.Views
{
    public abstract class AppPage : Page
    {
        public abstract string Title { get; }
    }
}