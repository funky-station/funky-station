// SPDX-FileCopyrightText: 2026 otokonoko-dev <248204705+otokonoko-dev@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.Paper;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BookPaginationComponent : Component
{
    [DataField, AutoNetworkedField]
    public int CurrentPage;

    [DataField]
    public int LinesPerPage = 19;
}
