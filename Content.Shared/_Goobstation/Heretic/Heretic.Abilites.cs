// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Heretic;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticActionComponent : Component
{
    /// <summary>
    ///     Indicates that a user should wear a heretic amulet, a hood or something else.
    /// </summary>
    [DataField] public bool RequireMagicItem = true;

    [DataField] public string? MessageLoc = null;
}

#region DoAfters

[Serializable, NetSerializable] public sealed partial class EldritchInfluenceDoAfterEvent : SimpleDoAfterEvent
{
    public bool MagicItemActive = false;
}
[Serializable, NetSerializable] public sealed partial class DrawRitualRuneDoAfterEvent : SimpleDoAfterEvent
{
    [NonSerialized] public EntityCoordinates Coords;
    [NonSerialized] public EntityUid RitualRune;

    public DrawRitualRuneDoAfterEvent(EntityUid ritualRune, EntityCoordinates coords)
    {
        RitualRune = ritualRune;
        Coords = coords;
    }
}
[Serializable, NetSerializable] public sealed partial class HereticMansusLinkDoAfter : SimpleDoAfterEvent
{
    [NonSerialized] public EntityUid Target;

    public HereticMansusLinkDoAfter(EntityUid target)
    {
        Target = target;
    }
}
[Serializable, NetSerializable] public sealed partial class EventHereticFleshSurgeryDoAfter : SimpleDoAfterEvent
{
    [NonSerialized] public EntityUid? Target;

    public EventHereticFleshSurgeryDoAfter(EntityUid target)
    {
        Target = target;
    }
}

#endregion

#region Abilities

/// <summary>
///     Raised whenever we need to check for a magic item before casting a spell that requires one to be worn.
/// </summary>
public sealed partial class CheckMagicItemEvent : HandledEntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}

// basic
public sealed partial class EventHereticOpenStore : InstantActionEvent { }
public sealed partial class EventHereticMansusGrasp : InstantActionEvent { }
public sealed partial class EventHereticLivingHeart : InstantActionEvent { } // opens ui
[Serializable, NetSerializable] public sealed partial class EventHereticLivingHeartActivate : BoundUserInterfaceMessage // triggers the logic
{
    public NetEntity? Target { get; set; }
}
[Serializable, NetSerializable] public enum HereticLivingHeartKey : byte
{
    Key
}

// for mobs
public sealed partial class EventHereticMansusLink : EntityTargetActionEvent { }

// ash
public sealed partial class EventHereticAshenShift : InstantActionEvent { }
public sealed partial class EventHereticVolcanoBlast : InstantActionEvent { }
public sealed partial class EventHereticNightwatcherRebirth : InstantActionEvent { }
public sealed partial class EventHereticFlames : InstantActionEvent { }
public sealed partial class EventHereticCascade : InstantActionEvent { }


// flesh
public sealed partial class EventHereticFleshSurgery : EntityTargetActionEvent { }
public sealed partial class EventHereticFleshAscend : InstantActionEvent { }

// void (including upgrades)
[Serializable, NetSerializable, DataDefinition] public sealed partial class HereticAristocratWayEvent : EntityEventArgs { }
[Serializable, NetSerializable, DataDefinition] public sealed partial class HereticAscensionVoidEvent : EntityEventArgs { }
public sealed partial class HereticVoidBlastEvent : InstantActionEvent { }
public sealed partial class HereticVoidBlinkEvent : WorldTargetActionEvent { }
public sealed partial class HereticVoidPullEvent : InstantActionEvent { }

// ascensions
[Serializable, NetSerializable, DataDefinition] public sealed partial class HereticAscensionAshEvent : EntityEventArgs { }
#endregion
