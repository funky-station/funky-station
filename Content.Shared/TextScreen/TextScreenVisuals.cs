// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 avery <51971268+graevy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.TextScreen;

[Serializable, NetSerializable]
public enum TextScreenVisuals : byte
{
    // TODO: support for a small image, I think. Probably want to rename textscreen to just screen then.
    /// <summary>
    ///     What text to default to after timer completion?
    ///     Expects a <see cref="string"/>.
    /// </summary>
    DefaultText,
    /// <summary>
    ///     What text to render? <br/>
    ///     Expects a <see cref="string"/>.
    /// </summary>
    ScreenText,

    /// <summary>
    ///     What is the target time? <br/>
    ///     Expects a <see cref="TimeSpan"/>.
    /// </summary>
    TargetTime,

    /// <summary>
    ///     Change text color on the entire screen
    ///     Expects a <see cref="Color"/>.
    /// </summary>
    Color
}
