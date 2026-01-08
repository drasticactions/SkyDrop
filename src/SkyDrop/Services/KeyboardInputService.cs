// <copyright file="KeyboardInputService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Avalonia.Input;
using SkyDrop.Models;

namespace SkyDrop.Services;

/// <summary>
/// Keyboard implementation of IInputService.
/// Processes Avalonia key events and raises appropriate input events.
/// </summary>
public class KeyboardInputService : IInputService
{
    private readonly HashSet<Key> _pressedKeys = new();

    /// <inheritdoc/>
    public event EventHandler<GameInputEventArgs>? GameInputChanged;

    /// <inheritdoc/>
    public event EventHandler<UIInputEventArgs>? UIInputReceived;

    /// <inheritdoc/>
    public event EventHandler<InputSource>? InputSourceChanged;

    /// <inheritdoc/>
    public InputSource ActiveInputSource => InputSource.Keyboard;

    /// <inheritdoc/>
    public void ProcessKeyDown(Key key)
    {
        // Prevent key repeat from firing multiple down events
        if (!_pressedKeys.Add(key))
        {
            return;
        }

        // Check game input mapping
        if (InputMappings.KeyboardGameMappings.TryGetValue(key, out var gameInput))
        {
            GameInputChanged?.Invoke(this, new GameInputEventArgs(
                gameInput,
                InputSource.Keyboard,
                isPressed: true));
        }

        // Check UI input mapping (menus don't need release tracking)
        if (InputMappings.KeyboardUIMappings.TryGetValue(key, out var uiInput))
        {
            UIInputReceived?.Invoke(this, new UIInputEventArgs(
                uiInput,
                InputSource.Keyboard));
        }
    }

    /// <inheritdoc/>
    public void ProcessKeyUp(Key key)
    {
        if (!_pressedKeys.Remove(key))
        {
            return;
        }

        // Only game inputs need release events (for DAS)
        if (InputMappings.KeyboardGameMappings.TryGetValue(key, out var gameInput))
        {
            GameInputChanged?.Invoke(this, new GameInputEventArgs(
                gameInput,
                InputSource.Keyboard,
                isPressed: false));
        }
    }
}
