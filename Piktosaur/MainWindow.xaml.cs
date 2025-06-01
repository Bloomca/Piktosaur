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
using Windows.Foundation;
using Windows.Foundation.Collections;

using Piktosaur.Services;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Piktosaur
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            LoadImages();
        }

        private async void LoadImages()
        {
            var result = Search.GetImages(Search.GetPicturesFolder());
            var images = result.Results;
            List<Task> thumbnailTasks = [];
            // pre-generate first 15 image thumbnails
            foreach (var image in images[..15])
            {
                thumbnailTasks.Add(image.GenerateThumbnail());
            }

            // 1. Render Progress bar

            await Task.WhenAll(thumbnailTasks);

            // 2. Render Control for image thumbnails
        }
    }
}
