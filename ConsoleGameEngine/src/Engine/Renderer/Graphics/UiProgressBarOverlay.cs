using System;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

/// <summary>
/// A modal progress bar overlay that displays loading progress
/// </summary>
public class UiProgressBarOverlay : UiPanel
{
    private readonly UiLabel _titleLabel;
    private readonly UiLabel _statusLabel;
    private readonly ProgressBar _progressBar;
    private readonly GraphicsComponent _parent;

    public UiProgressBarOverlay(GraphicsComponent parent, string title = "Loading...")
    {
        _parent = parent;

        ForegroundColor = ConsoleColor.White;
        BackgroundColor = ConsoleColor.DarkBlue;
        BorderColor = ConsoleColor.White;
        HasBorder = true;

        // Create title label
        _titleLabel = new UiLabel(title)
        {
            ForegroundColor = ConsoleColor.White,
            BackgroundColor = BackgroundColor,
        };

        // Create status label
        _statusLabel = new UiLabel("")
        {
            ForegroundColor = ConsoleColor.Gray,
            BackgroundColor = BackgroundColor,
        };

        // Create progress bar
        _progressBar = new ProgressBar()
        {
            Size = new Dimension2D(40, 1)
        };

        AddChild(_titleLabel);
        AddChild(_statusLabel);
        AddChild(_progressBar);

        ComputeSizeAndPosition();

        parent.AddChild(this);
    }

    /// <summary>
    /// Updates the progress (0.0 to 1.0)
    /// </summary>
    public void SetProgress(float progress)
    {
        _progressBar.SetProgress(progress, 0); // No animation for smoother updates
    }

    /// <summary>
    /// Updates the status text below the progress bar
    /// </summary>
    public void SetStatus(string status)
    {
        _statusLabel.Text = status;
        ComputeSizeAndPosition();
    }

    /// <summary>
    /// Closes the overlay
    /// </summary>
    public void Close()
    {
        _parent.RemoveChild(this);
        Visible = false;
    }

    private void ComputeSizeAndPosition()
    {
        int maxWidth = Math.Max(_titleLabel.Size.Width, _progressBar.Size.Width);
        maxWidth = Math.Max(maxWidth, _statusLabel.Size.Width);

        int width = maxWidth + 4; // padding
        int height = 6; // Title + progress bar + status + padding

        Size = new Dimension2D(width, height);

        // Center on parent
        int midPosPx = _parent.Size.Width / 2;
        int midPosTx = Size.Width / 2;
        int midPosPy = _parent.Size.Height / 2;
        int midPosTy = Size.Height / 2;

        RelativePosition = new Point2D(midPosPx - midPosTx, midPosPy - midPosTy);

        // Position children
        _titleLabel.RelativePosition = new Point2D(Size.Width / 2 - _titleLabel.Size.Width / 2, 1);
        _progressBar.RelativePosition = new Point2D(Size.Width / 2 - _progressBar.Size.Width / 2, 3);
        _statusLabel.RelativePosition = new Point2D(Size.Width / 2 - _statusLabel.Size.Width / 2, 4);
    }
}
