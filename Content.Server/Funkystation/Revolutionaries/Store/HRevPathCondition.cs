using Content.Shared.Heretic;
using Content.Shared.Mind;
using Content.Shared.Revolutionary;
using Content.Shared.Store;
using static Content.Shared.Revolutionary.HRevComponent;

namespace Content.Server.Store.Conditions;

public sealed partial class HRevPathCondition : ListingCondition
{
    [DataField] public HashSet<string>? Whitelist;
    [DataField] public HashSet<string>? Blacklist;

    public static string GetFriendlyRevPathName(RevolutionaryPaths path)
    {
        return path switch
        {
            RevolutionaryPaths.NONE => "None",
            RevolutionaryPaths.VANGUARD => "Vanguard",
            RevolutionaryPaths.WOTP => "WOTP",
            RevolutionaryPaths.WARLORD => "Warlord",
            _ => "None",
        };
    }

    public override bool Condition(ListingConditionArgs args)
    {
        var ent = args.EntityManager;
        var minds = ent.System<SharedMindSystem>();

        if (!minds.TryGetMind(args.Buyer, out var mindId, out var mind))
            return false;

        if (!ent.TryGetComponent<HRevComponent>(args.Buyer, out var hRevComponent))
            return false;

        if (Whitelist != null)
        {
            foreach (var allowed in Whitelist)
                if (GetFriendlyRevPathName(hRevComponent.CurrentPath) == allowed)
                    return true;

            return false;
        }

        if (Blacklist != null)
        {
            foreach (var disallowed in Blacklist)
                if (GetFriendlyRevPathName(hRevComponent.CurrentPath) == disallowed)
                    return false;

            return true;
        }

        return true;
    }
}
