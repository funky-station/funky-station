// SPDX-FileCopyrightText: 2025 jackel234 <52829582+jackel234@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Shared._RMC14.Weapons.Common;

public sealed class UniqueActionEvent(EntityUid userUid) : HandledEntityEventArgs
{
    public readonly EntityUid UserUid = userUid;
}
