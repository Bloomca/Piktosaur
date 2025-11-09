using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Piktosaur.ViewModels;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Piktosaur
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static Window MainWindow {  get; private set; }

        public static IntPtr MainWindowHandle => WindowNative.GetWindowHandle(MainWindow);

        private Window? _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            EvaluateStartingPath();

            _window = new MainWindow();
            MainWindow = _window;

            _window.ExtendsContentIntoTitleBar = true;

            // Cast to MainWindow to access the TitleBar property
            if (_window is MainWindow mainWindow)
            {
                _window.SetTitleBar(mainWindow.TitleBar);
            }

            _window.Activate();
        }

        private void EvaluateStartingPath()
        {
            try
            {
                string[] commandLineArgs = Environment.GetCommandLineArgs();
                if (commandLineArgs.Length <= 1) { return; }
                var folderPath = commandLineArgs[1];
                if (String.IsNullOrEmpty(folderPath)) { return; }
                if (!Directory.Exists(folderPath)) { return; }

                AppStateVM.Shared.AddPathQuery(folderPath);
            } catch
            {
                // ignore
            }
        }
    }
}
