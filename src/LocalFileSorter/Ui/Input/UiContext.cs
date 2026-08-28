using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace LocalFileSorter.Ui.Input;

public sealed class UiContext
{
    private readonly HashSet<Keyboard.Key> pressedKeys = [];

    private bool leftDown;
    private bool leftPressed;
    private bool leftReleased;
    private float wheelDelta;

    public Vector2f MousePosition { get; private set; }

    public bool LeftDown => leftDown;

    public bool LeftPressed => leftPressed;

    public bool LeftReleased => leftReleased;

    public float WheelDelta => wheelDelta;

    public bool Blocked { get; set; }

    public void BeginFrame()
    {
        leftPressed = false;
        leftReleased = false;
        wheelDelta = 0f;
        pressedKeys.Clear();
    }

    public void Attach(WindowBase window)
    {
        window.MouseMoved += (_, e) => MousePosition = new Vector2f(e.Position.X, e.Position.Y);
        window.KeyPressed += (_, e) => pressedKeys.Add(e.Code);
        window.MouseButtonPressed += (_, e) => OnButton(e, down: true);
        window.MouseButtonReleased += (_, e) => OnButton(e, down: false);
        window.MouseWheelScrolled += (_, e) =>
        {
            MousePosition = new Vector2f(e.Position.X, e.Position.Y);
            if (e.Wheel == Mouse.Wheel.Vertical)
            {
                wheelDelta += e.Delta;
            }
        };
    }

    public bool KeyPressed(Keyboard.Key key) => !Blocked && pressedKeys.Contains(key);

    public bool IsHovering(FloatRect area) => !Blocked && area.Contains(MousePosition);

    public bool ClickedIn(FloatRect area) => leftPressed && IsHovering(area);

    public float WheelOver(FloatRect area) => IsHovering(area) ? wheelDelta : 0f;

    private void OnButton(MouseButtonEventArgs e, bool down)
    {
        MousePosition = new Vector2f(e.Position.X, e.Position.Y);
        if (e.Button != Mouse.Button.Left)
        {
            return;
        }

        leftDown = down;
        if (down)
        {
            leftPressed = true;
        }
        else
        {
            leftReleased = true;
        }
    }
}
