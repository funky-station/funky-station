// SPDX-FileCopyrightText: 2022 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader;

[Virtual]
[Serializable, NetSerializable]
public class CartridgeLoaderUiState : BoundUserInterfaceState
{
    public NetEntity? ActiveUI;
    public List<NetEntity> Programs;

    public CartridgeLoaderUiState(List<NetEntity> programs, NetEntity? activeUI)
    {
        Programs = programs;
        ActiveUI = activeUI;
    }
}
