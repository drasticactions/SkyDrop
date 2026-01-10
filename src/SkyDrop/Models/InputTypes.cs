// <copyright file="InputTypes.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using SkyDrop.Services;

namespace SkyDrop.Models;

/// <summary>
/// Input actions for UI/menu navigation (single-action events).
/// </summary>
public enum UIInput
{
    /// <summary>Navigate up.</summary>
    Up,

    /// <summary>Navigate down.</summary>
    Down,

    /// <summary>Navigate left.</summary>
    Left,

    /// <summary>Navigate right.</summary>
    Right,

    /// <summary>Confirm/select action (Enter/A button).</summary>
    Confirm,

    /// <summary>Cancel/back action (Escape/B button).</summary>
    Cancel,

    /// <summary>Secondary action (Tab/Select button).</summary>
    Secondary
}

/// <summary>
/// Identifies the source of input for UI prompt display.
/// </summary>
public enum InputSource
{
    /// <summary>Keyboard input.</summary>
    Keyboard,

    /// <summary>Gamepad/controller input.</summary>
    Gamepad
}

/// <summary>
/// Event args for game input events (supports press/release for DAS).
/// </summary>
public class GameInputEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameInputEventArgs"/> class.
    /// </summary>
    /// <param name="input">The game input action.</param>
    /// <param name="source">The input source.</param>
    /// <param name="isPressed">True for key down, false for key up.</param>
    public GameInputEventArgs(GameInput input, InputSource source, bool isPressed)
    {
        Input = input;
        Source = source;
        IsPressed = isPressed;
    }

    /// <summary>
    /// Gets the game input action.
    /// </summary>
    public GameInput Input { get; }

    /// <summary>
    /// Gets the input source.
    /// </summary>
    public InputSource Source { get; }

    /// <summary>
    /// Gets a value indicating whether the key is pressed (true) or released (false).
    /// </summary>
    public bool IsPressed { get; }
}

/// <summary>
/// Event args for UI input events (single action, no press/release).
/// </summary>
public class UIInputEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UIInputEventArgs"/> class.
    /// </summary>
    /// <param name="input">The UI input action.</param>
    /// <param name="source">The input source.</param>
    public UIInputEventArgs(UIInput input, InputSource source)
    {
        Input = input;
        Source = source;
    }

    /// <summary>
    /// Gets the UI input action.
    /// </summary>
    public UIInput Input { get; }

    /// <summary>
    /// Gets the input source.
    /// </summary>
    public InputSource Source { get; }
}
