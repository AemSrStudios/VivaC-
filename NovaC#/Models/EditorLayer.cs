namespace VivaCSharp.Models;

public sealed class EditorLayer
{
    public string Name { get; set; } = "Layer";

    public bool IsVisible { get; set; } = true;

    public double X { get; set; }

    public double Y { get; set; }

    public double Scale { get; set; } = 1.0;

    public double Rotation { get; set; }

    public double Opacity { get; set; } = 1.0;
}
