namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public interface IComponentObserver
{
    void OnComponentAdded(GraphicsComponent component);
    void OnComponentRemoved(GraphicsComponent component);
}