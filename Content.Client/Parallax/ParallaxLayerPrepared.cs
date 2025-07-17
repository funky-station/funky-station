// SPDX-FileCopyrightText: 2022 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System;
using Robust.Client.Graphics;
using Content.Client.Parallax.Data;
using Robust.Shared.Graphics;

namespace Content.Client.Parallax;

/// <summary>
/// A 'prepared' (i.e. texture loaded and ready to use) parallax layer.
/// </summary>
public struct ParallaxLayerPrepared
{
    /// <summary>
    /// The loaded texture for this layer.
    /// </summary>
    public Texture Texture { get; set; }

    /// <summary>
    /// The configuration for this layer.
    /// </summary>
    public ParallaxLayerConfig Config { get; set; }
}

