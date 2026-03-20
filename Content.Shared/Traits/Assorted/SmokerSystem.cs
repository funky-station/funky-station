using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Random.Rules;
using Robust.Shared.Containers;

namespace Content.Shared.Traits.Assorted;

public sealed class SmokerSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;


    private EntityUid _solutionUid = default;

    public override void Initialize()
    {
        SubscribeLocalEvent<SmokerComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, SmokerComponent smoker, ComponentStartup args)
    {
        smoker.TimeSinceSmoking = 0f;
        smoker.WithdrawalStage = 0;
        var currentNicotine = 0f;
        EnsureComp<ContainerManagerComponent>(uid);
        if (!TryComp<ContainerManagerComponent>(uid, out var containerManager))
        {
            return;
        }
        foreach (var containers in containerManager!.Containers)
        {
            if (containers.Key != "solution@chemicals")
                continue;
            foreach (var i in containers.Value.ContainedEntities)
            {
                _solutionUid = i;
                _popup.PopupEntity(i.ToString(), uid, uid);
            }
        }



    }

    public override void Update(float frameTime)
    {
        // it's pretty much same format as the NarcolepsySystem
        //tries to increment the time spent without smoking and progresses through the stages as the user ignore
        //smoking (to be implemented)
        base.Update(frameTime);

        var smokerQuery = EntityQueryEnumerator<SmokerComponent>();
        while (smokerQuery.MoveNext(out var uid, out var smoker))
        {

            //_popup.PopupEntity(_solutionUid.ToString(),uid,uid);
            smoker.TimeSinceSmoking += frameTime;
            var adjustedInterval = smoker.SmokingInterval * (1 + smoker.WithdrawalStage);
            if (smoker.TimeSinceSmoking >= adjustedInterval)
            {
                smoker.WithdrawalStage++;
                //_popup.PopupEntity("You feel like you should smoke...", uid, uid);
            }

            if (!TryComp<SolutionComponent>(_solutionUid, out var solutions))
            {
                _popup.PopupEntity("No solutionsComp with "+_solutionUid.ToString(), uid,uid);
            }


            else
            {
               CheckNicotineLevel(uid,_solutionUid,smoker);
            }
        }
    }

    private bool CheckNicotineLevel(EntityUid uid, EntityUid solutionUid, SmokerComponent smoker)
    {
        var currentNicotine = smoker.CurrentNicotineLevel;
        if (!TryComp<SolutionComponent>(solutionUid, out var solution))
            return false;
        foreach (var name in solution.Solution.Contents)
        {
            if (name.Reagent.Prototype != "Nicotine")
                continue;
            if (name.Quantity <= currentNicotine || name.Quantity - currentNicotine == 0.45)
            {
                smoker.CurrentNicotineLevel = name.Quantity;
                return false;
            }

            //_popup.PopupEntity(name.Reagent.Prototype +": "+ name.Quantity.ToString()+ " > " + smoker
            //.CurrentNicotineLevel, uid, uid);
            _popup.PopupEntity("You are cool...", uid, uid);
            smoker.CurrentNicotineLevel = name.Quantity;
            return true;

        }
        return false;
    }
}







