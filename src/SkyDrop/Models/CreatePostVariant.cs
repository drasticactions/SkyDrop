// <copyright file="CreatePostVariant.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace SkyDrop.Models;

/// <summary>
/// Defines the variant of CreatePost gameplay mode.
/// </summary>
public enum CreatePostVariant
{
    /// <summary>
    /// Standard mode - compose posts using T9/ABC/Kana input during gameplay.
    /// </summary>
    Standard,

    /// <summary>
    /// Queued mode - pre-write posts before game, words revealed by clearing lines.
    /// </summary>
    Queued
}
