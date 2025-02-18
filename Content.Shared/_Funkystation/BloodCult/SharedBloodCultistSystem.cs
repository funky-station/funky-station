using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Content.Shared.BloodCult.Components;
using Content.Shared.Antag;

namespace Content.Shared.BloodCult;

public abstract class SharedBloodCultistSystem : EntitySystem
{
	public override void Initialize()
    {
        base.Initialize();

		SubscribeLocalEvent<BloodCultistComponent, ComponentGetStateAttemptEvent>(OnRevCompGetStateAttempt);

		SubscribeLocalEvent<BloodCultistComponent, ComponentStartup>(DirtyRevComps);
	}

	/// <summary>
    /// Determines if a BloodCultist component should be sent to the client.
    /// </summary>
    private void OnRevCompGetStateAttempt(EntityUid uid, BloodCultistComponent comp, ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }
	/// <summary>
    /// The criteria that determine whether a Rev/HeadRev component should be sent to a client.
    /// </summary>
    /// <param name="player"> The Player the component will be sent to.</param>
    /// <returns></returns>
    private bool CanGetState(ICommonSession? player)
    {
        //Apparently this can be null in replays so I am just returning true.
        if (player?.AttachedEntity is not {} uid)
            return true;

        if (HasComp<BloodCultistComponent>(uid))
            return true;

        return HasComp<ShowAntagIconsComponent>(uid);
    }

    private void DirtyRevComps<T>(EntityUid someUid, T someComp, ComponentStartup ev)
    {
        var cultComps = AllEntityQuery<BloodCultistComponent>();
        while (cultComps.MoveNext(out var uid, out var comp))
        {
            Dirty(uid, comp);
        }
    }
}