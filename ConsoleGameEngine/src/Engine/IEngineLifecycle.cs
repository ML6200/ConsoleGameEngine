namespace ConsoleGameEngine.Engine;

public interface IEngineLifecycle
{
    void Initialize();
    
    void OnStart();
    void OnUpdate();
}