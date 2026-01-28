using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Shared.Actions;
using Content.Shared._Funkystation.Genetics.Events;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Forensics;
using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationInkGlandsSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;
    [Dependency] private readonly ForensicsSystem _forensics = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private const string ReagentId = "Ink";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MutationInkGlandsComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MutationInkGlandsComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MutationInkGlandsComponent, InkSpurtActionEvent>(OnActionPerformed);
    }

    private void OnInit(EntityUid uid, MutationInkGlandsComponent comp, ref ComponentInit args)
    {
        _actions.AddAction(uid, ref comp.GrantedAction, comp.ActionId);
    }

    private void OnShutdown(Entity<MutationInkGlandsComponent> uid, ref ComponentShutdown args)
    {
        if (uid.Comp.GrantedAction is { Valid: true } action)
            _actions.RemoveAction(action);
    }

    private void OnActionPerformed(EntityUid uid, MutationInkGlandsComponent comp, ref InkSpurtActionEvent args)
    {
        if (args.Handled || args.Performer != uid)
            return;

        args.Handled = true;

        var amount = FixedPoint2.New(comp.Amount);

        var solution = new Solution();
        solution.AddReagent(ReagentId, amount);

        if (!TryComp<TransformComponent>(uid, out var xform))
            return;

        // Spill ink one tile behind facing direction to prevent slipping yourself
        var behindCoords = xform.Coordinates.Offset(xform.LocalRotation.GetCardinalDir().GetOpposite().ToVec());

        if (_puddle.TrySpillAt(behindCoords, solution, out var puddleUid))
        {
            _forensics.TransferDna(puddleUid, uid, false);
        }
        else
        {
            // Fallback to feet if blocked
            _puddle.TrySpillAt(xform.Coordinates, solution, out _);
        }
        _audio.PlayPredicted(comp.SpillSound, uid, uid);
    }
}
