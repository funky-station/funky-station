using Content.Shared.Mind;
using Content.Shared.Revolutionary;
using Content.Shared.Store;
using static Content.Shared.Revolutionary.HeadRevolutionaryPathComponent;

namespace Content.Server.Store.Conditions;

public sealed partial class HeadRevolutionaryPathCondition : ListingCondition
{
    [DataField] public HashSet<string>? Whitelist;
    [DataField] public HashSet<string>? Blacklist;
    [DataField] public bool? AllowOnNone;

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

        if (!minds.TryGetMind(args.Buyer, out var _, out var _))
            return false;

        if (!ent.TryGetComponent<HeadRevolutionaryPathComponent>(args.Buyer, out var headRevolutionaryPathComponent))
            return false;

        if (Whitelist != null)
        {
            foreach (var allowed in Whitelist)
                if (GetFriendlyRevPathName(headRevolutionaryPathComponent.CurrentPath) == allowed)
                    return true;

            return false;
        }

        if (Blacklist != null)
        {
            foreach (var disallowed in Blacklist)
                if (GetFriendlyRevPathName(headRevolutionaryPathComponent.CurrentPath) == disallowed)
                    return false;

            return true;
        }

        return true;
    }
}
