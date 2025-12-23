using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
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
 */
namespace ConsoleGameEngine.Engine.Renderer;


#if PERFORMANCE_MODE
using OptionalInline = MethodImplAttribute;
#else
internal class OptionalInlineAttribute : Attribute
{
    public OptionalInlineAttribute(MethodImplOptions options)
    {
    }
}
#endif

public class ConsoleRenderer2D : IDisposable
{
    private int _screenWidth;
    private int _screenHeight;
    private Cell[,] _renderBuffer;
    private bool[,] _dirtyMarks;
    private bool _isResizing;
    private Stream _stdOut;
    
    /*
     * Worst case scenario:
     *
     * POSITION:
     * \x1b[999;999H -> 10 bytes
     * because: '\x1b' + "[999;999H"
     *          1byte  +     9 bytes = 10 bytes
     * 
     * COLOR:
     * \x1b[100;107m=>Same as before (10)
     *
     * CHAR:
     * 4 bytes (UTF8)
     *
     * we can also count with reset sequence later (\x1b[0m)
     *
     *
     * Therefore=> 10 + 10 + 4 = 24 bytes per cell
     */
    private const int BytesPerCell = 24;
    private byte[] _writeBuffer;

    public int ScreenWidth => _screenWidth;
    public int ScreenHeight => _screenHeight;
    

    public void SetDimension(int width, int height)
    {
        _isResizing = true;
        _screenWidth = width;
        _screenHeight = height;
        _renderBuffer = new Cell[_screenWidth, _screenHeight];
        _dirtyMarks = new bool[_screenWidth, _screenHeight];
        _writeBuffer= new byte[BytesPerCell * width * height];
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
        
        _stdOut = Console.OpenStandardOutput();
        SetDimension(_screenWidth, _screenHeight);

        FlushBuffer();
    }

    public void Dispose()
    {
        FlushBuffer();
        
        if (_stdOut?.CanWrite == true)
        {
            /* reset cursor and color */
            byte[] cleanup = "\x1b[?25h\x1b[0m"u8.ToArray();
            _stdOut.Write(cleanup, 0, cleanup.Length);
            _stdOut.Flush();
        }
    }
    
    [OptionalInline(MethodImplOptions.AggressiveInlining)]
    private bool IsValidCoordinate(int x, int y)
    {
        return (uint) x < _screenWidth && (uint) y < _screenHeight;
    }

    public void FlushBuffer()
    {
        if (_isResizing) return;
        
        for (int i = 0; i < _screenHeight; i++)
        {
            for (int j = 0; j < _screenWidth; j++)
            {
                _dirtyMarks[j, i] = true;
                _renderBuffer[j, i] = Cell.Empty;
            }
        }
    }
    
    [OptionalInline(MethodImplOptions.AggressiveInlining)]
    public void SetCell(int x, int y, Cell cell)
    {
        if (_isResizing)  return;
        
        if (IsValidCoordinate(x, y) 
            && !_renderBuffer[x, y].Equals(cell))
        {
            _dirtyMarks[x, y] = true;
            _renderBuffer[x, y] = cell;
        }
    }

    public void SetCell(int x, int y, Cell cell,
        RenderStyle style)
    {
        if (_isResizing) return;

        if (IsValidCoordinate(x, y)
            && !_renderBuffer[x, y].Equals(cell))
        {
            _dirtyMarks[x, y] = true;
            _renderBuffer[x, y] = cell;
        }
    }
    
    public void DrawText(int x, int y, string text,
        RenderStyle style = default)
    {
        if (_isResizing) return;
        
        for (int i = 0; i < text.Length; i++)
        {
            SetCell(x + i, y, new Cell(text[i], style));
        }
    }

    /* Box:
     * (x, y)      (x+width-1, y)
     *        +----+
     *        |TEXT|
     *        +----+    (x+width-1, y + height-1)
     * (x, y+height-1)
     */
    public void DrawBox(int x, int y, int width, int height,
        RenderStyle style = default)

    {
        if (_isResizing) return;
        
        SetCell(x, y, 
            new Cell(
                RenderSpecCharacters.TopLeftCorner,
                style
            )
        ); // top left corner

        SetCell(x + width - 1, y, 
            new Cell(
                RenderSpecCharacters.TopRightCorner,
                style
            )
        ); // top right corner

        SetCell(x, y + height - 1, 
            new Cell(
                RenderSpecCharacters.BottomLeftCorner,
                style
            )
        ); // bottom left corner

        SetCell(x + width - 1, y + height - 1,
            new Cell(
                RenderSpecCharacters.BottomRightCorner,
                style
            )
        ); // bottom right corner

        for (int xIndex = 1; xIndex < width - 1; xIndex++)
        {
            SetCell(x + xIndex, y,
                new Cell(
                    RenderSpecCharacters.HorizontalLine, 
                    style
                )
            );

            SetCell(x + xIndex, y + height - 1,
                new Cell(
                    RenderSpecCharacters.HorizontalLine,
                    style
                )
            );
        }

        for (int yIndex = 1; yIndex < height - 1; yIndex++)
        {
            SetCell(x, y + yIndex,
                new Cell(
                    RenderSpecCharacters.VerticalLine,
                    style
                )
            );
            SetCell(x + width - 1, y + yIndex,
                new Cell(
                    RenderSpecCharacters.VerticalLine,
                    style
                )
            );
        }
    }

