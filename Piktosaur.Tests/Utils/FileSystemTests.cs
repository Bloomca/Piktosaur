using Piktosaur.Utils;

namespace Piktosaur.Tests.Utils;

public class FileSystemTests
{
    private readonly string _userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    [Fact]
    public void GetFormattedFolderName_PathInsideUserProfile_ReturnsRelativePath()
    {
        var path = Path.Combine(_userProfile, "Documents", "Photos");

        var result = FileSystem.GetFormattedFolderName(path);

        Assert.Equal(Path.Combine("Documents", "Photos"), result);
    }

    [Fact]
    public void GetFormattedFolderName_PathOutsideUserProfile_ReturnsOriginalPath()
    {
        var path = @"C:\Windows\System32";

        var result = FileSystem.GetFormattedFolderName(path);

        Assert.Equal(path, result);
    }

    [Fact]
    public void GetFormattedFolderName_UserProfileItself_ReturnsDot()
    {
        var result = FileSystem.GetFormattedFolderName(_userProfile);

        Assert.Equal(".", result);
    }

    [Fact]
    public void GetFormattedFolderName_NestedDeepPath_ReturnsRelativePath()
    {
        var path = Path.Combine(_userProfile, "Downloads", "Projects", "Images", "2024");

        var result = FileSystem.GetFormattedFolderName(path);

        Assert.Equal(Path.Combine("Downloads", "Projects", "Images", "2024"), result);
    }

    [Fact]
    public void GetPicturesFolder_ReturnsNonEmptyPath()
    {
        var result = FileSystem.GetPicturesFolder();

        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public void GetPicturesFolder_ReturnsExistingDirectory()
    {
        var result = FileSystem.GetPicturesFolder();

        Assert.True(Directory.Exists(result));
    }

    [Fact]
    public void GetDownloadsFolder_ReturnsNonEmptyPath()
    {
        var result = FileSystem.GetDownloadsFolder();

        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public void GetDownloadsFolder_ReturnsPathEndingWithDownloads()
    {
        var result = FileSystem.GetDownloadsFolder();

        Assert.EndsWith("Downloads", result);
    }

    [Fact]
    public void GetDownloadsFolder_ReturnsPathInsideUserProfile()
    {
        var result = FileSystem.GetDownloadsFolder();

        Assert.StartsWith(_userProfile, result);
    }
}
