using System.Linq;
using Content.Server.Pinpointer;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.HealthExaminable;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Photography;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.Photography;

public sealed partial class PhotographySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly HealthExaminableSystem _healthExaminableSystem = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhotoCameraComponent, GetItemActionsEvent>(OnGetActionsEvent);
        SubscribeLocalEvent<PhotoComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<PhotoComponent, UseInHandEvent>(OnUseInHandEvent);
    }

    private void OnExamined(EntityUid uid, PhotoComponent component, ExaminedEvent args)
    {
        args.PushMessage(component.Descriptor);
    }

    private void OnUseInHandEvent(EntityUid uid, PhotoComponent component, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (!EntityManager.TryGetComponent(args.User, out ActorComponent? actor))
            return;

        OpenSessionFor(actor.PlayerSession, uid);
    }

    private void OnGetActionsEvent(EntityUid uid, PhotoCameraComponent component, GetItemActionsEvent args)
    {
        args.AddAction(ref component.ActionEntity, component.Action);
    }

    public void CreatePhotoOnPlayer(EntityUid player, bool selfie)
    {
        var res = _lookup.GetEntitiesInRange(Transform(player).Coordinates, 5f, LookupFlags.Uncontained);
        var markup = new FormattedMessage();

        markup.AddMarkupOrThrow("Upon closer inspection, you notice the following:\n");
        markup.AddMarkupOrThrow($"Nearest beacon: {_navMap.GetNearestBeaconString(player)}\n");

        //todo: simplify code here
        foreach (var entityUid in res)
        {
            if (selfie)
            {
                markup.AddText($"You see {MetaData(player).EntityName}'s face in the picture.\n");
            }
            else if (entityUid == player)
            {
                continue;
            }

            if (TryComp<HealthExaminableComponent>(entityUid, out var examinableComponent) && TryComp<DamageableComponent>(entityUid, out var damageableComponent))
            {
                var msg = ParseLiving(entityUid, examinableComponent, damageableComponent);
                var meta = MetaData(entityUid);

                var nameMsg = new FormattedMessage();

                nameMsg.AddMarkupOrThrow($"You see {meta.EntityName}, ");
                nameMsg.AddMarkupOrThrow($"{msg.ToString().ToLower()}\n");

                markup.AddMessage(nameMsg);
            }

            if (TryComp<ObjectOfInterestComponent>(entityUid, out _))
            {
                var msg = ParseObjectOfInterest(entityUid);

                markup.AddMessage(msg);
            }
        }

        var photo = Spawn("Photo", Transform(player).Coordinates);

        if (!TryComp<PhotoComponent>(photo, out var component))
            return;

        component.Descriptor = markup;
    }

    private FormattedMessage ParseLiving(EntityUid uid, HealthExaminableComponent examinableComponent, DamageableComponent damageableComponent)
    {
        var msg = _healthExaminableSystem.CreateMarkup(uid, examinableComponent, damageableComponent);

        return new FormattedMessage(msg);
    }

    private FormattedMessage ParseObjectOfInterest(EntityUid objectOfInterestUid)
    {
        var meta = MetaData(objectOfInterestUid);
        var msg = new FormattedMessage();

        msg.AddText($"The {meta.EntityName} can be seen in the photograph.");

        return msg;
    }

    private void OnPhotoCompInit(EntityUid uid, PhotoComponent component, ComponentInit args)
    {

    }
}
