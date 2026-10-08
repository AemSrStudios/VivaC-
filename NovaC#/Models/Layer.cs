namespace VivaCSharp.Models;

public class Layer
{
    public string Name { get; set; } = "Layer";

    public bool IsVisible { get; set; } = true;

    public bool IsLocked { get; set; } = false;

    public double Opacity { get; set; } = 1.0;
}
