using Content.Shared.Emag.Systems;
using Content.Shared._Impstation.Thaven.Components;

namespace Content.Shared._Impstation.Thaven;

public abstract class SharedThavenMoodSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThavenMoodsBoundComponent, GotEmaggedEvent>(OnEmagged);
    }
    protected virtual void OnEmagged(EntityUid uid, ThavenMoodsBoundComponent comp, ref GotEmaggedEvent args)
    {
        args.Handled = true;
    }
}
