// <copyright file="IInputHandler.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using SkyDrop.Models;

namespace SkyDrop.Services;

/// <summary>
/// Interface for ViewModels that handle game input.
/// </summary>
public interface IGameInputHandler
{
    /// <summary>
    /// Handle a game input event.
    /// </summary>
    /// <param name="args">The game input event args.</param>
    void HandleGameInput(GameInputEventArgs args);
}

/// <summary>
/// Interface for ViewModels that handle UI navigation input.
/// </summary>
public interface IUIInputHandler
{
    /// <summary>
    /// Handle a UI input event.
    /// </summary>
    /// <param name="args">The UI input event args.</param>
    void HandleUIInput(UIInputEventArgs args);
}
