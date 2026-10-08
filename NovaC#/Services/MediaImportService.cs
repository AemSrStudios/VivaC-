using System.IO;
using System.Windows.Media.Imaging;
using VivaCSharp.Models;

namespace VivaCSharp.Services;

public sealed class MediaImportService
{
    private static readonly HashSet<string> ImageExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".bmp",
            ".gif",
            ".tif",
            ".tiff",
            ".webp"
        };

    private static readonly HashSet<string> VideoExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4",
            ".mov",
            ".avi",
            ".mkv",
            ".webm"
        };

    private static readonly HashSet<string> AudioExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".wav",
            ".mp3",
            ".ogg",
            ".flac",
            ".m4a"
        };

    public async Task<MediaItem?> ImportAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        if (!File.Exists(path))
            return null;

        string extension =
            Path.GetExtension(path);

        MediaType type =
            GetMediaType(extension);

        var item = new MediaItem
        {
            Name = Path.GetFileName(path),
            FullPath = path,
            Type = type,
            FileSize = new FileInfo(path).Length
        };

        // Images get an asynchronous lightweight thumbnail.
        if (type == MediaType.Image)
        {
            item.Thumbnail =
                await CreateThumbnailAsync(
                    path,
                    cancellationToken);
        }

        return item;
    }

    private static MediaType GetMediaType(
        string extension)
    {
        if (ImageExtensions.Contains(extension))
            return MediaType.Image;

        if (VideoExtensions.Contains(extension))
            return MediaType.Video;

        if (AudioExtensions.Contains(extension))
            return MediaType.Audio;

        return MediaType.Unknown;
    }

    private static Task<BitmapImage?> CreateThumbnailAsync(
        string path,
        CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using FileStream stream =
                    new(
                        path,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read);

                var bitmap = new BitmapImage();

                bitmap.BeginInit();

                // Decode only a small preview.
                bitmap.DecodePixelWidth = 256;

                bitmap.CacheOption =
                    BitmapCacheOption.OnLoad;

                bitmap.CreateOptions =
                    BitmapCreateOptions.IgnoreColorProfile;

                bitmap.StreamSource = stream;

                bitmap.EndInit();

                bitmap.Freeze();

                return bitmap;
            }
            catch
            {
                return null;
            }
        }, cancellationToken);
    }
}
