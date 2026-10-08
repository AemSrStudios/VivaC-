using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace VivaCSharp.Models;

public enum MediaType
{
    Image,
    Video,
    Audio,
    Unknown
}

public sealed class MediaItem : INotifyPropertyChanged
{
    private BitmapImage? _thumbnail;

    public string Name { get; init; } = string.Empty;

    public string FullPath { get; init; } = string.Empty;

    public MediaType Type { get; init; }

    public long FileSize { get; init; }

    public BitmapImage? Thumbnail
    {
        get => _thumbnail;

        set
        {
            if (_thumbnail == value)
                return;

            _thumbnail = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
