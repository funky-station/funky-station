using Content.Client.Alerts;
using Content.Shared.StatusIcon.Components;
using Content.Shared.Strip.Components;

namespace Content.Client._Funkystation.Thief;

public sealed class ThiefSystem : EntitySystem
{

    /*public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThievingComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
        SubscribeLocalEvent<ThievingComponent, GetStatusIconsEvent>(GetIcon);
    }


    private void GetIcon(Entity<ThievingComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }*/
}
