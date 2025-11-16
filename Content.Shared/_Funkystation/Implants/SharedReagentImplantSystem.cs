// SPDX-FileCopyrightText: 2025 TheSecondLord <88201625+TheSecondLord@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Shared._Funkystation.Implants;

public abstract class SharedReagentImplantSystem : EntitySystem { }

public sealed partial class UseReagentImplantEvent : InstantActionEvent { }
