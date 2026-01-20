using System.Linq;
using Content.Server.Station.Events;
using Content.Server.Storage.EntitySystems;
using Content.Shared._Funkystation.Traits.Unions;
using Content.Shared.GameTicking;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Funkystation.Unions;

public sealed class UnionSelectorSystem : EntitySystem
{
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly StorageSystem _storageSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    
    private ISawmill _sawmill = default!;

    private static readonly ProtoId<UnionRoleItemSetPrototype> UnionLeaderItemSetProto = "unionItemSet";
    private static readonly ProtoId<UnionElgibleDepartmentPrototype> UnionEligibleDepartmentSetProto = "unionEligibleDepartments";

    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(PlayerSpawnedEvent);
        SubscribeLocalEvent<StationPostInitEvent>(StationCreatedEvent);
        
        _sawmill = _logManager.GetSawmill("unions");
    }

    private void StationCreatedEvent(ref StationPostInitEvent ev)
    {
        EnsureComp<StationUnionsComponent>(ev.Station.Owner);
    }

    private void PlayerSpawnedEvent(PlayerSpawnCompleteEvent ev)
    {
        if (TryComp<UnionLeaderComponent>(ev.Mob, out _))
        {
            CreateUnion(ev, true);
            AddItemsToUnionRole(ev.Mob, "UnionLeader");

            return;
        }

        if (TryComp<UnionMemberComponent>(ev.Mob, out _))
        {
            CreateUnion(ev, true);
            AddItemsToUnionRole(ev.Mob, "UnionMember");
        }
    }

    private string? GetDepartmentFromJob(string jobId)
    {
        var allDepartments = _prototypeManager.EnumeratePrototypes<DepartmentPrototype>();

        return (from department in allDepartments
            from _ in 
            department.Roles.Where(departmentRole => departmentRole == jobId)
            select department.ID).FirstOrDefault();
    }

    private void CreateUnion(PlayerSpawnCompleteEvent ev, bool eligibleForUnionLeader)
    {
        // no cc union
        if (!IsJobElgibleForUnion(ev.JobId!))
        {
            RemComp<UnionLeaderComponent>(ev.Mob);
            RemComp<UnionMemberComponent>(ev.Mob);
            return;
        }

        var departmentId = GetDepartmentFromJob(ev.JobId!);
        if (departmentId == null)
            return;
        
        var unionsComp = GetStationUnionsComponent();
        foreach (var union in unionsComp.Unions.Where(union => union.Departments.Contains(departmentId)))
        {
            union.Members.Add(ev.Mob, eligibleForUnionLeader);

            return;
        }
        
        var newUnion = new StationUnion
        {
            Departments = [ departmentId ],
            Members = new Dictionary<EntityUid, bool>
            {
                { ev.Mob, eligibleForUnionLeader }
            },
            OnStrike = false,
        };
        
        unionsComp.Unions.Add(newUnion);
        newUnion.Name = GenerateUnionName(departmentId, null);
    }

    private string GenerateUnionName(string department, List<string>? existingDepartments)
    {
        // prefixes are hardcoded but will change later
        if (string.Equals(department, "Security", StringComparison.OrdinalIgnoreCase))
            return $"SECU-{_random.Next(0, 100):D2}";

        var initials = char.ToUpper(department.Trim()[0]).ToString();

        if (existingDepartments != null)
        {
            // could use linq as rider keeps yelling but fuck that
            foreach (var dept in existingDepartments)
            {
                if (!string.IsNullOrWhiteSpace(dept))
                {
                    initials += char.ToUpper(dept.Trim()[0]);
                }
            }
        }

        return $"NTWU-{initials} {_random.Next(0, 100):D2}";
    }

    private bool IsJobElgibleForUnion(string jobId)
    {
        var allDepartments = _prototypeManager.EnumeratePrototypes<DepartmentPrototype>();
        
        _prototypeManager.TryIndex(UnionEligibleDepartmentSetProto, out var eligibleDepartmentPrototype);
        if (eligibleDepartmentPrototype is null)
            return false;
        
        foreach (var department in allDepartments)
        {
            foreach (var _ in department.Roles.Where(departmentRole => departmentRole == jobId))
            {
                if (eligibleDepartmentPrototype.EligibleDepartments.Contains(department.ID))
                    return true;
            }
        }

        return false;
    }
    
    // should only ever be a single one
    private StationUnionsComponent GetStationUnionsComponent()
    {
        var query = EntityQuery<StationUnionsComponent>();
        var stationUnionsComponents = query as StationUnionsComponent[] ?? query.ToArray();
        if (stationUnionsComponents.Length != 1)
        {
            _sawmill.Error("GetStationUnionComponent query returned a value greater than 1, or less than 1.");
        }

        return stationUnionsComponents[0];
    }

    private void AddItemsToUnionRole(EntityUid playerMobUid, string role)
    {
        switch (role)
        {
            case "UnionLeader":
            {
                _prototypeManager.TryIndex(UnionLeaderItemSetProto, out var itemSet);

                foreach (var itemSetId in itemSet!.Items)
                {
                    AddItemsToStorage(playerMobUid, itemSetId);
                }

                break;
            }
            case "UnionMember":
            {
                //TODO: need to setup other stuff surrounding membership
                break;
            }
        }
    }

    private void AddItemsToStorage(EntityUid playerMobUid, EntProtoId item)
    {
        Spawn(item);
    }
}