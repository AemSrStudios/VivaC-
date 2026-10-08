using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace VivaCSharp.Services;

public static class ImageService
{
    public static BitmapImage? OpenImage()
    {
        var Dialog = new OpenFileDialog
        {
            Title = "Open Image",
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All Files|*.*"
        };

        if (Dialog.ShowDialog() != true)
            return null;

        var Image = new BitmapImage();

        Image.BeginInit();
        Image.UriSource = new Uri(Dialog.FileName);
        Image.CacheOption = BitmapCacheOption.OnLoad;
        Image.EndInit();

        return Image;
    }
}
