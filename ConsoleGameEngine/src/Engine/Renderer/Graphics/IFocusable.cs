namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public interface IFocusable
{
    bool IsFocused { get; set; }
    bool CanFocus { get; set; }
    void OnFocusGained();
    void OnFocusLost();
    void OnFocusActivate();
}