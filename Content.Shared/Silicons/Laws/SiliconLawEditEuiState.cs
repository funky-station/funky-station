// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.Silicons.Laws;

[Serializable, NetSerializable]
public sealed class SiliconLawsEuiState : EuiStateBase
{
    public List<SiliconLaw> Laws { get; }
    public NetEntity Target { get; }
    public SiliconLawsEuiState(List<SiliconLaw> laws, NetEntity target)
    {
        Laws = laws;
        Target = target;
    }
}

[Serializable, NetSerializable]
public sealed class SiliconLawsSaveMessage : EuiMessageBase
{
    public List<SiliconLaw> Laws { get; }
    public NetEntity Target { get; }

    public SiliconLawsSaveMessage(List<SiliconLaw> laws, NetEntity target)
    {
        Laws = laws;
        Target = target;
    }
}
