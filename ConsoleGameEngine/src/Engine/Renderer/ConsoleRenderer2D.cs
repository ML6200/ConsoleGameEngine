using System;
using System.Text;
using System.Threading;
using System.Xml;
using ConsoleGameEngine.Engine.Renderer.Geometry;

/*
 *
 * Console:
 * +----------------------------+
 * | P                          | buffer[x, 0]
 * |      +----+                | buffer[x, 1]
 * |      |TEXT|                |      .
 * |      +----+                |      .
 * |                            |      .
 * |                            | buffer[x, 5]
 * +----------------------------+
 *  bb                         b
 *  uu                         u
 *  ff                         f
 *  ff                         f
 *  ee                         e
 *  rr                         r
 *  __                         _
 *  12.........................28
 *  ----------------------------
 *  yy                         y
 *
 *
 *
 * Box:
 * (x, y)      (x+width-1, y)
 *        +----+
 *        |TEXT|
 *        +----+    (x+width-1, y + height-1)
 * (x, y+height-1)
 *
 *
 *
 */
namespace ConsoleGameEngine.Engine.Renderer;

public class ConsoleRenderer2D
{
    private readonly object _bufferLock = new object();
    private int _screenWidth;
    private int _screenHeight;
    
    private Cell[,] _renderBuffer;
    private bool[,] _dirtyMarks;
    
    private volatile bool _isResizing;

    public int ScreenWidth
    {
        get => _screenWidth;
    }

    public int ScreenHeight
    {
        get => _screenHeight;
    }

    public void SetDimension(int width, int height)
    {
        _isResizing = true;
        _screenWidth = width;
        _screenHeight = height;
        _renderBuffer = new Cell[_screenWidth, _screenHeight];
        _dirtyMarks = new bool[_screenWidth, _screenHeight];
        
        Thread.MemoryBarrier(); // priorizaljuk a meretezest
        _isResizing = false;
    }

    public ConsoleRenderer2D(int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        InitRenderer();
    }

    public ConsoleRenderer2D(Dimension2D dimension)
    {
        _screenWidth = dimension.Width;
        _screenHeight = dimension.Height;
        InitRenderer();
    }

