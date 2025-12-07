// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 coolmankid12345 <55817627+coolmankid12345@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 coolmankid12345 <coolmankid12345@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Zergologist <114537969+Chedd-Error@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ferynn <117872973+ferynn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ferynn <witchy.girl.me@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.IdentityManagement;
using Content.Shared.Implants;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;
using Content.Shared.Revolutionary.Components;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Content.Shared.Antag;
using Content.Shared.Strip.Components;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.Roles;

namespace Content.Shared.Revolutionary;

public abstract class SharedRevolutionarySystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedStunSystem _sharedStun = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindShieldComponent, MapInitEvent>(MindShieldImplanted);
        SubscribeLocalEvent<RevolutionaryComponent, ComponentGetStateAttemptEvent>(OnRevCompGetStateAttempt);
        SubscribeLocalEvent<HeadRevolutionaryComponent, ComponentGetStateAttemptEvent>(OnRevCompGetStateAttempt);
        SubscribeLocalEvent<RevolutionaryLieutenantComponent, ComponentGetStateAttemptEvent>(OnLieuRevCompGetStateAttempt);
        SubscribeLocalEvent<RevolutionaryComponent, ComponentStartup>(DirtyRevComps);
        SubscribeLocalEvent<HeadRevolutionaryComponent, ComponentStartup>(DirtyRevComps);
        SubscribeLocalEvent<RevolutionaryLieutenantComponent, ComponentStartup>(DirtyRevComps);
        SubscribeLocalEvent<ShowAntagIconsComponent, ComponentStartup>(DirtyRevComps);
        
        SubscribeLocalEvent<HeadRevolutionaryComponent, AddImplantAttemptEvent>(OnHeadRevImplantAttempt);
        SubscribeLocalEvent<RevolutionaryComponent, AddImplantAttemptEvent>(OnRevImplantAttempt);
    }

    /// <summary>
    /// When the mindshield is implanted in the rev it will popup saying they were deconverted. In Head Revs it will remove the mindshield component.
    /// </summary>
    private void MindShieldImplanted(EntityUid uid, MindShieldComponent comp, MapInitEvent init)
    {
        if (HasComp<HeadRevolutionaryComponent>(uid))
        {
            comp.Broken = true; // Goobstation - Broken mindshield implant instead of break it
            Dirty(uid, comp);
            return;
        }

        if (HasComp<RevolutionaryComponent>(uid))
        {
            var stunTime = TimeSpan.FromSeconds(4);
            var name = Identity.Entity(uid, EntityManager);
            RemComp<RevolutionaryComponent>(uid);
            _sharedStun.TryParalyze(uid, stunTime, true);
            _popupSystem.PopupEntity(Loc.GetString("rev-break-control", ("name", name)), uid);
        }

        if (HasComp<CosmicCultComponent>(uid) || HasComp<CosmicCultLeadComponent>(uid))
        {
            comp.Broken = true; // Goobstation - Broken mindshield implant instead of break it
            Dirty(uid, comp);
            return;
        }
    }

    /// <summary>
    /// Determines if a HeadRev component should be sent to the client.
    /// </summary>
    private void OnRevCompGetStateAttempt(EntityUid uid, HeadRevolutionaryComponent comp, ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }

    /// <summary>
    /// Determines if a Rev component should be sent to the client.
    /// </summary>
    private void OnRevCompGetStateAttempt(EntityUid uid, RevolutionaryComponent comp, ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }
    
    /// <summary>
    /// Determines if a Lieutenant Rev component should be sent to the client.
    /// </summary>
    private void OnLieuRevCompGetStateAttempt(EntityUid uid, RevolutionaryLieutenantComponent comp, ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }

    /// <summary>
    /// The criteria that determine whether a Rev/HeadRev component should be sent to a client.
    /// </summary>
    /// <param name="player"> The Player the component will be sent to.</param>
    /// <returns></returns>
    private bool CanGetState(ICommonSession? player)
    {
        //Apparently this can be null in replays so I am just returning true.
        if (player?.AttachedEntity is not {} uid)
            return true;

        if (HasComp<RevolutionaryComponent>(uid) || HasComp<HeadRevolutionaryComponent>(uid) || HasComp<RevolutionaryLieutenantComponent>(uid))
            return true;

        return HasComp<ShowAntagIconsComponent>(uid);
    }

    /// <summary>
    /// Dirties all the Rev components so they are sent to clients.
    ///
    /// We need to do this because if a rev component was not earlier sent to a client and for example the client
    /// becomes a rev then we need to send all the components to it. To my knowledge there is no way to do this on a
    /// per client basis so we are just dirtying all the components.
    /// </summary>
    private void DirtyRevComps<T>(EntityUid someUid, T someComp, ComponentStartup ev)
    {
        var revComps = AllEntityQuery<RevolutionaryComponent>();
        while (revComps.MoveNext(out var uid, out var comp))
        {
            Dirty(uid, comp);
        }

        var headRevComps = AllEntityQuery<HeadRevolutionaryComponent>();
        while (headRevComps.MoveNext(out var uid, out var comp))
        {
            Dirty(uid, comp);
        }

        var lieuRevComps = AllEntityQuery<RevolutionaryLieutenantComponent>();
        while (lieuRevComps.MoveNext(out var uid, out var comp))
        {
            Dirty(uid, comp);
        }
    }

    private void OnHeadRevImplantAttempt(Entity<HeadRevolutionaryComponent> headRev, ref AddImplantAttemptEvent args)
    {
        if (TryCancelSelfMindshield(args.User, args.Target, args.Implant))
            args.Cancel();
    }

    private void OnRevImplantAttempt(Entity<RevolutionaryComponent> rev, ref AddImplantAttemptEvent args)
    {
        if (TryCancelSelfMindshield(args.User, args.Target, args.Implant))
            args.Cancel();
    }

    /// <summary>
    /// Prevents Revs from mindshielding themselves.
    /// </summary>
    /// <param name="user">Person using implanter</param>
    /// <param name="target">Target of implanter</param>
    /// <param name="implant">The implant</param>
    /// <returns></returns>
    private bool TryCancelSelfMindshield(EntityUid user, EntityUid target, EntityUid implant)
    {
        if (user != target)
            return false;

        if (!TryComp<TagComponent>(implant, out var tagComp))
            return false;

        if (!_tag.HasTag(tagComp, "MindShield"))
            return false;

        return true;
    }

    // GoobStation
    /// <summary>
    /// Change headrevs ability to convert people
    /// </summary>
    public void ToggleConvertAbility(Entity<HeadRevolutionaryComponent> headRev, bool toggle = true)
    {
        headRev.Comp.ConvertAbilityEnabled = toggle;
    }

    // Funky Station
    /// <summary>
    /// Change headrevs ability to give Rev Vision
    /// </summary>
    public void ToggleConvertGivesVision(Entity<HeadRevolutionaryComponent> headRev, bool toggle = true)
    {
        headRev.Comp.ConvertGivesRevVision = toggle;
    }
}
