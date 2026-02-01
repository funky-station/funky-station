using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Server.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Verbs;
using Robust.Server.Player;

namespace Content.Server._Impstation.Borgs.FreeformLaws;

/// <summary>
/// Adds a verb to allow custom law entry on SiliconLawProviders. Should probably never be added to anything that isn't a lawboard.
/// </summary>
public sealed class FreeformLawEntrySystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly SiliconLawSystem _siliconLawSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FreeformLawEntryComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);
    }

    private void OnGetVerbs(Entity<FreeformLawEntryComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var lawBoundComp = EnsureComp<SiliconLawBoundComponent>(ent);
        var user = args.User;

        args.Verbs.Add(new InteractionVerb()
        {
            Text = Loc.GetString(ent.Comp.VerbName),
            Act = () =>
            {
                var ui = new SiliconLawEui(_siliconLawSystem, EntityManager, _adminManager, ent);
                if (!_playerManager.TryGetSessionByEntity(user, out var session))
                    return;
                _euiManager.OpenEui(ui, session);
                ui.UpdateLaws(lawBoundComp, ent);
            },
            Icon = ent.Comp.VerbIcon,
        });
    }
}