    public void InitRenderer()
    {
        Console.CursorVisible = false;
        Console.Clear();
        
        _renderBuffer = new Cell[_screenWidth, _screenHeight];
        _dirtyMarks = new bool[_screenWidth, _screenHeight];

        FlushBuffer();
    }
    
    
    private bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < _screenWidth && y >= 0 && y < _screenHeight;
    }

    public void FlushBuffer()
    {
        if (_isResizing) return;
        
        
        for (int i = 0; i < _screenHeight; i++)
        {
            for (int j = 0; j < _screenWidth; j++)
            {
                _renderBuffer[j, i] = Cell.Empty;
                _dirtyMarks[j, i] = true; 
            }
        }
    }

    public void SetCell(int x, int y, Cell cell)
    {
        if (_isResizing)  return;
        
        if (IsValidCoordinate(x, y) 
            && !_renderBuffer[x, y].Equals(cell))
        {
            _renderBuffer[x, y] = cell;
            _dirtyMarks[x, y] = true;
        }
    }

    public void DrawText(int x, int y, string text,
        ConsoleColor bgColor = ConsoleColor.Black,
        ConsoleColor fgColor = ConsoleColor.White)
    {
        if (_isResizing) return;
        
        for (int i = 0; i < text.Length; i++)
        {
            SetCell(x + i, y, new Cell(text[i], bgColor, fgColor));
        }
    }
    
    /*           #
     *          # #
     *         #   #
     *        ######
     *
     *
     *         *   
     *        ***
     *       *****
     *      *******
     *
     *
     *        WIDTH    H
     *      +-------+  E
     *      |   *   |  I
     *      |  ***  |  G
     *      | ***** |  H
     *      +-------+  T
     *
     * WIDTH=5
     * HEIGHT=3
     * 
     *
     */
    public void DrawTriangle(int x, int y,
        int height,
        Cell? cell = default)
    {
        if (_isResizing) return;
        
        for (int dy = 1; dy <= height; dy++)
        {
            for (int i = 1; i <= dy; i++)
            {
                // _renderBuffer[x, y + i] = new Cell(
                //     RenderSpecCharacters.Empty,
                //     cell.BackgroundColor,
                //     cell.ForegroundColor
                // );
            }
        }
    }

    public void DrawBox(int x, int y, int width, int height,
        ConsoleColor bg = ConsoleColor.Black,
        ConsoleColor fg = ConsoleColor.White)

    {
        if (_isResizing) return;
        
        SetCell(x, y, 
            new Cell(
                RenderSpecCharacters.TopLeftCorner,
                bg,
                fg
            )
        ); // top left corner

        SetCell(x + width - 1, y, 
            new Cell(
                RenderSpecCharacters.TopRightCorner,
                bg,
                fg
            )
        ); // top right corner

        SetCell(x, y + height - 1, 
            new Cell(
                RenderSpecCharacters.BottomLeftCorner,
                bg,
                fg
            )
        ); // bottom left corner

        SetCell(x + width - 1, y + height - 1,
            new Cell(
                RenderSpecCharacters.BottomRightCorner,
                bg,
                fg
            )
        ); // bottom right corner

        for (int xIndex = 1; xIndex < width - 1; xIndex++)
        {
            SetCell(x + xIndex, y,
                new Cell(
                    RenderSpecCharacters.HorizontalLine, 
                    bg, 
                    fg
                )
            );

            SetCell(x + xIndex, y + height - 1,
                new Cell(
                    RenderSpecCharacters.HorizontalLine,
                    bg,
                    fg
                )
            );
        }

        for (int yIndex = 1; yIndex < height - 1; yIndex++)
        {
            SetCell(x, y + yIndex,
                new Cell(
                    RenderSpecCharacters.VerticalLine,
                    bg,
                    fg
                )
            );
            SetCell(x + width - 1, y + yIndex,
                new Cell(
                    RenderSpecCharacters.VerticalLine,
                    bg,
                    fg
                )
            );
        }
    }

    public void FillRect(int x, int y,
        int width,
        int height,
        char character = RenderSpecCharacters.Empty,
        ConsoleColor bg = ConsoleColor.Black,
        ConsoleColor fg = ConsoleColor.White)
    {
        if (_isResizing) return;
        
        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                if (IsValidCoordinate(x + dx, y + dy))
                {
                    Cell cell = 
                        new Cell(_renderBuffer[x + dx, y + dy].Character, 
                            bg,
                            fg
                        );
                    SetCell(x + dx, y + dy, cell);
                }
            }
        }   
    }
    
    
    /*
     *     Camera-|
     * Components-->RenderManager->Renderer
     * WindowSize-|
     * 
     * RenderManager:
     *  -kiszamitja kepkockankent a komponensek pozicioit & ertekeit
     *  -rendereli az elemeket
     *
     * Renderer:
     *  -puffer torles
     *  -alapveto elemek pufferelese
     *  -render
     *
     * renderBuffer:kepernyomeretX, kepernyomeretY
     *
     * 
     */
    private ConsoleColor _lastFg = ConsoleColor.White;
    private ConsoleColor _lastBg = ConsoleColor.Black;
    private StringBuilder consoleBuffer = new StringBuilder();
    public void Render()
    {
        if(_isResizing) return;
        
        //Console.SetCursorPosition(0, 0);
        consoleBuffer.Clear();
        for (int y = 0; y < _screenHeight; y++)
        {
            for (int x = 0; x < _screenWidth; x++)
            {
                if (_dirtyMarks[x, y])
                {
                    Cell cell = _renderBuffer[x, y];
                    consoleBuffer.Append("\x1b[" + (y + 1) + ";" + (x + 1) + "H");

                    if (cell.ForegroundColor != _lastFg || cell.BackgroundColor != _lastBg)
                    {
                        consoleBuffer.Append(GetAnsiColorCode(cell.ForegroundColor, cell.BackgroundColor));
                        _lastFg = cell.ForegroundColor;
                        _lastBg = cell.BackgroundColor;
                    }

                    consoleBuffer.Append(cell.Character);
                    _dirtyMarks[x, y] = false;
                }
            }
        }
        
        consoleBuffer.Append("\x1b[0m");
        Console.Write(consoleBuffer.ToString());
    }

    private string GetAnsiColorCode(ConsoleColor fg, ConsoleColor bg)
    {
        // Convert ConsoleColor to ANSI escape codes
        int fgCode = fg switch
        {
            ConsoleColor.Black => 30,
            ConsoleColor.DarkBlue => 34,
            ConsoleColor.DarkGreen => 32,
            ConsoleColor.DarkCyan => 36,
            ConsoleColor.DarkRed => 31,
            ConsoleColor.DarkMagenta => 35,
            ConsoleColor.DarkYellow => 33,
            ConsoleColor.Gray => 37,
            ConsoleColor.DarkGray => 90,
            ConsoleColor.Blue => 94,
            ConsoleColor.Green => 92,
            ConsoleColor.Cyan => 96,
            ConsoleColor.Red => 91,
            ConsoleColor.Magenta => 95,
            ConsoleColor.Yellow => 93,
            ConsoleColor.White => 97,
            _ => 37
        };

        int bgCode = bg switch
        {
            ConsoleColor.Black => 40,
            ConsoleColor.DarkBlue => 44,
            ConsoleColor.DarkGreen => 42,
            ConsoleColor.DarkCyan => 46,
            ConsoleColor.DarkRed => 41,
            ConsoleColor.DarkMagenta => 45,
            ConsoleColor.DarkYellow => 43,
            ConsoleColor.Gray => 47,
            ConsoleColor.DarkGray => 100,
            ConsoleColor.Blue => 104,
            ConsoleColor.Green => 102,
            ConsoleColor.Cyan => 106,
            ConsoleColor.Red => 101,
            ConsoleColor.Magenta => 105,
            ConsoleColor.Yellow => 103,
            ConsoleColor.White => 107,
            _ => 40
        };

        return "\x1b[" + fgCode + ";" + bgCode + "m";
    }

    private static class RenderSpecCharacters
    {
        /*
         * SOURCE: https://ss64.com/ascii.html
         */
        public const char TopLeftCorner = (char) 0x250C;
        public const char TopRightCorner = (char) 0x2510;
        public const char BottomLeftCorner = (char) 0x2514;
        public const char BottomRightCorner = (char) 0x2518;
        
        public const char VerticalLine = (char) 0x2502;
        public const char HorizontalLine = (char) 0x2500;
        public const char Empty = ' ';
    }
}