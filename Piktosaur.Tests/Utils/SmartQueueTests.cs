using Microsoft.UI.Xaml.Media;
using Piktosaur.Services;
using Piktosaur.Utils;

namespace Piktosaur.Tests.Utils;

public class SmartQueueTests : IDisposable
{
    private readonly FakeThumbnailGenerator _fakeThumbnailGenerator;
    private readonly SmartQueue _queue;

    public SmartQueueTests()
    {
        _fakeThumbnailGenerator = new FakeThumbnailGenerator();
        _queue = new SmartQueue(_fakeThumbnailGenerator);
    }

    public void Dispose()
    {
        _queue.Dispose();
    }

    [Fact]
    public async Task AddRequest_WhenQueueExceedsMax_DropsOldestRequest()
    {
        // Arrange - block execution so requests pile up
        _fakeThumbnailGenerator.BlockExecution = true;
        var tasks = new List<Task<ImageSource?>>();

        // Act - add 25 requests (max is 20, and we process them in batches of 4)
        for (int i = 0; i < 25; i++)
        {
            tasks.Add(_queue.AddRequest($"path{i}", CancellationToken.None));
        }

        // Allow execution to proceed
        _fakeThumbnailGenerator.BlockExecution = false;
        _fakeThumbnailGenerator.ReleaseAll();

        // Wait for all tasks
        await Task.WhenAll(tasks);

        // Assert - first request (oldest) should have been dropped and completed with null
        var firstResult = await tasks[0];
        Assert.Null(firstResult);
    }

    [Fact]
    public async Task AddRequest_InsertsAtFrontOfQueue()
    {
        // Arrange - block execution and track order of processing
        _fakeThumbnailGenerator.BlockExecution = true;
        var processedPaths = new List<string>();
        _fakeThumbnailGenerator.OnProcess = path => processedPaths.Add(path);

        // Act - add requests
        _queue.AddRequest("first", CancellationToken.None);
        _queue.AddRequest("second", CancellationToken.None);
        _queue.AddRequest("third", CancellationToken.None);
        _queue.AddRequest("fourth", CancellationToken.None);

        // Release and let them process
        _fakeThumbnailGenerator.BlockExecution = false;
        _fakeThumbnailGenerator.ReleaseAll();

        // Wait a bit for processing
        await Task.Delay(100);

        // first item is processed immediately;  and then the order is reversed
        Assert.Equal("first", processedPaths[0]);
        Assert.Equal("fourth", processedPaths[1]);
        Assert.Equal("third", processedPaths[2]);
        Assert.Equal("second", processedPaths[3]);
    }

    [Fact]
    public async Task ExecuteRequests_CancelledToken_CompletesWithNull()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-cancel the token

        // Act
        var task = _queue.AddRequest("path", cts.Token);
        await Task.Delay(50); // Allow execution to process

        // Assert
        var result = await task;
        Assert.Null(result);

        _fakeThumbnailGenerator.ReleaseAll();
    }

    [Fact]
    public async Task ExecuteRequests_WhenDisposed_SkipsExecution()
    {
        // Arrange
        _fakeThumbnailGenerator.BlockExecution = true;
        var task = _queue.AddRequest("path", CancellationToken.None);

        // Act - dispose before execution can complete
        _queue.Dispose();

        // Assert - task should complete with null
        var result = await task;
        Assert.Null(result);
        Assert.Equal(0, _fakeThumbnailGenerator.ProcessCount);
    }

    private class FakeThumbnailGenerator : IThumbnailGenerator
    {
        public bool BlockExecution { get; set; } = false;
        public int ProcessCount { get; private set; } = 0;
        public Action<string>? OnProcess { get; set; }

        private TaskCompletionSource _blocker = new();

        public async Task<ImageSource> CreateManualThumbnail(string path, CancellationToken cancellationToken)
        {
            if (BlockExecution)
            {
                await _blocker.Task;
            }

            cancellationToken.ThrowIfCancellationRequested();

            ProcessCount++;
            OnProcess?.Invoke(path);

            return null!;
        }

        public void ReleaseAll()
        {
            _blocker.TrySetResult();
            _blocker = new TaskCompletionSource();
        }
    }
}
