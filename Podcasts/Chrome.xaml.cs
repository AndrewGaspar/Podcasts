using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Podcasts
{
    using System.ComponentModel;
    using Commands;
    using Utilities;
    using ViewModels;
    using Windows.UI.Xaml.Controls.Primitives;

    public enum SplitViewState
    {
        Open,
        Close,
        Toggle,
    };

    public class OpenSplitViewCommand : CommandBase<SplitViewState?>
    {
        private SplitView _splitView;

        internal SplitView SplitView
        {
            get
            {
                return _splitView;
            }
            set
            {
                _splitView = value;
            }
        }

        public OpenSplitViewCommand()
        {
        }

        private SplitViewState CurrentState => SplitView.IsPaneOpen ? SplitViewState.Open : SplitViewState.Close;

        public override bool CanExecute(SplitViewState? parameter)
        {
            return parameter != null;
        }

        public override void Execute(SplitViewState? desiredState)
        {
            if (!desiredState.HasValue)
            {
                return;
            }

            if (desiredState == SplitViewState.Toggle)
            {
                desiredState = CurrentState == SplitViewState.Open ? SplitViewState.Close : SplitViewState.Open;
            }

            SplitView.IsPaneOpen = (desiredState == SplitViewState.Open);
        }
    }

    public class NavigationCommand<PageT, Args> : CommandBase<Args>
        where PageT : Page
    {
        public override bool CanExecute(Args parameter) => Chrome.Current != null;

        public override void Execute(Args parameter)
        {
            Chrome.Current.NavigateTo(typeof(PageT), parameter);
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Chrome : Page
    {
        private static Chrome _current;
        public static Chrome Current => _current;

        public static Chrome CreateChrome()
        {
            if (_current != null)
            {
                return _current;
            }

            return _current = new Chrome();
        }

        public App App => (App)Application.Current;

        public AppViewModel ViewModel => App.ViewModel;

        public readonly SplitViewState HamburgerCommandParameter = SplitViewState.Toggle;
        public OpenSplitViewCommand HamburgerCommand { get; private set; } = new OpenSplitViewCommand();

        public bool NavigateTo(Type destination, object arguments) => RootFrame.Navigate(destination, arguments);

        public Chrome()
        {
            this.InitializeComponent();

            this.HamburgerCommand.SplitView = MainSplitView;

            this.DataContext = this;

            this.RootFrame.NavigationFailed += this.OnNavigationFailed;

            this.RootFrame.Navigating += RootFrame_Navigating;

            this.RootFrame.Navigated += RootFrame_Navigated;

            this.ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.CurrentPodcastPosition))
            {
                if (ViewModel.CurrentPodcastPosition.HasValue)
                {
                    CurrentPodcastSlider.ValueChanged -= CurrentPodcastSlider_ValueChanged;
                    CurrentPodcastSlider.Value = (int)ViewModel.CurrentPodcastPosition.Value.TotalSeconds;
                    CurrentPodcastSlider.ValueChanged += CurrentPodcastSlider_ValueChanged;
                }
            }
        }

        private void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            var navManager = SystemNavigationManager.GetForCurrentView();

            if (!this.RootFrame.CanGoBack)
            {
                navManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }
            else
            {
                navManager.BackRequested += NavManager_BackRequested;
                navManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
        }

        private void NavManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (this.RootFrame.CanGoBack)
            {
                e.Handled = true;
                this.RootFrame.GoBack();
            }
        }

        private void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var navManager = SystemNavigationManager.GetForCurrentView();
            navManager.BackRequested -= NavManager_BackRequested;
        }

        public void AppLaunched(LaunchActivatedEventArgs e)
        {
            if (this.RootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                this.RootFrame.Navigate(typeof(Views.PodcastsListPage), e.Arguments);
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void CurrentPodcastSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            ViewModel.ScrubTo(new TimeSpan(hours: 0, minutes: 0, seconds: (int)e.NewValue));
        }
    }
}