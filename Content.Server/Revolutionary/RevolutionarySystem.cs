using Content.Server.Actions;
using Content.Shared.Revolutionary;
using Content.Shared.Revolutionary.Components;


namespace Content.Server.Revolutionary;

public sealed class RevolutionarySystem : SharedRevolutionarySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeadRevolutionaryComponent, ComponentInit>(OnStartHeadRev);
    }

    /// <summary>
    /// Add the starting ability(s) to the Head Rev.
    /// </summary>
    private void OnStartHeadRev(Entity<HeadRevolutionaryComponent> uid, ref ComponentInit args)
    {
        foreach (var actionId in uid.Comp.BaseHeadRevActions)
        {
            var actionEnt = _actions.AddAction(uid, actionId);
        }
    }
}
