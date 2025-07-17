// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 duston <66768086+dch-GH@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 willowzeta <willowzeta632146@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Heretic.EntitySystems;
using Content.Shared.Dataset;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Text;
using Content.Shared.Body.Part;
using Content.Shared.Extensions;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualKnowledgeBehavior : RitualCustomBehavior
{
    // made static so that it doesn't regenerate itself each time
    private static Dictionary<ProtoId<TagPrototype>, bool> _satisfiedTags = new();
    private List<EntityUid> _toDelete = new();

    private IPrototypeManager _prot = default!;
    private IRobustRandom _rand = default!;
    private EntityLookupSystem _lookup = default!;
    private HereticSystem _heretic = default!;

    [ValidatePrototypeId<DatasetPrototype>]
    public const string EligibleTagsDataset = "EligibleTags";

    // this is basically a ripoff from hereticritualsystem
    public override bool Execute(RitualData args, out string? outstr)
    {
        _prot = IoCManager.Resolve<IPrototypeManager>();
        _rand = IoCManager.Resolve<IRobustRandom>();
        _lookup = args.EntityManager.System<EntityLookupSystem>();
        _heretic = args.EntityManager.System<HereticSystem>();
        var entityMan = args.EntityManager;

        outstr = null;

        // generate new set of tags
        if (_satisfiedTags.Count == 0)
            for (int i = 0; i < 4; i++)
                _satisfiedTags.Add(_rand.Pick(_prot.Index<DatasetPrototype>(EligibleTagsDataset).Values), false);

        var lookup = _lookup.GetEntitiesInRange(args.Platform, .75f);
        var missingList = new List<string>();

        foreach (var thing in lookup)
        {
            // Just in case.
            if (thing == args.Performer)
                continue;

            // Don't use the performer's clothes, backpack contents, body parts, or organs...
            if (entityMan.IsChildOf(args.Performer, thing))
                continue;

            foreach (var neededTag in _satisfiedTags)
            {
                if (!entityMan.TryGetComponent<TagComponent>(thing, out var thingTags))
                    continue;

                var tagsOfThing = thingTags.Tags;
                if (!tagsOfThing.Contains(neededTag.Key))
                    continue;

                _satisfiedTags[neededTag.Key] = true;
                _toDelete.Add(thing);
            }
        }

        foreach (var required in _satisfiedTags)
            if (required.Value == false)
                missingList.Add(required.Key);

        if (missingList.Count > 0)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < missingList.Count; i++)
            {
                if (i != missingList.Count - 1)
                    sb.Append($"{missingList[i]}, ");
                else
                    sb.Append(missingList[i]);
            }

            outstr = Loc.GetString("heretic-ritual-fail-items", ("itemlist", sb.ToString()));
            return false;
        }

        return true;
    }

    public override void Finalize(RitualData args)
    {
        // delete all and reset
        foreach (var ent in _toDelete)
            args.EntityManager.QueueDeleteEntity(ent);

        _toDelete = new();

        if (args.EntityManager.TryGetComponent<HereticComponent>(args.Performer, out var hereticComp))
            _heretic.UpdateKnowledge(args.Performer, hereticComp, 2); // funkystation: changed value to encourage sacs

        // reset tags
        _satisfiedTags = new();
    }
}
