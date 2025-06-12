using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Piktosaur.Services;

namespace Piktosaur.Utils
{
    /// <summary>
    /// This is a custom implementation of priority queue. It is not a generic
    /// implementation, it directly calls `ThumbnailGeneration` from DI.
    /// 
    /// The idea is to schedule thumbnail creation through this queue, as it
    /// has backpressure built-in, so rapid scrolling will drop pretty much
    /// all requests, not straining the UI thread and general resources.
    /// 
    /// Currently it allows only 1 concurrent thumbnail action, through the
    /// testing I determined it has the best UI performance while processing
    /// requests very fast.
    /// </summary>
    public class SmartQueue : IDisposable
    {
        private readonly int MAX_REQUESTS = 15;
        private List<QueueItem> requests = new();

        private ThumbnailGeneration thumbnailGeneration;

        private bool isExecuting = false;

        private bool isDisposed = false;

        public SmartQueue(ThumbnailGeneration thumbnailGeneration)
        {
            this.thumbnailGeneration = thumbnailGeneration;
        }

        public Task<BitmapSource?> AddRequest(string path, CancellationToken ct)
        {
            TaskCompletionSource<BitmapSource?> tcs = new();
            if (requests.Count > MAX_REQUESTS)
            {
                var oldestRequest = requests.Last();
                oldestRequest.Tcs.SetResult(null);
                requests.Remove(oldestRequest);
            }

            requests.Insert(0, new QueueItem(tcs, path, ct));
            ExecuteRequests();

            return tcs.Task;
        }

        public async void ExecuteRequests()
        {
            if (isExecuting || isDisposed) return;

            isExecuting = true;

            // ideally, it would be a recursive call, but tail-call optimization
            // seems to be spotty in JIT
            while (GetNextRequest() is var newRequest && newRequest != null)
            {
                // remove the request first from the data structure,
                // otherwise it can be processed twice
                requests.Remove(newRequest);

                try
                {
                    newRequest.ct.ThrowIfCancellationRequested();
                    var result = await thumbnailGeneration.CreateManualThumbnail(newRequest.Path, newRequest.ct);
                    newRequest.Tcs.SetResult(result);
                }
                catch
                {
                    newRequest.Tcs.SetResult(null);
                }
            }

            isExecuting = false;
        }

        private QueueItem? GetNextRequest()
        {
            if (requests.Count == 0) return null;
            return requests.Last();
        }

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
            foreach (var request in requests)
            {
                request.Tcs.SetResult(null);
            }

            requests.Clear();
        }
    }

    // probably would be better as a struct
    public class QueueItem
    {
        public TaskCompletionSource<BitmapSource?> Tcs;
        public string Path;
        public CancellationToken ct;

        public QueueItem(TaskCompletionSource<BitmapSource?> tcs, string path, CancellationToken ct)
        {
            Tcs = tcs;
            Path = path;
            this.ct = ct;
        }
    }
}
