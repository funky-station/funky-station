// SPDX-FileCopyrightText: 2021 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 c4llv07e <38111072+c4llv07e@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Shared.SubFloor;

[RegisterComponent, NetworkedComponent]
public sealed partial class TrayScannerComponent : Component
{
    /// <summary>
    ///     Whether the scanner is currently on.
    /// </summary>
    [DataField]
    public bool Enabled;

    /// <summary>
    ///     Radius in which the scanner will reveal entities. Centered on the <see cref="LastLocation"/>.
    /// </summary>
    [DataField]
    public float Range = 4f;

    /// <summary>
    /// The action used to toggle the scanner when equipped.
    /// </summary>
    [DataField]
    public EntProtoId ActionId = "ActionToggleTrayScanner";

    [DataField]
    public EntityUid? ActionEntity;

    [DataField]
    public SoundSpecifier? OnSound = new SoundPathSpecifier("/Audio/_EE/Items/Goggles/activate.ogg");

    [DataField]
    public SoundSpecifier? OffSound = new SoundPathSpecifier("/Audio/_EE/Items/Goggles/deactivate.ogg");
}

[Serializable, NetSerializable]
public sealed class TrayScannerState : ComponentState
{
    public bool Enabled;
    public float Range;

    public TrayScannerState(bool enabled, float range)
    {
        Enabled = enabled;
        Range = range;
    }
}
