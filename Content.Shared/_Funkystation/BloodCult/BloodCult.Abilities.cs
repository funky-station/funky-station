// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Serialization;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Content.Shared.BloodCult.Prototypes;
using Content.Shared.Actions;
using Content.Shared.DoAfter;

namespace Content.Shared.BloodCult;

[RegisterComponent, NetworkedComponent]
public sealed partial class CultistSpellComponent : Component
{
	public static List<ProtoId<CultAbilityPrototype>> ValidSpells = new List<ProtoId<CultAbilityPrototype>>{"SummonDagger", "CultStun", "CultTwistedConstruction"};

	/// <summary>
	/// 	ID of the prototype that summons this spell.
	///		(I hate that there doesn't seem to be a way to fetch this in another way...)
	/// </summary>
	[DataField] public ProtoId<CultAbilityPrototype> AbilityId = default!;

	/// <summary>
	/// 	Specifies the number of charges remaining.
	/// </summary>
	[DataField] public uint Charges = 1;

	/// <summary>
	///		Specifies if the spell has infinite charges.
	/// </summary>
	[DataField] public bool Infinite = false;

	/// <summary>
	/// 	Specifies the health cost of the spell.
	/// </summary>
	[DataField] public int HealthCost = 0;

	/// <summary>
	/// 	Specifies the recharge time in seconds
	/// </summary>
	[DataField] public int RechargeTime = 1;

	/// <summary>
	///		The verbal invocation when used.
	/// </summary>
	[DataField] public string Invocation = "";

	/// <summary>
	///		The sound to play when cast.
	/// </summary>
	[DataField] public SoundSpecifier? CastSound = null;
}

#region DoAfters

[Serializable, NetSerializable] public sealed partial class DrawRuneDoAfterEvent : SimpleDoAfterEvent
{
	[NonSerialized] public EntityUid CarverUid;
	[NonSerialized] public EntityUid Rune;
    [NonSerialized] public EntityCoordinates Coords;
	[NonSerialized] public string EntityId;
	[NonSerialized] public int BleedOnCarve;
	[NonSerialized] public SoundSpecifier CarveSound;

    public DrawRuneDoAfterEvent(EntityUid carverUid, EntityUid rune, EntityCoordinates coords, string entityId, int bleedOnCarve, SoundSpecifier carveSound)
    {
		CarverUid = carverUid;
        Rune = rune;
        Coords = coords;
		EntityId = entityId;
		BleedOnCarve = bleedOnCarve;
		CarveSound = carveSound;
    }
}

[Serializable, NetSerializable] public sealed partial class CarveSpellDoAfterEvent : SimpleDoAfterEvent
{
	[NonSerialized] public EntityUid CarverUid;
	[NonSerialized] public CultAbilityPrototype CultAbility;
	[NonSerialized] public bool RecordKnownSpell;
	[NonSerialized] public bool StandingOnRune;

    public CarveSpellDoAfterEvent(EntityUid carverUid, CultAbilityPrototype cultAbility, bool recordKnownSpell, bool standingOnRune)
    {
		CarverUid = carverUid;
		CultAbility = cultAbility;
		RecordKnownSpell = recordKnownSpell;
		StandingOnRune = standingOnRune;
    }
}

#endregion

#region Spells

[Serializable, NetSerializable]
public sealed partial class SpellsMessage : BoundUserInterfaceMessage
{
	public ProtoId<CultAbilityPrototype> ProtoId;

	public SpellsMessage(ProtoId<CultAbilityPrototype> protoId)
	{
		ProtoId = protoId;
	}
}
[Serializable, NetSerializable]
public enum SpellsUiKey : byte
{
	Key
}

[Serializable, NetSerializable]
public sealed class BloodCultSpellsBuiState : BoundUserInterfaceState
{
	public BloodCultSpellsBuiState()
	{}
}

public sealed partial class EventCultistStudyVeil : InstantActionEvent { }
public sealed partial class EventCultistSummonDagger : InstantActionEvent { }
public sealed partial class EventCultistStun : EntityTargetActionEvent { }
public sealed partial class EventCultistTwistedConstruction : EntityTargetActionEvent { }

#endregion
