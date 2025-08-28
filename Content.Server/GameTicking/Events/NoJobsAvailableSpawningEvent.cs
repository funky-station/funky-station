// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Player;

namespace Content.Server.GameTicking.Events;

/// <summary>
/// Raised on players who attempt to spawn in but fail to get a job, due to there not being any job slots available.
/// </summary>
public readonly record struct NoJobsAvailableSpawningEvent(ICommonSession Player);
