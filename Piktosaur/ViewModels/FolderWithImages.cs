﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piktosaur.Models;

namespace Piktosaur.ViewModels
{
    /// <summary>
    /// This view model is designed to be used as a Source for CollectionViewSource.
    /// </summary>
    public class FolderWithImages : BaseViewModel, IDisposable
    {
        public string Name { get; }

        private bool isDisposed = false;

        private readonly IReadOnlyList<ImageResult> _images;

        private bool expanded;

        public bool Expanded
        {
            get => expanded;
            private set => SetProperty(ref expanded, value);
        }

        public ObservableCollection<ImageResult> Images { get; }

        public FolderWithImages(string name, IReadOnlyList<ImageResult> images, bool isExpanded = true)
        {
            Name = name;
            _images = images;
            Images = isExpanded ? new ObservableCollection<ImageResult>(images) : new ObservableCollection<ImageResult>();
            expanded = isExpanded;
        }

        public void ToggleExpanded()
        {
            Expanded = !expanded;

            if (!expanded)
            {
                // From testing, it seems that removing 1 by 1, compared to
                // Images.Clear() is more performant.
                // Remove from end to avoid index shifting
                for (int i = Images.Count - 1; i >= 0; i--)
                {
                    Images.RemoveAt(i);
                }
            }
            else
            {
                foreach (var image in _images)
                {
                    Images.Add(image);
                }
            }
        }

        public void Dispose()
        {
            if (isDisposed) return;

            isDisposed = true;

            foreach (var image in _images)
            {
                image?.Dispose();
            }
        }
    }
}
