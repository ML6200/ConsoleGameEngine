using System;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace ConsoleGameEngine.Engine;

public class StatsPanel : UiPanel
{
    private readonly ConsoleEngine _engine;
    private readonly UiLabel _label;
    public StatsPanel(ConsoleEngine engine, string txt)
    {
        _engine = engine;
        BackgroundColor = ConsoleColor.Black;
        ForegroundColor = ConsoleColor.Green;
        HasBorder = true;
        BorderColor = ConsoleColor.DarkYellow;

        string text = GetText(out double fps);
        
        _label = new UiLabel(text)
        {
            RelativePosition = new Point2D(2, 1),
            ForegroundColor = ConsoleColor.Green,
            BackgroundColor = ConsoleColor.Black,
        };
        AddChild(_label);
        Size = _label.Size + new Dimension2D(5, 2);
        
        var label1 = new UiLabel(txt)
        {
            RelativePosition = new Point2D(Size.Width / 2 - txt.Length / 2, 0),
            ForegroundColor = ConsoleColor.DarkYellow,
        };
        AddChild(label1);
    }

    private string GetText(out double fps)
    {
        fps = Math.Round(_engine.GetAverageFrameRate());
        return $"Framerate: {fps} FPS\n" +
                      $"Update: {Math.Round(_engine.GetAverageUpdateRate())} UPS \n" +
                      $"CPU: {Math.Round(_engine.Monitoring.GetAverageCpuUsage())}%\n" +
                      $"MEM: {Math.Round(_engine.Monitoring.GetWorkingSet())} MB";
    }
    protected override void UpdateSelf()
    {
        _label.Text = GetText(out double fps);
        int newWidth = _engine.RootPanel().ScreenSize.Width - _label.Size.Width;
        
        // configure size based on num length to avoid flicker
        RelativePosition = fps / 10 < 10D ? new Point2D(newWidth - 5, 0) : new Point2D(newWidth - 4, 0);
    }
}
