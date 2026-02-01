using System;
using System.Threading;
using System.Threading.Tasks;
using Piktosaur.Services;

namespace Piktosaur.ViewModels
{
    public class ImagesListVM : BaseViewModel, IDisposable
    {
        private AppStateVM appStateVM;
        private ImageQueryService imageQueryService;
        private CancellationTokenSource? cancellationTokenSource;
        private bool isDisposed = false;

        private bool loading = false;

        public bool Loading
        {
            get => loading;
            private set => SetProperty(ref loading, value);
        }

        public ImagesListVM(AppStateVM appStateVM)
        {
            this.appStateVM = appStateVM;
            this.imageQueryService = ImageQueryService.Shared;
        }

        public Task LoadImages()
        {
            Loading = true;

            cancellationTokenSource = new CancellationTokenSource();

            var currentQuery = appStateVM.SelectedQuery;
            var task = imageQueryService.ExecuteQuery(currentQuery);

            _ = HandleThumbnails();

            return task;
        }

        private async Task HandleThumbnails()
        {
            // give it 100ms to load the first folder. This is not guaranteed by any means
            // but should work most of the time
            await Task.Delay(100);

            var token = cancellationTokenSource?.Token ?? CancellationToken.None;
            await imageQueryService.GenerateInitialThumbnails(token);

            Loading = false;
        }

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            cancellationTokenSource?.Cancel();
            // Note: We don't dispose imageQueryService here as it's a shared singleton
        }
    }
}
