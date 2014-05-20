using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Recommender.Common;
using Windows.Devices.Enumeration.Pnp;
using Windows.Storage;

#if WINDOWS_APP
using Windows.UI.ApplicationSettings;
#endif

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace Recommender
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif
        public enum NavaigationPages : int { MainPage, SearchPage, TopicSummaryPage, TopicDetailPage, ItemPage, SettingFlyoutAboutPage, SettingFlyoutPrivacyPolicyPage };
        public static class NavigationRoadmap
        {
            private static string _from = "";
            private static string _to = "";

            public static string GetFrom()
            {
                return _from;
            }

            public static string GetTo()
            {
                return _to;
            }

            public static void SetFrom(int from)
            {
                _from = GetNavigationPage(from);
            }

            public static void SetTo(int to)
            {
                _to = GetNavigationPage(to);
            }

            private static string GetNavigationPage(int page)
            {
                string pageName = "MainPage";
                switch (page)
                {
                    case 0:
                        pageName = "MainPage";
                        break;
                    case 1:
                        pageName = "SearchPage";
                        break;
                    case 2:
                        pageName = "TopicSummaryPage";
                        break;
                    case 3:
                        pageName = "TopicDetailPage";
                        break;
                    case 4:
                        pageName = "ItemPage";
                        break;
                    case 5:
                        pageName = "SettingFlyoutAboutPage";
                        break;
                    case 6:
                        pageName = "SettingFlyoutPrivacyPolicyPage";
                        break;
                    default:
                        pageName = "MainPage";
                        break;
                }
                return pageName;
            }
        }

        private ApplicationDataContainer roamingSettings = null;
        private const string DeviceNameSetting = "DeviceName";
        public static string gDeviceName = "";
        public static string gDeviceType = "";

        /// <summary>
        /// Initializes the singleton instance of the <see cref="App"/> class. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
            if (gDeviceType.Equals(string.Empty))
            {
#if WINDOWS_APP
                gDeviceType = "WINDOWS";
#elif WINDOWS_PHONE_APP
                gDeviceType = "WINDOWS_PHONE";
#endif
            }

            if (gDeviceName.Equals(string.Empty))
            {
                GetDeviceName();
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        // Something went wrong restoring state.
                        // Assume there is no state and continue
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
#if WINDOWS_APP
                // Register handler for CommandsRequested events from the settings pane
                SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
#endif
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(HubPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        private async void GetDeviceName()
        {
            // read device name
            roamingSettings = ApplicationData.Current.RoamingSettings;
            Object value = roamingSettings.Values[DeviceNameSetting];
            string deviceName = "";

            if (value != null)
            {
                deviceName = value.ToString();
                if (String.IsNullOrEmpty(deviceName) || String.IsNullOrWhiteSpace(deviceName))
                {
                    string[] properties = { "System.ItemNameDisplay" };
                    var deviceobj = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer, "00000000-0000-0000-FFFF-FFFFFFFFFFFF", properties);

                    foreach (KeyValuePair<string, object> kvp in deviceobj.Properties)
                    {
                        if (kvp.Key.Equals("System.ItemNameDisplay"))
                        {
                            deviceName = kvp.Value.ToString();
                        }
                    }
#if WINDOWS_APP
                    gDeviceName = deviceName;
#elif WINDOWS_PHONE_APP
                    if (deviceName.Equals("Windows Phone"))
                    {
                        Random r = new Random();
                        int randomId = r.Next();
                        deviceName = "WindowsPhone_" + randomId.ToString();
                    }
                    gDeviceName = deviceName;
#endif
                    roamingSettings.Values[DeviceNameSetting] = gDeviceName;
                }
                else
                {
                    gDeviceName = deviceName;
                    roamingSettings.Values[DeviceNameSetting] = gDeviceName;
                }
            }
            else
            {
                string[] properties = { "System.ItemNameDisplay" };
                var deviceobj = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer, "00000000-0000-0000-FFFF-FFFFFFFFFFFF", properties);

                foreach (KeyValuePair<string, object> kvp in deviceobj.Properties)
                {
                    if (kvp.Key.Equals("System.ItemNameDisplay"))
                    {
                        deviceName = kvp.Value.ToString();
                    }
                }
#if WINDOWS_APP
                gDeviceName = deviceName;
#elif WINDOWS_PHONE_APP
                    if (deviceName.Equals("Windows Phone"))
                    {
                        Random r = new Random();
                        int randomId = r.Next();
                        deviceName = "WindowsPhone_" + randomId.ToString();
                    }
                    gDeviceName = deviceName;
#endif
                roamingSettings.Values[DeviceNameSetting] = gDeviceName;
            }
        }

#if WINDOWS_APP
        private void OnCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs e)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string aboutSettingCommandTitle = loader.GetString("About");
            string privacyPolicySettingCommandTitle = loader.GetString("PrivacyPolicy");

            SettingsCommand defaultsCommand = new SettingsCommand("about", aboutSettingCommandTitle,
                (handler) =>
                {
                    SettingsFlyoutAbout sfa = new SettingsFlyoutAbout();
                    sfa.Show();
                });
            e.Request.ApplicationCommands.Add(defaultsCommand);

            SettingsCommand privacyPolicyCommand = new SettingsCommand("privacypolicy", privacyPolicySettingCommandTitle,
                (handler) =>
                {
                    SettingsFlyoutPrivacyPolicy sfpp = new SettingsFlyoutPrivacyPolicy();
                    sfpp.Show();
                });
            e.Request.ApplicationCommands.Add(privacyPolicyCommand);
        }
#endif
    }
}