using System;
using System.Collections.Generic;
using System.Text;

namespace SkyDrop.Models;

/// <summary>
/// Input mode for text entry.
/// </summary>
public enum TextInputMode
{
    /// <summary>
    /// T9 predictive text mode - digits build words via dictionary lookup.
    /// </summary>
    T9,

    /// <summary>
    /// ABC multi-tap mode - rotation cycles through individual characters on the key.
    /// </summary>
    ABC,

    /// <summary>
    /// Kana input mode - Japanese hiragana input with kanji prediction.
    /// </summary>
    Kana
}
