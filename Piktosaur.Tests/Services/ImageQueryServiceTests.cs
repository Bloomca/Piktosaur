using Microsoft.UI.Xaml.Media;
using Piktosaur.Models;
using Piktosaur.Services;

namespace Piktosaur.Tests.Services;

public class ImageQueryServiceTests : IDisposable
{
    private readonly string _testRootPath;
    private readonly FakeThumbnailGenerator _fakeThumbnailGenerator;

    public ImageQueryServiceTests()
    {
        _fakeThumbnailGenerator = new FakeThumbnailGenerator();
        _testRootPath = Path.Combine(Path.GetTempPath(), $"PiktosaurTest_{Guid.NewGuid()}");
        SetupTestFolderStructure();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testRootPath))
        {
            Directory.Delete(_testRootPath, recursive: true);
        }
    }

    /// <summary>
    /// Creates a nested folder structure:
    /// root/
    ///   image1.jpg
    ///   image2.png
    ///   subfolder1/
    ///     image3.jpg
    ///     deep/
    ///       image4.jpeg
    ///       deeper/
    ///         image5.png
    ///   subfolder2/
    ///     image6.jpg
    ///   empty_folder/
    ///   text_only/
    ///     readme.txt
    /// </summary>
    private void SetupTestFolderStructure()
    {
        // Create directories
        Directory.CreateDirectory(_testRootPath);
        Directory.CreateDirectory(Path.Combine(_testRootPath, "subfolder1"));
        Directory.CreateDirectory(Path.Combine(_testRootPath, "subfolder1", "deep"));
        Directory.CreateDirectory(Path.Combine(_testRootPath, "subfolder1", "deep", "deeper"));
        Directory.CreateDirectory(Path.Combine(_testRootPath, "subfolder2"));
        Directory.CreateDirectory(Path.Combine(_testRootPath, "empty_folder"));
        Directory.CreateDirectory(Path.Combine(_testRootPath, "text_only"));

        // Create image files (just empty files with correct extensions)
        File.WriteAllBytes(Path.Combine(_testRootPath, "image1.jpg"), [0xFF, 0xD8, 0xFF]); // JPEG magic bytes
        File.WriteAllBytes(Path.Combine(_testRootPath, "image2.png"), [0x89, 0x50, 0x4E, 0x47]); // PNG magic bytes
        File.WriteAllBytes(Path.Combine(_testRootPath, "subfolder1", "image3.jpg"), [0xFF, 0xD8, 0xFF]);
        File.WriteAllBytes(Path.Combine(_testRootPath, "subfolder1", "deep", "image4.jpeg"), [0xFF, 0xD8, 0xFF]);
        File.WriteAllBytes(Path.Combine(_testRootPath, "subfolder1", "deep", "deeper", "image5.png"), [0x89, 0x50, 0x4E, 0x47]);
        File.WriteAllBytes(Path.Combine(_testRootPath, "subfolder2", "image6.jpg"), [0xFF, 0xD8, 0xFF]);

        // Create non-image file
        File.WriteAllText(Path.Combine(_testRootPath, "text_only", "readme.txt"), "not an image");
    }

    [Fact]
    public async Task ExecuteQuery_ReadsNestedFoldersRecursively()
    {
        // Arrange
        var service = new ImageQueryService(_fakeThumbnailGenerator);
        var query = new Query("Test", _testRootPath);

        // Act
        await service.ExecuteQuery(query);

        // Assert - should have 4 folders with images (root, subfolder1, deep, deeper, subfolder2)
        // Note: empty_folder and text_only should not be included
        Assert.Equal(5, service.Folders.Count);

        // Verify total image count
        var totalImages = service.Folders.Sum(f => f.Images.Count);
        Assert.Equal(6, totalImages);

        // Verify folder names are present
        var folderNames = service.Folders.Select(f => f.Name).ToList();
        Assert.Contains(Path.GetFileName(_testRootPath), folderNames);
        Assert.Contains("subfolder1", folderNames);
        Assert.Contains("deep", folderNames);
        Assert.Contains("deeper", folderNames);
        Assert.Contains("subfolder2", folderNames);

        // Verify empty_folder and text_only are NOT included
        Assert.DoesNotContain("empty_folder", folderNames);
        Assert.DoesNotContain("text_only", folderNames);

        service.Dispose();
    }

    [Fact]
    public async Task GetImagePathsSnapshot_ReturnsAllImagePaths()
    {
        // Arrange
        var service = new ImageQueryService(_fakeThumbnailGenerator);
        var query = new Query("Test", _testRootPath);
        await service.ExecuteQuery(query);

        // Act
        var snapshot = service.GetImagePathsSnapshot();

        // Assert
        Assert.Equal(6, snapshot.Length);

        // Verify all expected paths are present
        Assert.Contains(snapshot, p => p.EndsWith("image1.jpg"));
        Assert.Contains(snapshot, p => p.EndsWith("image2.png"));
        Assert.Contains(snapshot, p => p.EndsWith("image3.jpg"));
        Assert.Contains(snapshot, p => p.EndsWith("image4.jpeg"));
        Assert.Contains(snapshot, p => p.EndsWith("image5.png"));
        Assert.Contains(snapshot, p => p.EndsWith("image6.jpg"));

        service.Dispose();
    }

    [Fact]
    public async Task GetImagePathsSnapshot_ReturnsIndependentCopy()
    {
        // Arrange
        var service = new ImageQueryService(_fakeThumbnailGenerator);
        var query = new Query("Test", _testRootPath);
        await service.ExecuteQuery(query);

        // Act - get snapshot, then clear the service
        var snapshot = service.GetImagePathsSnapshot();

        // Clear by executing query on empty folder
        var emptyPath = Path.Combine(_testRootPath, "empty_folder");
        await service.ExecuteQuery(new Query("Empty", emptyPath));

        // Assert - snapshot should still have original data
        Assert.Equal(6, snapshot.Length);
        Assert.Empty(service.Folders);

        service.Dispose();
    }

    private class FakeThumbnailGenerator : IThumbnailGenerator
    {
        public Task<ImageSource?> GenerateThumbnail(string path, CancellationToken cancellationToken)
        {
            return Task.FromResult<ImageSource?>(null);
        }

        public Task<ImageSource> CreateManualThumbnail(string path, CancellationToken cancellationToken)
        {
            return Task.FromResult<ImageSource>(null!);
        }
    }
}
