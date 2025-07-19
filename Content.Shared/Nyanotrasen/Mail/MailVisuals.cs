// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Mail
{
    /// <summary>
    /// Stores the visuals for mail.
    /// </summary>
    [Serializable, NetSerializable]
    public enum MailVisuals : byte
    {
        IsLocked,
        IsTrash,
        IsBroken,
        IsFragile,
        IsPriority,
        IsPriorityInactive,
        JobIcon,
    }
}
