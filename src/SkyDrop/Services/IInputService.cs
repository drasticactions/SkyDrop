// <copyright file="IInputService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Avalonia.Input;
using SkyDrop.Models;

namespace SkyDrop.Services;

/// <summary>
/// Abstracts input handling for keyboard and future gamepad support.
/// Views subscribe to events; the service raises them based on raw input.
/// </summary>
public interface IInputService
{
    /// <summary>
    /// Raised when a game input action occurs (supports press and release for DAS).
    /// </summary>
    event EventHandler<GameInputEventArgs>? GameInputChanged;

    /// <summary>
    /// Raised when a UI navigation input occurs (single-action, no press/release).
    /// </summary>
    event EventHandler<UIInputEventArgs>? UIInputReceived;

    /// <summary>
    /// Gets the current active input source (for displaying appropriate button prompts).
    /// </summary>
    InputSource ActiveInputSource { get; }

    /// <summary>
    /// Raised when the active input source changes.
    /// </summary>
    event EventHandler<InputSource>? InputSourceChanged;

    /// <summary>
    /// Process a raw key down event from Avalonia.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    void ProcessKeyDown(Key key);

    /// <summary>
    /// Process a raw key up event from Avalonia.
    /// </summary>
    /// <param name="key">The key that was released.</param>
    void ProcessKeyUp(Key key);
}
