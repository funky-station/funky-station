using System.Numerics;
using Content.Shared.Chat;
using Content.Shared.Damage.Components;
using Content.Shared.Examine;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Map;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;

namespace Content.Shared.Damage.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class SensitiveHearingSystem : EntitySystem
{

    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<GunShotEvent>(OnGunShotEvent); //Doesn't work for some reason.
        SubscribeLocalEvent<Components.SensitiveHearingComponent, SpeakAttemptEvent>(OnSpeakAttemptEvent);
        base.Initialize();
    }

    private void OnSpeakAttemptEvent(EntityUid uid, Components.SensitiveHearingComponent component, ref SpeakAttemptEvent ev)
    {
        // NOTE: this prevents you from speaking in all ways, so, when you're deaf, no one will hear you.
        // This is not the intended behavior for this system.
        //Everyone except the deaf player should see their chat message

        if (component.damageAmount > 100.0)
        {
            //need to use Loc
            var iSession = GetEntityICommonSession(uid);
            if (iSession != null)
                _popupSystem.PopupEntity("You spoke something", uid, iSession, PopupType.Medium);
            ev.Cancel();
        }
    }


    public void BlastRadius(float amount, float radius, MapCoordinates coords)
    {
        var blastEntities = _lookupSystem.GetEntitiesInRange(coords, radius);
        foreach (var entity in blastEntities)
        {
            // skips an iteration if entity does not have sensitive hearing or does not have a valid position in the world.
            if (!HasComp<Components.SensitiveHearingComponent>(entity) || !HasComp<TransformComponent>(entity))
                continue;

            var entCoords =  _transformSystem.GetMapCoordinates(entity);

            //pythagoras theorem
            var distance = Math.Sqrt(Math.Pow(entCoords.X - coords.X, 2.0) + Math.Pow(entCoords.Y - coords.Y, 2.0));

            //lowkey no clue how to use a predicate here. this works
            if (!_examine.InRangeUnOccluded(coords, entCoords, radius, predicate: (e) => false))
                continue;


            //show pain message when a certain damage threshold is passed, in or case this threshold is 50.0f.
            if (Comp<Components.SensitiveHearingComponent>(entity).damageAmount >= 50.0)
            {
                //I don't like using var, feel free to use intellisense.
                //get user's ISession to show the message locally, didn't test this out yet.
                var iSession = GetEntityICommonSession(entity);
                if (iSession != null)
                    _popupSystem.PopupEntity("Your eardrums tremble", entity, iSession, PopupType.Medium);
            }

            Comp<Components.SensitiveHearingComponent>(entity).damageAmount += CalculateFalloff(amount, radius, distance);
        }

    }

    private ICommonSession? GetEntityICommonSession(EntityUid entity)
    {
        MindContainerComponent? mindContainer = CompOrNull<MindContainerComponent>(entity);
        MindComponent? mind;
        if (mindContainer == null || !mindContainer.HasMind)
            return null;
        mind = CompOrNull<MindComponent>(mindContainer.Mind);
        return mind?.Session;
    }

    private float CalculateFalloff(float maxDamage, float maxDistance, double sample)
    {
        // NOTE: Using linear formula because it deals better damage.

        //no clue how safe an explicit cast here is
        return (float) (maxDamage * (sample / maxDistance));
        // return (float) Math.Pow((1 - (1 / maxDistance) * sample), 2) * maxDamage;
    }


    //This does NOT work.
    private void OnGunShotEvent(ref GunShotEvent msg)
    {
        TransformComponent? xform = CompOrNull<TransformComponent>(msg.User);
        if (xform == null)
            return;

        BlastRadius(10.0f, 3.0f, _transformSystem.GetMapCoordinates(xform));
    }



}

