
using Content.Shared.Chemistry.Components;

using Content.Server.Chat.Systems;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Content.Shared.Traits.Assorted;

using Robust.Shared.Containers;

namespace Content.Server.Traits.Assorted;

public sealed class SmokerSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;


    private EntityUid _solutionUid = default;

    public override void Initialize()
    {
        SubscribeLocalEvent<SmokerComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, SmokerComponent smoker, ComponentStartup args)
    {
        smoker.TimeSinceSmoking = 0f;
        smoker.WithdrawalStage = 0;
        smoker.CurrentNicotineLevel = 0f;
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
            smoker.TimeSinceSmoking += frameTime;
            if(CheckNicotineLevel(uid,_solutionUid,smoker))
                continue;
            SetWithdrawalStage(uid,smoker);
        }
    }

    private bool CheckNicotineLevel(EntityUid uid, EntityUid solutionUid, SmokerComponent smoker)
    {
        // Check the current nicotine levels inside the chemicals container and if it detects that the nicotine is
        // rising it resets the
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
            //_popup.PopupEntity("You are cool...", uid, uid);
            smoker.CurrentNicotineLevel = name.Quantity;
            smoker.TimeSinceSmoking = 0;
            smoker.CurrentSmokingInterval = 0;
            smoker.WithdrawalStage = 0;
            return true;
        }
        return false;
    }

    private void SetWithdrawalStage(EntityUid uid, SmokerComponent smoker)
    {
        if(smoker.WithdrawalStage < 0)
            smoker.WithdrawalStage = 0;

        if (_mobStateSystem.IsIncapacitated(uid))
            return;

        var adjustedInterval = smoker.SmokingInterval + smoker.CurrentSmokingInterval;
        if (!(smoker.TimeSinceSmoking >= adjustedInterval))
            return;
        if(smoker.WithdrawalStage < 8)
            smoker.WithdrawalStage++;
        smoker.CurrentSmokingInterval += smoker.SmokingInterval/(1+smoker.WithdrawalStage);
        switch (smoker.WithdrawalStage)
        {
            case 0:
                _popup.PopupEntity("All's fine in the world!", uid, uid);
                break;
            case 1:
                _popup.PopupEntity("A cigarette would be nice..",uid,uid);
                break;
            case 2:
                _popup.PopupEntity("You feel the need for a smoke break.", uid, uid,PopupType.Medium);
                break;
            case 3:
                _popup.PopupEntity("You must get some nicotine!", uid, uid,PopupType.MediumCaution);
                if (TryComp<SpeechComponent>(uid,out _))
                    _chatSystem.TryEmoteWithChat(uid,"Sigh");
                break;
            case 4:
                _popup.PopupEntity("You REALLY need to smoke!", uid, uid,PopupType.MediumCaution);
                break;
            default:
                _popup.PopupEntity("YOUR WHOLE BODY CRAVES NICOTINE!", uid, uid, PopupType.MediumCaution);
                if (TryComp<SpeechComponent>(uid, out _)&& smoker.WithdrawalStage % 2 == 0)
                    _chatSystem.TryEmoteWithChat(uid,"Scream");


                break;
        }
    }
}







