// SPDX-FileCopyrightText: 2025 mq <113324899+mqole@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Physics.Components;
using static Content.Shared._EE.CCVars.ECCVars;

namespace Content.Shared._EE.Contests;

public sealed partial class ContestsSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private bool _doContests;
    private bool _doMassContest;
    private bool _doStamContest;
    private float _contestsMaxPercentage;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_cfg, DoContestsSystem, value => _doContests = value, true);
        Subs.CVar(_cfg, ContestsMaxPercentage, value => _contestsMaxPercentage = value, true);

        if (_doContests)
        {
            Subs.CVar(_cfg, DoMassContests, value => _doMassContest = value, true);
            Subs.CVar(_cfg, DoStaminaContests, value => _doStamContest = value, true);
        }
        else
        {
            _doMassContest = false;
            _doStamContest = false;
        }
    }

    #region Mass Contests

    /// <summary>
    ///     Outputs the ratio of mass between a performer and a target
    /// </summary>
    public float MassContest(Entity<PhysicsComponent?> performer, Entity<PhysicsComponent?> target, float rangeFactor = 1f)
    {
        if (!_doContests || !_doMassContest
            || !Resolve(performer, ref performer.Comp)
            || !Resolve(target, ref target.Comp)
            || performer.Comp.Mass == 0
            || target.Comp.InvMass == 0)
            return 1f;

        return ContestClamp(Math.Clamp(performer.Comp.Mass * target.Comp.InvMass,
                1 - _contestsMaxPercentage * rangeFactor,
                1 + _contestsMaxPercentage * rangeFactor));
    }

    #endregion

    #region Stamina Contests

    /// <summary>
    ///     Outputs 1 minus the percentage of an Entity's Stamina, with a Range of [Epsilon, 1 - _contestsMaxPercentage * rangeFactor].
    ///     This will never return a value >1.
    /// </summary>
    public float StaminaContest(Entity<StaminaComponent?> performer, float rangeFactor = 1f)
    {
        if (!_doContests || _doStamContest
            || !Resolve(performer, ref performer.Comp)
            || performer.Comp.StaminaDamage == 0)
            return 1f;

        return ContestClamp(1 - Math.Clamp(performer.Comp.StaminaDamage
            / performer.Comp.CritThreshold, 0, _contestsMaxPercentage * rangeFactor));
    }

    /// <summary>
    ///     Outputs the ratio of percentage of an Entity's Stamina and a Target Entity's Stamina, with a Range of [Epsilon, _contestsMaxPercentage * rangeFactor], or a range of [Epsilon, +inf] if bypassClamp is true.
    ///     This does NOT produce the same kind of outputs as a Single-Entity StaminaContest. 2Entity StaminaContest returns the product of two Solo Stamina Contests, and so its values can be very strange.
    /// </summary>
    public float StaminaContest(Entity<StaminaComponent?> performer, Entity<StaminaComponent?> target, float rangeFactor = 1f)
    {
        if (!_doContests || _doStamContest
            || !Resolve(performer, ref performer.Comp)
            || !Resolve(target, ref target.Comp))
            return 1f;

        return ContestClamp((1 - Math.Clamp(performer.Comp.StaminaDamage / performer.Comp.CritThreshold, 0, _contestsMaxPercentage * rangeFactor))
                / (1 - Math.Clamp(target.Comp.StaminaDamage / target.Comp.CritThreshold, 0, _contestsMaxPercentage * rangeFactor)));
    }

    #endregion
}
