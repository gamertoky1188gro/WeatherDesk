using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using WinRT.Interop; // Correct namespace for Win32Interop and GetWindowHandle
using Windows.Storage;
using Windows.Graphics;
using Microsoft.UI;

namespace WeatherDesk
{
    public partial class App : Application
    {
        private Window? m_window;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            if (m_window == null)
            {
                m_window = new MainWindow();

                // Initialize the Frame for navigation
                Frame rootFrame = new Frame();
                m_window.Content = rootFrame;

                // Access LocalSettings
                var localSettings = ApplicationData.Current.LocalSettings;

                // Check if the "FirstLaunch" setting exists
                if (localSettings.Values["FirstLaunch"] == null)
                {
                    // Mark as launched
                    localSettings.Values["FirstLaunch"] = true;

                    // Navigate to the Welcome Page
                    rootFrame.Navigate(typeof(BlankPage1));
                }
                else
                {
                    // Navigate to the Main Page
                    rootFrame.Navigate(typeof(BlankPage2));
                }

                // Make the window full screen
                var windowHandle = WindowNative.GetWindowHandle(m_window);
                var appWindow = AppWindow.GetFromWindowId(
                    Win32Interop.GetWindowIdFromWindow(windowHandle));

                if (appWindow != null)
                {
                    // Simulate full screen by resizing the window to cover the entire display area
                    var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
                    appWindow.MoveAndResize(new RectInt32(0, 0, displayArea.WorkArea.Width, displayArea.WorkArea.Height));
                }

                m_window.Activate();
            }
        }
    }
}
