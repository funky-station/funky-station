// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 YourName
//
// SPDX-License-Identifier: MIT

using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Maths;
using Content.Client.Resources;

namespace Content.Client.MalfAI.Theme;

/// <summary>
/// Centralized Malf AI UI theme utilities (accent color, font, and style factories).
/// Use these helpers to apply the Malf theme consistently across different UIs.
/// </summary>
public static class MalfUiTheme
{
    /// <summary>
    /// Malf accent color (green).
    /// </summary>
    public static readonly Color Accent = new(0f, 1f, 0f);

    /// <summary>
    /// Path to the KodeMono font used in Malf-themed UIs.
    /// </summary>
    public const string FontPath = "/Fonts/_DV/KodeMono/KodeMono-Regular.ttf";

    /// <summary>
    /// Loads the Malf font from resources.
    /// </summary>
    public static Font GetFont(IResourceCache cache, int size = 12)
        => cache.GetFont(FontPath, size);

    /// <summary>
    /// Creates the main black panel style with a green border.
    /// </summary>
    public static StyleBoxFlat CreateMainPanelStyle(Color? accent = null)
    {
        var a = accent ?? Accent;
        var style = new StyleBoxFlat
        {
            BackgroundColor = Color.Black,
            BorderColor = a,
            BorderThickness = new Thickness(2f)
        };
        style.ContentMarginLeftOverride = 6;
        style.ContentMarginTopOverride = 4;
        style.ContentMarginRightOverride = 6;
        style.ContentMarginBottomOverride = 4;
        return style;
    }

    /// <summary>
    /// Creates the category panel style (very dark background with green border).
    /// </summary>
    public static StyleBoxFlat CreateCategoryPanelStyle(Color? accent = null)
    {
        var a = accent ?? Accent;
        var style = new StyleBoxFlat
        {
            BackgroundColor = new Color(0.02f, 0.02f, 0.03f, 1f),
            BorderColor = a,
            BorderThickness = new Thickness(2f)
        };
        style.ContentMarginLeftOverride = 4;
        style.ContentMarginTopOverride = 4;
        style.ContentMarginRightOverride = 4;
        style.ContentMarginBottomOverride = 4;
        return style;
    }

    /// <summary>
    /// Creates a green-bordered black button style.
    /// </summary>
    public static StyleBoxFlat CreateButtonStyle(Color? accent = null)
    {
        var a = accent ?? Accent;
        var style = new StyleBoxFlat
        {
            BackgroundColor = Color.Black,
            BorderColor = a,
            BorderThickness = new Thickness(2f)
        };
        style.ContentMarginLeftOverride = 4;
        style.ContentMarginTopOverride = 2;
        style.ContentMarginRightOverride = 4;
        style.ContentMarginBottomOverride = 2;
        return style;
    }

    /// <summary>
    /// Creates a solid black backdrop (no border) for window backgrounds.
    /// </summary>
    public static StyleBoxFlat CreateBackdropStyle()
    {
        return new StyleBoxFlat
        {
            BackgroundColor = Color.Black
        };
    }

    /// <summary>
    /// Creates a black list entry panel with a green outline for Malf-themed lists.
    /// </summary>
    public static StyleBoxFlat CreateEntryPanelStyle(Color? accent = null)
    {
        var a = accent ?? Accent;
        var style = new StyleBoxFlat
        {
            BackgroundColor = Color.Black,
            BorderColor = a,
            BorderThickness = new Thickness(2f)
        };
        style.ContentMarginLeftOverride = 6;
        style.ContentMarginTopOverride = 6;
        style.ContentMarginRightOverride = 6;
        style.ContentMarginBottomOverride = 6;
        return style;
    }

    /// <summary>
    /// Creates a hollow square checkbox style for the inactive state (transparent background with green border).
    /// </summary>
    public static StyleBoxFlat CreateCheckboxInactiveStyle(Color? accent = null)
    {
        var a = accent ?? Accent;
        return new StyleBoxFlat
        {
            BackgroundColor = Color.Transparent,
            BorderColor = a,
            BorderThickness = new Thickness(2f)
        };
    }

    /// <summary>
    /// Creates a filled square checkbox style for the active state (green background with green border).
    /// </summary>
    public static StyleBoxFlat CreateCheckboxActiveStyle(Color? accent = null)
    {
        var a = accent ?? Accent;
        return new StyleBoxFlat
        {
            BackgroundColor = a,
            BorderColor = a,
            BorderThickness = new Thickness(2f)
        };
    }

}
