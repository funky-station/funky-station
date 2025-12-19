// SPDX-FileCopyrightText: 2022 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 drakewill-CRL <46307022+drakewill-CRL@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Tom Leys <tom@crump-leys.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 coolmankid12345 <55817627+coolmankid12345@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 coolmankid12345 <coolmankid12345@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Ciac32 <aknoxlor@gmail.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 Kira Bridgeton <161087999+Verbalase@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PoTeletubby <108604614+PoTeletubby@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Rainfey <rainfey0+github@gmail.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 crazybrain23 <44417085+crazybrain23@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2024 no <165581243+pissdemon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server._DV.CosmicCult.Components;
using Content.Server.Administration.Commands;
using Content.Server.Antag;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Zombies;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Humanoid;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Administration.Systems;

public sealed partial class AdminVerbSystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly ZombieSystem _zombie = default!;
    [Dependency] private readonly ZombieTumorOrganSystem _zombieTumor = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    [ValidatePrototypeId<EntityPrototype>]
    private const string DefaultTraitorRule = "Traitor";

    [ValidatePrototypeId<EntityPrototype>]
    private const string DefaultInitialInfectedRule = "Zombie";

    [ValidatePrototypeId<EntityPrototype>]
    private const string DefaultNukeOpRule = "LoneOpsSpawn";

    [ValidatePrototypeId<EntityPrototype>]
    private const string DefaultRevsRule = "Revolutionary";

    [ValidatePrototypeId<EntityPrototype>]
    private const string DefaultThiefRule = "Thief";

    [ValidatePrototypeId<StartingGearPrototype>]
    private const string PirateGearId = "PirateGear";

    private readonly EntProtoId _paradoxCloneRuleId = "ParadoxCloneSpawn";

    // All antag verbs have names so invokeverb works.
    private void AddAntagVerbs(GetVerbsEvent<Verb> args)
    {
        if (!TryComp<ActorComponent>(args.User, out var actor))
            return;

        var player = actor.PlayerSession;

        if (!_adminManager.HasAdminFlag(player, AdminFlags.Fun))
            return;

        if (!HasComp<MindContainerComponent>(args.Target) || !TryComp<ActorComponent>(args.Target, out var targetActor))
            return;

        var targetPlayer = targetActor.PlayerSession;

        Verb traitor = new()
        {
            Text = Loc.GetString("admin-verb-text-make-traitor"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Structures/Wallmounts/posters.rsi"), "poster5_contraband"),
            Act = () =>
            {
                _antag.ForceMakeAntag<TraitorRuleComponent>(targetPlayer, DefaultTraitorRule);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-traitor"),
        };
        args.Verbs.Add(traitor);

        Verb initialInfected = new()
        {
            Text = Loc.GetString("admin-verb-text-make-initial-infected"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "InitialInfected"),
            Act = () =>
            {
                _antag.ForceMakeAntag<ZombieRuleComponent>(targetPlayer, DefaultInitialInfectedRule);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-initial-infected"),
        };
        args.Verbs.Add(initialInfected);

        Verb zombie = new()
        {
            Text = Loc.GetString("admin-verb-text-make-zombie"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/Actions/zombie-turn.png")),
            Act = () =>
            {
                // Give them a tumor infection instead of immediately zombifying
                // The tumor will progress normally and eventually zombify them
                _zombieTumor.InfectEntity(args.Target);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-zombie"),
        };
        args.Verbs.Add(zombie);


        Verb nukeOp = new()
        {
            Text = Loc.GetString("admin-verb-text-make-nuclear-operative"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Structures/Wallmounts/signs.rsi"), "radiation"),
            Act = () =>
            {
                _antag.ForceMakeAntag<NukeopsRuleComponent>(targetPlayer, DefaultNukeOpRule);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-nuclear-operative"),
        };
        args.Verbs.Add(nukeOp);

        Verb pirate = new()
        {
            Text = Loc.GetString("admin-verb-text-make-pirate"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Clothing/Head/Hats/pirate.rsi"), "icon"),
            Act = () =>
            {
                // pirates just get an outfit because they don't really have logic associated with them
                SetOutfitCommand.SetOutfit(args.Target, PirateGearId, EntityManager);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-pirate"),
        };
        args.Verbs.Add(pirate);

        Verb headRev = new()
        {
            Text = Loc.GetString("admin-verb-text-make-head-rev"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "HeadRevolutionary"),
            Act = () =>
            {
                _antag.ForceMakeAntag<RevolutionaryRuleComponent>(targetPlayer, DefaultRevsRule);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-head-rev"),
        };
        args.Verbs.Add(headRev);

        Verb thief = new()
        {
            Text = Loc.GetString("admin-verb-text-make-thief"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Clothing/Hands/Gloves/Color/black.rsi"), "icon"),
            Act = () =>
            {
                _antag.ForceMakeAntag<ThiefRuleComponent>(targetPlayer, DefaultThiefRule);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-thief"),
        };
        args.Verbs.Add(thief);

        // Goobstation - changelings
        Verb ling = new()
        {
            Text = Loc.GetString("admin-verb-text-make-changeling"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Changeling/changeling_abilities.rsi"), "transform"),
            Act = () =>
            {
                if (!HasComp<SiliconComponent>(args.Target))
                    _antag.ForceMakeAntag<ChangelingRuleComponent>(targetPlayer, "Changeling");
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-changeling"),
        };
        if (!HasComp<SiliconComponent>(args.Target))
            args.Verbs.Add(ling);

        // goobstation - heretics
        Verb heretic = new()
        {
            Text = Loc.GetString("admin-verb-make-heretic"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Heretic/Blades/blade_blade.rsi"), "icon"),
            Act = () =>
            {
                _antag.ForceMakeAntag<HereticRuleComponent>(targetPlayer, "Heretic");
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-heretic"),
        };
        args.Verbs.Add(heretic);

        // Goobstation - Blob
        Verb blobAntag = new()
        {
            Text = Loc.GetString("admin-verb-text-make-blob"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/_Goobstation/Blob/Actions/blob.rsi"), "blobFactory"),
            Act = () =>
            {
                EnsureComp<Shared._Goobstation.Blob.Components.BlobCarrierComponent>(args.Target).HasMind = HasComp<ActorComponent>(args.Target);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-text-make-blob"),
	    };
        args.Verbs.Add(blobAntag);

        // IMPSTATION - COSMIC CULT
        //note - the UI for the monument currently doesn't properly account for cultists added like this until it gets sent a new state - ruddygreat
        //todo figure out how to fix that
        var cosmicCultName = Loc.GetString("admin-verb-text-make-cosmiccultist");
        Verb cosmiccult = new()
        {
            Text = cosmicCultName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/_DV/CosmicCult/Icons/antag_icons.rsi"), "CosmicCult"),
            Act = () =>
            {
                _antag.ForceMakeAntag<CosmicCultRuleComponent>(targetPlayer, "CosmicCult");
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", cosmicCultName, Loc.GetString("admin-verb-make-cosmiccultist")),
        };
        args.Verbs.Add(cosmiccult);

        // Funkystation - blood cult
        Verb cultAntag = new()
        {
            Text = "Blood Cult",
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/_Funkystation/Structures/BloodCult/bloodrune.rsi"), "barrier"),
            Act = () =>
            {
                _antag.ForceMakeAntag<BloodCultRuleComponent>(targetPlayer, "BloodCult");
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-cultist")
        };
        args.Verbs.Add(cultAntag);

        // Funkystation - Malfunctioning AI
        Verb malfAi = new()
        {
            Text = "Malfunctioning AI",
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "StationAi"),
            Act = () =>
            {
                _antag.ForceMakeAntag<MalfAiRuleComponent>(targetPlayer, "MalfAi");
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-malfai")
        };
        args.Verbs.Add(malfAi);

        var paradoxCloneName = Loc.GetString("admin-verb-text-make-paradox-clone");
        Verb paradox = new()
        {
            Text = paradoxCloneName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "ParadoxClone"),
            Act = () =>
            {
                var ruleEnt = _gameTicker.AddGameRule(_paradoxCloneRuleId);

                if (!TryComp<ParadoxCloneRuleComponent>(ruleEnt, out var paradoxCloneRuleComp))
                    return;

                paradoxCloneRuleComp.OriginalBody = args.Target; // override the target player

                _gameTicker.StartGameRule(ruleEnt);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", paradoxCloneName, Loc.GetString("admin-verb-make-paradox-clone")),
        };

        if (HasComp<HumanoidAppearanceComponent>(args.Target)) // only humanoids can be cloned
            args.Verbs.Add(paradox);
    }
}
