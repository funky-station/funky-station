// SPDX-FileCopyrightText: 2019 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Nutrition.Components
{
    // TODO: Remove maybe? Add visualizer for food
    [Serializable, NetSerializable]
    public enum FoodVisuals : byte
    {
        Visual,
        MaxUses,
    }

    [Serializable, NetSerializable]
    public enum OpenableVisuals : byte
    {
        Opened,
        Layer
    }

    [Serializable, NetSerializable]
    public enum SealableVisuals : byte
    {
        Sealed,
        Layer,
    }
}
