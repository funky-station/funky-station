using Content.Shared.Movement.Systems;

namespace Content.Shared.Traits.Assorted;

public sealed class MigraineSlowdownSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<MigraineSlowdownComponent, RefreshMovementSpeedModifiersEvent>(OnRefresh);
    }

    private void OnRefresh(Entity<MigraineSlowdownComponent> ent,
        ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(ent.Comp.Modifier);
    }
}
