namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class ConsoleGraphicsLabel : ConsoleGraphicsComponent
{
    public string Text { get; set; } = "";

    public ConsoleGraphicsLabel()
    {
    }

    public ConsoleGraphicsLabel(string text)
    {
        Text = text;
    }

    public override void Update()
    { }

    public override void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;
        
        renderer.DrawText(AbsolutePosition.X, AbsolutePosition.Y, Text,
            BackgroundColor, ForegroundColor);
    }
}