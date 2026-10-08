using System.Collections.ObjectModel;

namespace VivaCSharp.Models;

public sealed class EditorDocument
{
    public string Name { get; set; } = "Untitled";

    public int Width { get; set; } = 1920;

    public int Height { get; set; } = 1080;

    public double FrameRate { get; set; } = 24;

    public ObservableCollection<EditorLayer> Layers { get; } = new();
}
