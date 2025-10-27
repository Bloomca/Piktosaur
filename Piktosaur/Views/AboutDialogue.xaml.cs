using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Piktosaur.Views
{
    public sealed partial class AboutDialogue : ContentDialog
    {
        public AboutDialogue()
        {
            InitializeComponent();
        }

        private async void EmailLink_Click(object sender, RoutedEventArgs e)
        {
            var mailto = new Uri("mailto:mail@bloomca.me");
            await Windows.System.Launcher.LaunchUriAsync(mailto);
        }
    }
}
