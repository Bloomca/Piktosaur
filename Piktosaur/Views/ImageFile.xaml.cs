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

using Piktosaur.Models;
using Piktosaur.ViewModels;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Piktosaur.Views
{
    public sealed partial class ImageFile : UserControl
    {
        public bool _unloaded = false;

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(
                nameof(Image),
                typeof(ImageResult),
                typeof(ImageFile),
                new PropertyMetadata(null));

        public ImageResult Image
        {
            get => (ImageResult)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public ImageFile()
        {
            InitializeComponent();
        }
    }
}
