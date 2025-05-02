// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 a.rudenko <creadth@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Atmos.Components;

[Serializable, NetSerializable]
public enum SharedGasTankUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class GasTankToggleInternalsMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class GasTankSetPressureMessage : BoundUserInterfaceMessage
{
    public float Pressure;
}

[Serializable, NetSerializable]
public sealed class GasTankBoundUserInterfaceState : BoundUserInterfaceState
{
    public float TankPressure;
}
