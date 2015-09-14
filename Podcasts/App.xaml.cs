using System;
using System.Diagnostics;
using Podcasts.Transport;
using Podcasts.ViewModels;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Podcasts
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        public ForeroundMessageTransport MessageService = new ForeroundMessageTransport();

        private AppViewModel AppViewModel = new AppViewModel();

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
            Chrome mainPage = Window.Current.Content as Chrome;
            
            await AppViewModel.InitializeAsync();
            
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (mainPage == null)
            {
                // Create the page chrome to act as the navigation context and navigate to the first page
                mainPage = new Chrome();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = mainPage;
            }

            mainPage.AppLaunched(args);

            MessageService.Start();
            
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
            MessageService.Stop();
            deferral.Complete();
        }
    }
}
