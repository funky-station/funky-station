using Content.Shared.Chemistry.Components;
using Content.Server.Chat.Systems;
using Content.Shared.Bed.Sleep;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Content.Shared.Speech.Muting;
using Content.Shared.Traits.Assorted;

using Robust.Shared.Containers;

namespace Content.Server.Traits.Assorted;

public sealed class SmokerSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

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
                smoker.ChemicalsContainer = i;
            }
        }



    }
    /// <summary>
    /// Updates the SmokerSystem.
    /// </summary>
    /// <param name="frameTime"></param>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var smokerQuery = EntityQueryEnumerator<SmokerComponent>();
        while (smokerQuery.MoveNext(out var uid, out var smoker))
        {
            if (_mobStateSystem.IsIncapacitated(uid) || TryComp<SleepingComponent>(uid,out _))
                continue;
            smoker.TimeSinceSmoking += frameTime;
            if(CheckNicotineLevel(uid,smoker.ChemicalsContainer,smoker))
                continue;
            SetWithdrawalStage(uid,smoker);
        }
    }
    /// <summary>
    /// Checks the current nicotine levels inside solution@chemicals. If the levels are rising (user is smoking)
    /// resets the WithdrawalStage and TimeSinceSmoking and return True.
    /// </summary>
    /// <param name="uid">User's UId.</param>
    /// <param name="solutionUid">Uid of the Chemicals solution.</param>
    /// <param name="smoker">User's SmokerComponent.</param>
    /// <returns></returns>
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
            smoker.CurrentNicotineLevel = name.Quantity;
            smoker.TimeSinceSmoking = 0;
            smoker.CurrentSmokingInterval = 0;
            smoker.WithdrawalStage = 0;
            return true;
        }
        return false;
    }
    /// <summary>
    /// Check's if the user TimeSinceSmoking is above the threshold and if applicable updates the WithdrawalStage and
    /// gives an updated time.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="smoker"></param>
    private void SetWithdrawalStage(EntityUid uid, SmokerComponent smoker)
    {
        // No dividing by zero.
        if(smoker.WithdrawalStage < 0)
            smoker.WithdrawalStage = 0;
        // If it's not the time, continue.
        if (!(smoker.TimeSinceSmoking >= smoker.SmokingInterval + smoker.CurrentSmokingInterval))
            return;
        smoker.WithdrawalStage++;
        // Ensures that ignoring the need to smoke will get harder longer they go.
        smoker.CurrentSmokingInterval += smoker.SmokingInterval/(1+Math.Clamp(smoker.WithdrawalStage,0,7));
        switch (smoker.WithdrawalStage)
        {
            case 0:
                _popup.PopupEntity("All's fine in the world!", uid, uid);
                break;
            case 1:
                _popup.PopupEntity(Loc.GetString("trait-smoker-stage1"),uid,uid);
                break;
            case 2:
                _popup.PopupEntity(Loc.GetString("trait-smoker-stage2"), uid, uid,PopupType.Medium);
                break;
            case 3:
                _popup.PopupEntity(Loc.GetString("trait-smoker-stage3"), uid, uid,PopupType.MediumCaution);
                if (TryComp<SpeechComponent>(uid,out _) && !TryComp<MutedComponent>(uid, out _))
                    _chatSystem.TryEmoteWithChat(uid,"Sigh");
                break;
            case 4:
                _popup.PopupEntity(Loc.GetString("trait-smoker-stage4"), uid, uid,PopupType.MediumCaution);
                break;
            case 5:
                _popup.PopupEntity(Loc.GetString("trait-smoker-stage5"), uid, uid, PopupType.MediumCaution);
                break;
            default:
                _popup.PopupEntity(Loc.GetString("trait-smoker-stage6"), uid, uid, PopupType.MediumCaution);
                if (TryComp<SpeechComponent>(uid, out _)&& !TryComp<MutedComponent>(uid, out _)&& smoker.WithdrawalStage % 3 == 0)
                    _chatSystem.TryEmoteWithChat(uid,"Scream");
                break;
        }
    }
}







