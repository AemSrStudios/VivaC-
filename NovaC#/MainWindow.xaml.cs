using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using VivaCSharp.Models;
using VivaCSharp.Services;

namespace VivaCSharp;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<MediaItem> _mediaItems = new();

    private bool _isMaximized;
    private double _zoom = 1.0;

    private readonly MediaImportService _mediaService;

    public MainWindow()
    {
        InitializeComponent();

        _mediaService = new MediaImportService();

        MediaList.ItemsSource = _mediaItems;

        PreviewKeyDown += MainWindow_PreviewKeyDown;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void ApplyResponsiveLayout()
    {
        if (ActualWidth <= 0 || ActualHeight <= 0)
            return;

        // Keep the editor comfortably sized on smaller screens.
        if (ActualHeight < 720)
        {
            // Timeline remains usable without taking over the screen.
        }

        UpdateCanvasSize();
    }

    private void UpdateCanvasSize()
    {
        double baseWidth = 900;
        double baseHeight = 506;

        EditorCanvasBorder.Width = baseWidth * _zoom;
        EditorCanvasBorder.Height = baseHeight * _zoom;

        CanvasResolutionText.Text =
            $"{(int)EditorCanvasBorder.Width} × {(int)EditorCanvasBorder.Height}";

        ZoomText.Text = $"{(int)(_zoom * 100)}%";
    }

    // ========================================================
    // WINDOW
    // ========================================================

    private void TitleBar_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
            return;
        }

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void ToggleMaximize()
    {
        if (_isMaximized)
        {
            WindowState = WindowState.Normal;
            _isMaximized = false;
        }
        else
        {
            WindowState = WindowState.Maximized;
            _isMaximized = true;
        }
    }

    private void MinimizeButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ToggleMaximize();
    }

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }

    // ========================================================
    // IMPORT MEDIA
    // ========================================================

    private async void ImportMediaButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Import Media",
            Multiselect = true,
            Filter =
                "Media Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff;*.webp;*.mp4;*.mov;*.avi;*.mkv;*.webm;*.wav;*.mp3;*.ogg|All Files|*.*"
        };

        if (dialog.ShowDialog() != true)
            return;

        await ImportFilesAsync(dialog.FileNames);
    }

    private async Task ImportFilesAsync(IEnumerable<string> files)
    {
        DropOverlay.Visibility = Visibility.Collapsed;

        foreach (string file in files)
        {
            try
            {
                MediaItem? item =
                    await _mediaService.ImportAsync(file);

                if (item != null)
                {
                    _mediaItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not import:\n\n{file}\n\n{ex.Message}",
                    "VivaC# — Import Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }

    // ========================================================
    // MEDIA DOUBLE CLICK
    // ========================================================

    private void MediaList_MouseDoubleClick(
        object sender,
        MouseButtonEventArgs e)
    {
        if (MediaList.SelectedItem is not MediaItem item)
            return;

        PlaceMediaOnCanvas(item);
    }

    private void PlaceMediaOnCanvas(MediaItem item)
    {
        if (item.Thumbnail == null)
            return;

        CanvasImage.Source = item.Thumbnail;
        CanvasImage.Visibility = Visibility.Visible;
        CanvasPlaceholder.Visibility = Visibility.Collapsed;

        LayerList.Items.Add(item.Name);

        StatusTextSafe($"Placed {item.Name} on canvas.");
    }

    // ========================================================
    // DRAG AND DROP
    // ========================================================

    private void MainWindow_DragOver(
        object sender,
        DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            DropOverlay.Visibility = Visibility.Visible;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private async void MainWindow_Drop(
        object sender,
        DragEventArgs e)
    {
        DropOverlay.Visibility = Visibility.Collapsed;

        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        if (e.Data.GetData(DataFormats.FileDrop) is not string[] files)
            return;

        await ImportFilesAsync(files);
    }

    private void Timeline_DragOver(
        object sender,
        DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private async void Timeline_Drop(
        object sender,
        DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        if (e.Data.GetData(DataFormats.FileDrop) is not string[] files)
            return;

        await ImportFilesAsync(files);
    }

    // ========================================================
    // ZOOM
    // ========================================================

    private void ZoomInButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _zoom = Math.Min(4.0, _zoom + 0.10);
        UpdateCanvasSize();
    }

    private void ZoomOutButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _zoom = Math.Max(0.10, _zoom - 0.10);
        UpdateCanvasSize();
    }

    private void FitCanvasButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        double availableWidth =
            CanvasScrollViewer.ActualWidth - 40;

        double availableHeight =
            CanvasScrollViewer.ActualHeight - 40;

        if (availableWidth <= 0 || availableHeight <= 0)
            return;

        double widthZoom = availableWidth / 900.0;
        double heightZoom = availableHeight / 506.0;

        _zoom = Math.Max(
            0.10,
            Math.Min(widthZoom, heightZoom));

        UpdateCanvasSize();
    }

    // ========================================================
    // SEARCH
    // ========================================================

    private void MediaSearchBox_TextChanged(
        object sender,
        System.Windows.Controls.TextChangedEventArgs e)
    {
        string query =
            MediaSearchBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            MediaList.ItemsSource = _mediaItems;
            return;
        }

        var filtered =
            _mediaItems
                .Where(x =>
                    x.Name.Contains(
                        query,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

        MediaList.ItemsSource = filtered;
    }

    // ========================================================
    // PLAYBACK FOUNDATION
    // ========================================================

    private bool _isPlaying;

    private void PlayButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _isPlaying = !_isPlaying;

        StatusTextSafe(
            _isPlaying
                ? "Playback started."
                : "Playback paused.");
    }

    private void StopButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _isPlaying = false;
        StatusTextSafe("Playback stopped.");
    }

    private void PreviousFrameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StatusTextSafe("Previous frame.");
    }

    // ========================================================
    // SAVE / EXPORT
    // ========================================================

    private void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        MessageBox.Show(
            "Project saving will be connected to the VivaC# document system next.",
            "VivaC#",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void ExportButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        MessageBox.Show(
            "Export pipeline is being prepared for the next editor module.",
            "VivaC#",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    // ========================================================
    // KEYBOARD
    // ========================================================

    private void MainWindow_PreviewKeyDown(
        object? sender,
        KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            PlayButton_Click(this, new RoutedEventArgs());
            e.Handled = true;
            return;
        }

        if (Keyboard.Modifiers == ModifierKeys.Control &&
            e.Key == Key.O)
        {
            ImportMediaButton_Click(
                this,
                new RoutedEventArgs());

            e.Handled = true;
        }

        if (Keyboard.Modifiers == ModifierKeys.Control &&
            e.Key == Key.S)
        {
            SaveButton_Click(
                this,
                new RoutedEventArgs());

            e.Handled = true;
        }
    }

    private void StatusTextSafe(string text)
    {
        // Reserved for the future dedicated status bar.
        Title = $"VivaC# — {text}";
    }
}