    public void FillRect(int x, int y,
        int width,
        int height,
        char character = RenderSpecCharacters.Empty,
        RenderStyle style = default)
    {
        if (_isResizing) return;

        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                if (IsValidCoordinate(x + dx, y + dy))
                {
                    Cell cell = new Cell(character, style);
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
    private RenderStyle _lastStyle = RenderStyle.Default;
    public void Render()
    {
        if (_isResizing) return;
        int pos = 0;
        
        for (int y = 0; y < _screenHeight; y++)
        {
            int x = 0;
            while (x < _screenWidth)
            {
                if (!_dirtyMarks[x, y])
                {
                    x++;
                    continue;
                }
                
                int startX = x;
                var runStyle = _renderBuffer[startX, y].RenderStyle;
                
                int runEnd = startX + 1;
                while (runEnd < _screenWidth && _dirtyMarks[runEnd, y])
                {
                    var next = _renderBuffer[runEnd, y];
                    if (next.RenderStyle != runStyle)
                    {
                        break; 
                    }
                    runEnd++;
                }
                
                pos = WriteEscPosToBuffer(_writeBuffer, pos, startX, y);
                
                if (runStyle != _lastStyle)
                {
                    //pos = WriteColorToBuffer(_writeBuffer, pos, runFg, runBg);
                    pos = WriteStyleToBuffer(_writeBuffer, pos, runStyle);
                }
                
                for (int sx = startX; sx < runEnd; sx++)
                {
                    char ch = _renderBuffer[sx, y].Character;
                    pos = WriteCharToBuffer(_writeBuffer, pos, ch);
                    _dirtyMarks[sx, y] = false;
                }
                
                x = runEnd;
            }
        }
        
        if (pos > 0)
        {
            _stdOut.Write(_writeBuffer, 0, pos);
        }
    }

    private int WriteEscPosToBuffer(byte[] buff, int pos, int x, int y)
    {
        buff[pos++] = 0x1B; // ANSI escape char
        pos = WriteCharToBuffer(buff, pos, '[');
        
        /* We need to put 'y' first bc. ANSI positioning has 'y' as primary coordinate */
        pos = WriteIntToBuffer(buff, pos, y + 1);
        pos = WriteCharToBuffer(buff, pos, ';');
        pos = WriteIntToBuffer(buff, pos, x + 1);
        pos = WriteCharToBuffer(buff, pos, 'H');
        return pos;
    }

    private int WriteFontStyleToBuffer(byte[] buff, int pos, FontStyle style)
    {
        int fontStyleCode = style switch
        {
            FontStyle.Bold => 1,
            FontStyle.Italic => 3,
            _ => 0
        };
        pos = WriteIntToBuffer(buff, pos, fontStyleCode);
        pos = WriteCharToBuffer(buff, pos, ';');
        return pos;
    }
    
    private int WriteColorToBuffer(byte[] buff, int pos, AnsiColor fg, AnsiColor bg)
    {
        //buff[pos++] = 0x1B; // ANSI escape char
        //pos = WriteCharToBuffer(buff, pos, '[');
        pos = WriteIntToBuffer(buff, pos, GetAnsiFgColorCode(fg));
        pos = WriteCharToBuffer(buff, pos, ';');
        pos = WriteIntToBuffer(buff, pos, GetAnsiBgColorCode(bg));
        pos = WriteCharToBuffer(buff, pos, 'm');
        return pos;
    }

    private int WriteStyleToBuffer(byte[] buff, int pos, RenderStyle style)
    {
        buff[pos++] = 0x1B;
        pos = WriteCharToBuffer(buff, pos, '[');
        pos = WriteFontStyleToBuffer(buff, pos, style.FontStyle);
        pos = WriteColorToBuffer(buff, pos, style.Foreground, style.Background);
        return pos;
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
        foreach (var c in str)
        {
            pos = WriteCharToBuffer(buff, pos, c);
        }

        return pos;
    }

    // Caching precomputed char bytes with dict for fast access [o(1)]
    private readonly Dictionary<char, byte[]> _charCache = new();
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
        
        /* if the value is not computed yet */
        if (!_charCache.TryGetValue(ch, out byte[] bytes))
        {
            bytes = Encoding.UTF8.GetBytes([ch]);
            _charCache[ch] = bytes;
        }
        /* Copy the value to the byte buffer */
        Array.Copy(bytes, 0, buff, pos, bytes.Length);
        return pos + bytes.Length;
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
    
    // BAse color codes 
    private readonly int[] _foregroundColorCodes =
    [
        30, // Black = 0
        34, // DarkBlue = 1
        32, // DarkGreen = 2
        36, // DarkCyan = 3
        31, // DarkRed = 4
        35, // DarkMagenta = 5
        33, // DarkYellow = 6
        37, // Gray = 7
        90, // DarkGray = 8
        94, // Blue = 9
        92, // Green = 10
        96, // Cyan = 11
        91, // Red = 12
        95, // Magenta = 13
        93, // Yellow = 14
        97 // White = 15
    ];
    
    private readonly int[] _backgroundColorCodes =
    [
        40,  // Black = 0
        44,  // DarkBlue = 1
        42,  // DarkGreen = 2
        46,  // DarkCyan = 3
        41,  // DarkRed = 4
        45,  // DarkMagenta = 5
        43,  // DarkYellow = 6
        47,  // Gray = 7
        100, // DarkGray = 8
        104, // Blue = 9
        102, // Green = 10
        106, // Cyan = 11
        101, // Red = 12
        105, // Magenta = 13
        103, // Yellow = 14
        107 // White = 15
    ];

    private int GetAnsiFgColorCode(AnsiColor fg)
    {
        return _foregroundColorCodes[(int)fg];
    }

    private int GetAnsiBgColorCode(AnsiColor bg)
    {
        return _backgroundColorCodes[(int)bg];
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