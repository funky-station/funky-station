using System.Linq;
using Content.Server.Antag.Components;
using Robust.Shared.Random;

namespace Content.Server.Antag;

public sealed class AntagMultipleRoleSpawnerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ILogManager _log = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AntagMultipleRoleSpawnerComponent, AntagSelectEntityEvent>(OnSelectEntity);

        _sawmill = _log.GetSawmill("antag_multiple_spawner");
    }

    private void OnSelectEntity(
        Entity<AntagMultipleRoleSpawnerComponent> ent,
        ref AntagSelectEntityEvent args)
    {
        // Combine preferred + fallback antag prototypes
        var antagRoles = args.Def.PrefRoles
            .Concat(args.Def.FallbackRoles)
            .ToList();

        if (antagRoles.Count != 1)
        {
            _sawmill.Fatal(
                $"Antag multiple role spawner had more than one antag ({antagRoles.Count})");
            return;
        }

        var antagRole = antagRoles[0];

        if (!ent.Comp.AntagRoleToPrototypes.TryGetValue(antagRole, out var entProtos))
            return; // No mapping → fall back to default behavior

        if (entProtos.Count == 0)
            return;

        args.Entity = Spawn(
            ent.Comp.PickAndTake
                ? _random.PickAndTake(entProtos)
                : _random.Pick(entProtos));
    }
}
