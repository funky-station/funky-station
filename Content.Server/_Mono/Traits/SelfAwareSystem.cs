using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Traits;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Linq;
using Content.Shared.Damage.Prototypes;

namespace Content.Server.Traits;

public sealed class SelfAwareSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SelfAwareComponent, GetVerbsEvent<ExamineVerb>>(OnGetExamineVerbs);
    }

    private void OnGetExamineVerbs(EntityUid uid, SelfAwareComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (!TryComp<DamageableComponent>(uid, out var damage))
            return;

        var msg = new FormattedMessage();

        // Add total damage
        msg.AddMarkup($"Total Damage: {damage.TotalDamage}");

        // Add damage by group
        var damageSortedGroups = damage.DamagePerGroup
            .OrderByDescending(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Value);

        foreach (var (groupId, amount) in damageSortedGroups)
        {
            if (amount == 0)
                continue;

            var groupName = _prototype.Index<DamageGroupPrototype>(groupId).LocalizedName;
            msg.PushNewline();
            msg.AddMarkup($"[color=red]{groupName}: {amount}[/color]");

            // Show individual damage types in this group
            var group = _prototype.Index<DamageGroupPrototype>(groupId);
            foreach (var type in group.DamageTypes)
            {
                if (!damage.Damage.DamageDict.TryGetValue(type, out var typeAmount) || typeAmount <= 0)
                    continue;

                msg.PushNewline();
                msg.AddMarkup($" · {_prototype.Index<DamageTypePrototype>(type).LocalizedName}: {typeAmount}");
            }
        }

        _examine.AddDetailedExamineVerb(args, (Component)component, msg,
            Loc.GetString("self-aware-examinable-verb-text"),
            "/Textures/Interface/VerbIcons/smite.svg.192dpi.png",
            Loc.GetString("self-aware-examinable-verb-message")
        );
    }
}
