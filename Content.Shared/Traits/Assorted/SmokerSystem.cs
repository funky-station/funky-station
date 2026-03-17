using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;

namespace Content.Shared.Traits.Assorted;

public sealed class SmokerSystem : EntitySystem
{

    [Dependency] private SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;



    public override void Initialize()
    {
        SubscribeLocalEvent<SmokerComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, SmokerComponent smoker, ComponentStartup args)
    {
        smoker.TimeSinceSmoking = 0f;
        smoker.WithdrawalStage = 0;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var smokerQuery = EntityQueryEnumerator<SmokerComponent>();
        while (smokerQuery.MoveNext(out var uid, out var smoker))
        {
            smoker.TimeSinceSmoking += frameTime;

            var adjustedInterval = smoker.SmokingInterval *(1 + smoker.WithdrawalStage);
            if (smoker.TimeSinceSmoking < adjustedInterval)
            {
                continue;
            }
            smoker.WithdrawalStage ++;
            //smoker.TimeSinceSmoking = 0f;
            _popup.PopupEntity("You feel like you should smoke...",uid,uid);


        }
    }
}






