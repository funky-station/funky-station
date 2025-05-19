using Robust.Shared.GameObjects;
using Content.Server.Mind;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;

namespace Content.Server.BloodCult.EntitySystems;

public sealed partial class BloodCultConstructSystem : EntitySystem
{
	[Dependency] private readonly MindSystem _mind = default!;

	public override void Initialize()
	{
		base.Initialize();

		//SubscribeLocalEvent<SoulStoneComponent, AfterInteractEvent>(OnTryApplySoulStone);
	}

	public void TryApplySoulStone(Entity<SoulStoneComponent> ent, ref AfterInteractEvent args)
    {
		if (args.Target == null || !HasComp<BloodCultConstructShellComponent>(args.Target))
			return;
		var coordinates = Transform((EntityUid)args.Target).Coordinates;
		EntityUid? mindId = CompOrNull<MindContainerComponent>(ent)?.Mind;
		MindComponent? mindComp = CompOrNull<MindComponent>(mindId);
		if (mindId != null && mindComp != null)
		{
			QueueDel((EntityUid)args.Target);
			var construct = Spawn("MobBloodCultJuggernaut", coordinates);
			_mind.TransferTo((EntityUid)mindId, construct, mind:mindComp);
			QueueDel(ent);
		}
		args.Handled = true;
	}
}
