namespace ConsoleGameEngine.Engine;

public interface IGameScene
{
    void Initialize(ConsoleEngine consoleEngine);
    void OnEnter();
    void OnUpdate(double deltaTime);
    void OnExit();
}