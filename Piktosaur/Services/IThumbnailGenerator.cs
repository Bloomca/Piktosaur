using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;

namespace Piktosaur.Services
{
    public interface IThumbnailGenerator
    {
        Task<ImageSource?> GenerateThumbnail(string path, CancellationToken cancellationToken);
        Task<ImageSource> CreateManualThumbnail(string path, CancellationToken cancellationToken);
    }
}
