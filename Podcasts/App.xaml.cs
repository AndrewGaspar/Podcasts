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

    public class PresentationSettings : DependencyObject
    {
        public class AccentColorValues
        {
            public Color AccentColor { get; set; }
            public Color AccentColorDark1 { get; set; }
            public Color AccentColorDark2 { get; set; }
            public Color AccentColorDark3 { get; set; }
            public Color AccentColorLight1 { get; set; }
            public Color AccentColorLight2 { get; set; }
            public Color AccentColorLight3 { get; set; }
        }

        private UISettings UISettings = new UISettings();

        public PresentationSettings()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                AccentColors = new AccentColorValues()
                {
                    AccentColor = GetDefaultColor("DefaultAccentColor"),
                    AccentColorDark1 = GetDefaultColor("DefaultAccentColorDark1"),
                    AccentColorDark2 = GetDefaultColor("DefaultAccentColorDark2"),
                    AccentColorDark3 = GetDefaultColor("DefaultAccentColorDark3"),
                    AccentColorLight1 = GetDefaultColor("DefaultAccentColorLight1"),
                    AccentColorLight2 = GetDefaultColor("DefaultAccentColorLight2"),
                    AccentColorLight3 = GetDefaultColor("DefaultAccentColorLight3"),
                };
            }

            try
            {
                UISettings.ColorValuesChanged += UISettings_ColorValuesChanged;

                UpdateColors();
            }
            catch(InvalidCastException)
            {
                UISettings = null;
            }
        }

        private void UISettings_ColorValuesChanged(UISettings sender, object args)
        {
            UpdateColors();
        }

        private static Color GetDefaultColor(string name)
        {
            return (Color)Application.Current.Resources[name];
        }

        public AccentColorValues AccentColors
        {
            get { return (AccentColorValues)GetValue(AccentColorsProperty); }
            set { SetValue(AccentColorsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AccentColors.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AccentColorsProperty =
            DependencyProperty.Register("AccentColors", typeof(AccentColorValues), typeof(PresentationSettings), new PropertyMetadata(0));
        
        public Color GetColor(UIColorType type) => UISettings.GetColorValue(type);

        public void UpdateColors()
        {
            AccentColors = new AccentColorValues
            {
                AccentColor = GetColor(UIColorType.Accent),
                AccentColorDark1 = GetColor(UIColorType.AccentDark1),
                AccentColorDark2 = GetColor(UIColorType.AccentDark2),
                AccentColorDark3 = GetColor(UIColorType.AccentDark3),
                AccentColorLight1 = GetColor(UIColorType.AccentLight1),
                AccentColorLight2 = GetColor(UIColorType.AccentLight2),
                AccentColorLight3 = GetColor(UIColorType.AccentLight3),
            };
        }
    }

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
