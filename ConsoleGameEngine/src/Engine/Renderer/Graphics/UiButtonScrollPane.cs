using System;
using System.Collections.Generic;
using ConsoleGameEngine.Engine.Input;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class UiButtonScrollPane : UiPanel, IFocusable
{
    //NOT YET IMPLEMENTED!!!!!
    private string _text = "";

    public List<UiButton> Buttons;
    public bool IsFocused { get; set; }
    public bool CanFocus { get; set; } = true;
    
    public bool HasBorder { get; set; } = false;

    public event EventHandler OnClick;
    
    public ConsoleColor FocusedBgColor { get; set; } = ConsoleColor.DarkBlue;
    
    public UiButtonScrollPane(string text)
    {
        Buttons = new List<UiButton>();
        throw new NotImplementedException();
    }

    public UiButtonScrollPane()
    {
        Buttons = new List<UiButton>();
        throw new NotImplementedException();
    }


    public void OnFocusGained()
    {
        HasBorder = true;
    }

    public void OnFocusLost()
    {
        HasBorder = false;
    }

    public void OnFocusActivate()
    {
        int offset = 0;
        foreach (var child in Children)
        {
            var childWordPos = child.WorldPosition + offset;
        }
    }
}