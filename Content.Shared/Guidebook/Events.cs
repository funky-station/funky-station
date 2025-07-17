// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Guidebook;

/// <summary>
/// Raised by the client on GuidebookDataSystem Initialize to request a
/// full set of guidebook data from the server.
/// </summary>
[Serializable, NetSerializable]
public sealed class RequestGuidebookDataEvent : EntityEventArgs { }

/// <summary>
/// Raised by the server at a specific client in response to <see cref="RequestGuidebookDataEvent"/>.
/// Also raised by the server at ALL clients when prototype data is hot-reloaded.
/// </summary>
[Serializable, NetSerializable]
public sealed class UpdateGuidebookDataEvent : EntityEventArgs
{
    public GuidebookData Data;

    public UpdateGuidebookDataEvent(GuidebookData data)
    {
        Data = data;
    }
}
