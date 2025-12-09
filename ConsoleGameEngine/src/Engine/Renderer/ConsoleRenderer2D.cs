using System;
using System.IO;
using System.Text;
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
    private int _screenWidth;
    private int _screenHeight;
    private Cell[,] _renderBuffer;
    private bool _isResizing;
    private Stream _stdOut;
    
    /*
     * Worst case scenario:
     *
     * POSITION:
     * \x1b[999;999H -> 10 bytes
     * because: '\x1b' + "[999;999H"
     *            1byte +  9 bytes = 10 bytes
     * 
     * COLOR:
     * \x1b[100;107m
     *
     * CHAR:
     * \x1bS
     * 4 bytes (UTF8)
     *
     *
     * Therefore=> 10 + 10 + 4 = 24 bytes
     */
    private const int ScreenBytesMax = 128;
    private const int BufferSize = ScreenBytesMax * 1024;
    private byte[] _writeBuffer = new byte[BufferSize];
    

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
        
        _stdOut = Console.OpenStandardOutput(BufferSize);
        _renderBuffer = new Cell[_screenWidth, _screenHeight];

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
                    Cell cell = new Cell(character, bg, fg);
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
     *  -Clears the buffer each frame
     *  -calculates each component's position(and value) cell-by-cell
     *  -renders the components using THIS
     *
     * IMPORTANT: Keeping the order is obligatory,
     * we must clear the render buffer!!!
     *
     * Renderer:
     *  -buffering each element
     *  -render
     *
     * renderBuffer:screenWidth, ScreenHeight
     */
    private ConsoleColor _lastFg = ConsoleColor.White;
    private ConsoleColor _lastBg = ConsoleColor.Black;
    public void Render()
    {
        if(_isResizing) return;
        
        int pos = 0;
        
        for (int y = 0; y < _screenHeight; y++)
        {
            for (int x = 0; x < _screenWidth; x++)
            {
                Cell cell = _renderBuffer[x, y];
                pos = WriteEscPosToBuffer(_writeBuffer, pos, x, y);

                if (cell.ForegroundColor != _lastFg || cell.BackgroundColor != _lastBg)
                {
                    pos = WriteColorToBuffer(_writeBuffer, pos, 
                        cell.ForegroundColor, 
                        cell.BackgroundColor);
                    
                    _lastFg = cell.ForegroundColor;
                    _lastBg = cell.BackgroundColor;
                }
                
                pos = WriteCharToBuffer(_writeBuffer, pos, cell.Character);
            }
        }
        
        _stdOut.Write(_writeBuffer, 0, pos);
    }

    private int WriteEscPosToBuffer(byte[] buff, int pos, int x, int y)
    {
        buff[pos++] = 0x1B; // ANSI escape char
        buff[pos++] = (byte) '[';
        
        /* We need to put 'y' first bc. ANSI positioning has 'y' as primary coordinate */
        pos = WriteIntToBuffer(buff, pos, y + 1);
        buff[pos++] = (byte)  ';';
        pos = WriteIntToBuffer(buff, pos, x + 1);
        buff[pos++] = (byte)'H';
        return pos;
    }
    
    private int WriteColorToBuffer(byte[] buff, int pos, ConsoleColor fg, ConsoleColor bg)
    {
        return WriteStrToBuffer(buff, pos, GetAnsiColorCode(fg, bg));
    }
    
    private int WriteStrToBuffer(byte[] buff, int pos, string str)
    {
        /*
         * This should be used for colors.
         * Note that if we care about micro-opt.
         * we shouldn't use it for char sequences.
         *
         * Btw we have more concerning things in terms of performance now
         */
        for (int i = 0; i < str.Length; i++)
        {
            pos = WriteCharToBuffer(buff, pos, str[i]);
        }
        
        return pos;
    }

    private int WriteCharToBuffer(byte[] buff, int pos, char ch)
    {
        /*
         * in case we are under the limit of ASCII
         * This gives us a range of: 0x00-0x7F (0-127)
         *
         * Basically it means we save time by avoiding
         * utf8 conversion in case we dont need it (common ASCII)
         */
        if (ch < 0x80)
        {
            buff[pos++] = (byte) ch;
            return pos;
        }

        int bytes = Encoding.UTF8.GetBytes([ch], 0, 1, buff, pos);
        return pos + bytes;
    }
    
    private int WriteIntToBuffer(byte[] buff, int pos, int num)
    {
        /*
         *  Ex: 123456789
         * 1 x 10
         * 2 x 100
         * 3 x 1000
         * ...
         * 
         * 123456789 % 10 = 9
         * 123456789 - 9 = 123456780 <- we dont need this 
         * 123456780 / 10 = 12345678 | since conversion would truncate automatically
         *
         * Therefore just simply:
         * 123456789 / 10 = 12345678
         *
         * And so on...
         * 
         */
        if (num == 0)
        {
            buff[pos++] = (byte)'0';
            return pos;
        }

        int digits = (int) Math.Log10(num) + 1;
        int endPos = pos + digits;
        
        if (num < 0)
        {
            buff[pos] = (byte)'-';
            num = -num;
        }
        
        for (int i = endPos - 1; i >= pos; i--)
        {
            int remainder = num % 10;
            buff[i] = (byte) ('0' + remainder);
            num /= 10;
        }

        return endPos;
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