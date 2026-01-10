// <copyright file="InputMappings.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Avalonia.Input;
using SkyDrop.Models;

namespace SkyDrop.Services;

/// <summary>
/// Centralized input mappings - single source of truth for key bindings.
/// Eliminates all duplicate MapKeyToInput() methods across game views.
/// </summary>
public static class InputMappings
{
    /// <summary>
    /// Default keyboard to GameInput mappings.
    /// </summary>
    public static readonly IReadOnlyDictionary<Key, GameInput> KeyboardGameMappings =
        new Dictionary<Key, GameInput>
        {
            // Arrow keys
            { Key.Left, GameInput.Left },
            { Key.Right, GameInput.Right },
            { Key.Down, GameInput.SoftDrop },
            { Key.Up, GameInput.RotateCW },

            // WASD (A/D for left/right, S for soft drop)
            { Key.A, GameInput.Left },
            { Key.D, GameInput.Right },
            { Key.S, GameInput.SoftDrop },

            // Action keys
            { Key.Space, GameInput.HardDrop },
            { Key.X, GameInput.RotateCW },
            { Key.Z, GameInput.RotateCCW },

            // Pause
            { Key.Escape, GameInput.Pause },
            { Key.P, GameInput.Pause },

            // Confirm (for game over restart, etc.)
            { Key.Enter, GameInput.Confirm },
        };

    /// <summary>
    /// Default keyboard to UIInput mappings for menu navigation.
    /// </summary>
    public static readonly IReadOnlyDictionary<Key, UIInput> KeyboardUIMappings =
        new Dictionary<Key, UIInput>
        {
            // Arrow keys
            { Key.Up, UIInput.Up },
            { Key.Down, UIInput.Down },
            { Key.Left, UIInput.Left },
            { Key.Right, UIInput.Right },

            // WASD
            { Key.W, UIInput.Up },
            { Key.S, UIInput.Down },
            { Key.A, UIInput.Left },
            { Key.D, UIInput.Right },

            // Action keys
            { Key.Enter, UIInput.Confirm },
            { Key.Escape, UIInput.Cancel },
            { Key.Tab, UIInput.Secondary },
        };

    // Future: Gamepad mappings
    // public static readonly IReadOnlyDictionary<GamepadButton, GameInput> GamepadGameMappings;
    // public static readonly IReadOnlyDictionary<GamepadButton, UIInput> GamepadUIMappings;
}
