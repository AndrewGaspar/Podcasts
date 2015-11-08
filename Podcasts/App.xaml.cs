using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Security.Authentication.OnlineId;
using Windows.UI.Xaml;

namespace Podcasts
{
    using Transport;
    using ViewModels;
    using Windows.ApplicationModel.UserDataAccounts;
    using Windows.Security.Credentials;
    using Windows.UI.Popups;

    class Subscription
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string LocationHref { get; set; }
    }

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        private AppViewModel AppViewModel = AppViewModel.Current;

        public AppViewModel ViewModel => AppViewModel;
        
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            await AppViewModel.InitializeAsync();

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (Chrome.Current == null)
            {
                // Create the page chrome to act as the navigation context and navigate to the first page
                var chrome = Chrome.CreateChrome();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = chrome;
            }

            Chrome.Current.AppLaunched(args);

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // suspend here

            deferral.Complete();
        }
    }
}