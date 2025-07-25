// SPDX-FileCopyrightText: 2025 V <97265903+formlessnameless@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._EE.Overlays;

public abstract partial class BaseOverlayComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public virtual Vector3 Tint { get; set; } = new(0.3f, 0.3f, 0.3f);

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public virtual float Strength { get; set; } = 2f;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public virtual float Noise { get; set; } = 0.5f;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public virtual Color Color { get; set; } = Color.White;
}
