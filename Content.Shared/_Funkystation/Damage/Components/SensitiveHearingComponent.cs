// SPDX-FileCopyrightText: 2025 vectorassembly <vectorassembly@icloud.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Content.Shared.Damage.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Damage.Components;

/// <summary>
/// Tracks how much damage an entity got from loud noises around it.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SensitiveHearingSystem))]
[AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class SensitiveHearingComponent : Component
{
    /// <summary>
    /// Controls whether the eardrum rupture message have been shown or not.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), DataField("RuptureFlag")]
    [AutoNetworkedField]
    public bool RuptureFlag;

    [ViewVariables(VVAccess.ReadWrite), DataField("DamageAmount")]
    [AutoNetworkedField]
    public float DamageAmount
    {
        get => _damage;
        set
        {
            _damage = Math.Max(Math.Min(value, DeafnessThreshold * 2), 0);
            if (_damage < DeafnessThreshold)
                RuptureFlag = false;
        }
    }

    [ViewVariables(VVAccess.ReadOnly), DataField("RealDamageAmount")]
    [AutoNetworkedField]
    private float _damage;

    /// <summary>
    /// When damage reaches this value - entity goes deaf.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("WarningThreshold")]
    [AutoNetworkedField]
    public float WarningThreshold = 20.0f;

    /// <summary>
    /// When damage reaches this value - entity goes deaf.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("DeafnessThreshold")]
    [AutoNetworkedField]
    public float DeafnessThreshold = 100.0f;

    [ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<AlertCategoryPrototype> HearingAlertProtoCategory = "Hearing";

    [ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<AlertPrototype> HearingWarningAlertProtoId = "HearingWarning";

    [ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<AlertPrototype> HearingDeafAlertProtoId = "HearingDeaf";

    public bool IsDeaf {get => (DamageAmount >= DeafnessThreshold); }

    [DataField("nextUpdateTime", customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan NextThresholdUpdateTime;

    /// <summary>
    /// The time between each threshold update.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public TimeSpan ThresholdUpdateRate = TimeSpan.FromSeconds(3);


    [DataField("selfHealAmount"), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float SelfHealAmount = 5.0f;


    [DataField("nextSelfHeal", customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan NextSelfHeal;

    /// <summary>
    /// The time between self-heals.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public TimeSpan SelfHealRate = TimeSpan.FromSeconds(10);
}

