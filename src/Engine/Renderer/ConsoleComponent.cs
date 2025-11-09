using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer;

/*
 * Egyszerű fa nézet:
 * 
 * Root - Child3 - Child4
 *   |
 * Child1
 *   |
 * Child2
 *
 * -----------------------------
 *
 *
 * Root (0, 0)
 *  |
 * Child1 (1, 1) Render->(0+1, 0+1)-> (1, 1)
 *  |
 * Child2 (1, 1) Render->(1+1, 1+1)->(2, 2)
 * +-----------------------------------------------------+
 * | A gyerek komponensek mindig relatív pozíciót várnak,|
 * | melyet az adott komponens Render() metódusa kezel.  |
 * +-----------------------------------------------------+
 *
 * ################MEGJEGYZÉS########################
 * # Későbbiekben célszerű ezt a relativisztikus    #
 * # megoldást egy külön osztályban kezelni vagy    #
 * # akár a renderelő motor által.                  #
 * ##################################################
 *
 *
 * Minden komponens rendelkezik egy Szülő és egy Gyerek
 * tulajdonsággal. Egy komponensnek több gyereke lehet
 * viszont csak egy szülője.
 * 
 */
public abstract class ConsoleComponent
{
    private int _width;
    private int _height;

    public Dimension2D Size
    {
        get
        {
            return new Dimension2D(_width, _height);
        }
        set
        {
            _width = value.Width;
            _height = value.Height;
        }
    }
    
    public Position2D Position { get; set; }
    public virtual bool Visible { get; set; } = true;
    
    public ConsoleColor BackgroundColor { get; set; }
    public ConsoleColor ForegroundColor { get; set; }
    
    public ConsoleColor BorderColor { get; set; }
    protected List<ConsoleComponent> Children { get; } = new();
    
    public virtual void AddChild(ConsoleComponent child) => Children.Add(child);
    public virtual void RemoveChild(ConsoleComponent child) => Children.Remove(child);
    
    public abstract void Update();

    public virtual void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;

        foreach (var child in Children)
        {
            child.Render(renderer);
        }
    }
    
    public virtual void Dispose()
    {
        Children.Clear();
    }
}