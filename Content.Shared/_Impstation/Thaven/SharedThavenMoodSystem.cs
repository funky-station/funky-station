using Content.Shared.Emag.Systems;
using Content.Shared._Impstation.Thaven.Components;
using Content.Shared.Mindshield.Components;

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
        if (HasComp<MindShieldComponent>(uid))
            return;

        args.Handled = true;
    }
}
