using Content.Server.Resist;
using Content.Shared.ActionBlocker;
using Content.Shared._EE.Carrying;
using Content.Shared._EE.Contests;
using Content.Shared.Movement.Events;

namespace Content.Server._EE.Carrying;

public sealed class CarryingSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly ContestsSystem _contests = default!;
    [Dependency] private readonly EscapeInventorySystem _escapeInventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BeingCarriedComponent, MoveInputEvent>(OnMoveInput); //NF
    }

    // Frontier/Imp edit.
    /// <summary>
    /// Try to escape via the escape inventory system.
    /// </summary>
    private void OnMoveInput(Entity<BeingCarriedComponent> ent, ref MoveInputEvent args)
    {
        if (!TryComp<CanEscapeInventoryComponent>(ent, out var escape) || !args.HasDirectionalMovement)
            return;

        // Check if the victim is in any way incapacitated, and if not make an escape attempt.
        // Escape time scales with the inverse of a mass contest. Being lighter makes escape harder.
        var carrier = ent.Comp.Carrier;
        if (_actionBlocker.CanInteract(ent, carrier))
        {
            var disadvantage = _contests.MassContest(carrier, ent.Owner, 2f);
            _escapeInventory.AttemptEscape(ent, carrier, escape, disadvantage);
        }
    }
}
