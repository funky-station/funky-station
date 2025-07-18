// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.BarSign;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class BarSignComponent : Component
{
    /// <summary>
    /// The current bar sign prototype being displayed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<BarSignPrototype>? Current;
}

[Serializable, NetSerializable]
public enum BarSignUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class SetBarSignMessage(ProtoId<BarSignPrototype> sign) : BoundUserInterfaceMessage
{
    public ProtoId<BarSignPrototype> Sign = sign;
}
