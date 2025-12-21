using ConsoleGameEngine.Engine.Input;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class UiManager : IComponentObserver
{
    private readonly KeyboardFocusManager _keyboardFocusManager;
    
    private readonly KeyBinding _tabKey = KeyBinding.Commons.Tab;
    private readonly KeyBinding _shiftTabKey = KeyBinding.Parse("shift+tab");

    
    public UiManager(InputManager inputManager)
    {
        _keyboardFocusManager = new KeyboardFocusManager(inputManager);
        
        inputManager.Register(_tabKey);
        inputManager.Register(_shiftTabKey);
        inputManager.Register(KeyBinding.Commons.Enter);

        inputManager.Subscribe(_tabKey, _keyboardFocusManager.FocusNext);
        inputManager.Subscribe(_shiftTabKey, _keyboardFocusManager.FocusPrevious);
        inputManager.Subscribe(KeyBinding.Commons.Enter, () =>
        {
            if (_keyboardFocusManager.FocusedComponent is UiButton)
            {
                _keyboardFocusManager.FocusedComponent.OnFocusActivate();
            }
        });
        
        inputManager.SubscribeToRawInput(HandleInput);
    }

    private void HandleInput(KeyEventArgs e)
    {
        if (_keyboardFocusManager.FocusedComponent is IUiInput inputComponent)
        {
            inputComponent.HandleInput(e);
        }
    }

    public void OnComponentAdded(GraphicsComponent component)
    {
        if (component is IFocusable focusable)
            _keyboardFocusManager.Register(focusable);
    }

    public void OnComponentRemoved(GraphicsComponent component)
    {
        if (component is IFocusable focusable)
            _keyboardFocusManager.Unregister(focusable);
    }
}