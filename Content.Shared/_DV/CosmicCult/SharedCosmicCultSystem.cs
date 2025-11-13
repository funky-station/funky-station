// SPDX-FileCopyrightText: 2025 No Elka <125199100+NoElkaTheGod@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 No Elka <no.elka.the.god@gmail.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 ferynn <117872973+ferynn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ferynn <witchy.girl.me@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Antag;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Verbs;
using Content.Shared._DV.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Content.Shared.Mindshield.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._DV.CosmicCult;

public abstract class SharedCosmicCultSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicCultComponent, ComponentGetStateAttemptEvent>(OnCosmicCultCompGetStateAttempt);
        SubscribeLocalEvent<CosmicCultLeadComponent, ComponentGetStateAttemptEvent>(OnCosmicCultCompGetStateAttempt);
        SubscribeLocalEvent<CosmicCultComponent, ComponentStartup>(OnCosmicCultConversion);
        SubscribeLocalEvent<CosmicCultLeadComponent, ComponentStartup>(OnCosmicCultConversion);
        SubscribeLocalEvent<CosmicCultComponent, ComponentRemove>(OnCosmicCultDeconversion);
        SubscribeLocalEvent<CosmicCultLeadComponent, ComponentRemove>(OnCosmicCultDeconversion);

        SubscribeLocalEvent<CosmicTransmutableComponent, GetVerbsEvent<ExamineVerb>>(OnTransmutableExamined);
        SubscribeLocalEvent<CosmicCultExamineComponent, ExaminedEvent>(OnCosmicCultExamined);
    }

    private void OnTransmutableExamined(Entity<CosmicTransmutableComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (ent.Comp.TransmutesTo is not { } transmutesTo || ent.Comp.RequiredGlyphType is not { } requiredGlyphType)
            return;
        if (_proto.TryIndex(transmutesTo, out var result) == false || _proto.TryIndex(requiredGlyphType, out var glyph) == false)
            return;
        if (!EntityIsCultist(args.User)) //non-cultists don't need to know this anyway
            return;
        var text = Loc.GetString("cosmic-examine-transmutable", ("result", result.Name), ("glyph", glyph.Name));
        var msg = new FormattedMessage();
        msg.AddMarkupOrThrow(text);
        _examine.AddHoverExamineVerb(args,
            ent.Comp,
            Loc.GetString("cosmic-examine-transmutable-verb-text"),
            msg.ToMarkup(),
            "/Textures/_DV/CosmicCult/Interface/transmute_inspect.png");
    }

    private void OnCosmicCultExamined(Entity<CosmicCultExamineComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(EntitySeesCult(args.Examiner) ? ent.Comp.CultistText : ent.Comp.OthersText));
    }

    public bool EntityIsCultist(EntityUid user)
    {
        if (!_mind.TryGetMind(user, out var mind, out _))
            return false;

        return HasComp<CosmicCultComponent>(user) || _role.MindHasRole<CosmicCultRoleComponent>(mind);
    }

    public bool EntitySeesCult(EntityUid user)
    {
        return EntityIsCultist(user) || HasComp<GhostComponent>(user);
    }

    /// <summary>
    /// Determines if a Cosmic Cult Lead component should be sent to the client.
    /// </summary>
    private void OnCosmicCultCompGetStateAttempt(EntityUid uid, CosmicCultLeadComponent comp, ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }

    /// <summary>
    /// Determines if a Cosmic Cultist component should be sent to the client.
    /// </summary>
    private void OnCosmicCultCompGetStateAttempt(EntityUid uid, CosmicCultComponent comp, ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }

    /// <summary>
    /// The criteria that determine whether a Cult Member component should be sent to a client.
    /// </summary>
    /// <param name="player">The Player the component will be sent to.</param>
    private bool CanGetState(ICommonSession? player)
    {
        //Apparently this can be null in replays so I am just returning true.
        if (player?.AttachedEntity is not { } uid)
            return true;

        if (EntitySeesCult(uid) || HasComp<CosmicCultLeadComponent>(uid))
            return true;

        return HasComp<ShowAntagIconsComponent>(uid);
    }

    private void OnCosmicCultConversion<T>(EntityUid someUid, T someComp, ComponentStartup ev)
    {
        ///DirtyCosmicCultComps
        /// <summary>
        /// Dirties all the Cult components so they are sent to clients.
        ///
        /// We need to do this because if a Cult component was not earlier sent to a client and for example the client
        /// becomes a Cult then we need to send all the components to it. To my knowledge there is no way to do this on a
        /// per client basis so we are just dirtying all the components.
        /// </summary>
        var cosmicCultComps = AllEntityQuery<CosmicCultComponent>();
        while (cosmicCultComps.MoveNext(out var uid, out var comp))
        {
            Dirty(uid, comp);
        }

        var cosmicCultLeadComps = AllEntityQuery<CosmicCultLeadComponent>();
        while (cosmicCultLeadComps.MoveNext(out var uid, out var comp))
        {
            Dirty(uid, comp);
        }
        //If the cultist has a mindshield, break it //Funky
        if (HasComp<MindShieldComponent>(someUid))
        {
            if (TryComp<MindShieldComponent>(someUid, out var mindShieldComp))
            {
                mindShieldComp.Broken = true;
                Dirty(someUid, mindShieldComp);
            }
        }
    }
    private void OnCosmicCultDeconversion<T>(EntityUid uid, T someComp, ComponentRemove ev)
    {
        if (HasComp<MindShieldComponent>(uid))
        {
            if (TryComp<MindShieldComponent>(uid, out var mindShieldComp))
            {
                mindShieldComp.Broken = false;
                Dirty(uid, mindShieldComp);
            }
        }
    }
}
