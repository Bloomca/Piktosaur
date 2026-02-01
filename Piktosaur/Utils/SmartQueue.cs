using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Piktosaur.Services;

namespace Piktosaur.Utils
{
    /// <summary>
    /// This is a custom implementation of priority queue. It is not a generic
    /// implementation, it directly calls `ThumbnailGenerator` from DI.
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
        private readonly int MAX_REQUESTS = 20;
        private List<QueueItem> requests = new();
        private List<QueueItem> activeRequests = new();

        private IThumbnailGenerator thumbnailGenerator;

        private bool isExecuting = false;

        private bool isDisposed = false;

        public SmartQueue(IThumbnailGenerator thumbnailGenerator)
        {
            this.thumbnailGenerator = thumbnailGenerator;
        }

        public Task<ImageSource?> AddRequest(string path, CancellationToken ct)
        {
            TaskCompletionSource<ImageSource?> tcs = new();
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
            while (GetNextRequests() is var newRequests && newRequests.Length > 0)
            {
                // Remove all items first, so they are not processed twice
                foreach (var request in newRequests)
                {
                    requests.Remove(request);
                    activeRequests.Add(request);
                }

                var tasks = newRequests.Select(async newRequest =>
                {
                    try
                    {
                        newRequest.ct.ThrowIfCancellationRequested();
                        var result = await thumbnailGenerator.CreateManualThumbnail(newRequest.Path, newRequest.ct);
                        newRequest.Tcs.SetResult(result);
                    }
                    catch
                    {
                        newRequest.Tcs.SetResult(null);
                    }
                    finally
                    {
                        activeRequests.Remove(newRequest);
                    }
                });

                await Task.WhenAll(tasks);
            }

            isExecuting = false;
        }

        private QueueItem[] GetNextRequests()
        {
            return requests.TakeLast(4).ToArray();
        }

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
            foreach (var request in requests)
            {
                request.Tcs.SetResult(null);
            }

            foreach (var request in activeRequests)
            {
                request.Tcs.SetResult(null);
            }

            requests.Clear();
            activeRequests.Clear();
        }
    }

    // probably would be better as a struct
    public class QueueItem
    {
        public TaskCompletionSource<ImageSource?> Tcs;
        public string Path;
        public CancellationToken ct;

        public QueueItem(TaskCompletionSource<ImageSource?> tcs, string path, CancellationToken ct)
        {
            Tcs = tcs;
            Path = path;
            this.ct = ct;
        }
    }
}
