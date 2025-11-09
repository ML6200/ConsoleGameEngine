using System;
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
    private int _width;
    private int _height;
    
    private Cell[,] _buffer;
    
    public ConsoleRenderer2D(int width, int height)
    {
        _width =  width;
        _height = height;
    }
    
    public ConsoleRenderer2D(Dimension2D dimension)
    {
        _width =  dimension.Width;
        _height = dimension.Height;
    }

    public void InitRenderer()
    {
        Console.CursorVisible = false;
        Console.Clear();
        
        _buffer = new Cell[_width, _height];
        Clear();
    }
    
    public void Clear(ConsoleColor bgColor = ConsoleColor.Black, 
        ConsoleColor fgColor = ConsoleColor.White)
    {
        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                _buffer[j, i] = new Cell(' ', bgColor, fgColor);
            }
        }
    }

    public void SetCell(int x, int y, Cell cell)
    {
        if (x >= 0 && x < _width && y >= 0 && y < _height)
        {
            _buffer[x, y] = cell;
        }
    }

    public void DrawText(int x, int y, string text, 
        ConsoleColor bgColor = ConsoleColor.Black, 
        ConsoleColor fgColor = ConsoleColor.White)
    {
        for (int i = 0; i < text.Length; i++)
        {
            SetCell(x + i, y, new Cell(text[i], bgColor, fgColor));
        }
    }

    public void DrawBox(int x, int y, int width, int height,
        ConsoleColor bg = ConsoleColor.Black,
        ConsoleColor fg = ConsoleColor.White)
        
    {
        SetCell(x, y, new Cell('┌', 
            bg, 
            fg)
        ); // top left corner
        
        SetCell(x + width - 1, y, new Cell('┐', 
            bg, 
            fg)
        ); // top right corner
        
        SetCell(x, y + height - 1, 
            new Cell('└', 
                bg, 
                fg)
            ); // bottom left corner
        
        SetCell(x + width - 1, y + height - 1, 
            new Cell('┘', 
                bg, 
                fg)
            ); // bottom right corner

        for (int xIndex = 1; xIndex < width - 1 ; xIndex++)
        {
            SetCell(x + xIndex, y , 
                new Cell('─', 
                    bg, 
                    fg)
                );
            
            SetCell(x + xIndex, y + height - 1, 
                new Cell('─', 
                    bg, 
                    fg)
                );
        }
        
        for (int yIndex = 1; yIndex < height - 1; yIndex++)
        {
            SetCell(x, y + yIndex , 
                new Cell('│', 
                    bg, 
                    fg)
                );
            SetCell(x + width - 1 , y + yIndex, 
                new Cell('│', 
                    bg, 
                    fg)
                );   
        }
    }

    public void FillRect(int x, int y, 
        int width, 
        int height,
        char character = ' ',
        ConsoleColor bg = ConsoleColor.Black,
        ConsoleColor fg = ConsoleColor.White)
    {
        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                Cell cell = _buffer[x+dx, y+dy];
                cell.BackgroundColor = bg;
                cell.ForegroundColor = fg;
                cell.Character = character;
                SetCell(x + dx, y + dy, cell);
            }
        }
    }

    public void Render()
    {
        Console.SetCursorPosition(0, 0);

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                var cell = _buffer[x, y];

                // Only change colors if they differ from current console state
                if (Console.ForegroundColor != cell.ForegroundColor)
                    Console.ForegroundColor = cell.ForegroundColor;
                if (Console.BackgroundColor != cell.BackgroundColor)
                    Console.BackgroundColor = cell.BackgroundColor;

                Console.Write(cell.Character);
            }
        }
    }


    public void Update()
    {
        Render();
        Clear();
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
        
        return $"\x1b[{fgCode};{bgCode}m";
    }
}
