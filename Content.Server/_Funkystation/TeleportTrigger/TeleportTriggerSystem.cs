using Content.Server.Chat.Managers;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Popups;
using Content.Server.Radio.EntitySystems;
using Content.Shared.Implants.Components;
using Content.Shared.Nuke;
using Content.Shared.Popups;
using Robust.Server.GameObjects;

namespace Content.Server._Funkystation.TeleportTrigger;

public sealed class TeleportOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TeleportOnTriggerComponent, TriggerEvent>(OnImplantTriggered);
    }

    private void OnImplantTriggered(EntityUid uid, TeleportOnTriggerComponent component, TriggerEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<SubdermalImplantComponent>(uid, out var implant) ||
            implant.ImplantedEntity is not { } user)
        {
            return;
        }

        if (HasNukeDisk(user) && !component.AllowNukeDisk)
        {
            _popup.PopupEntity(Loc.GetString("emergency-teleport-has-nuke-disk"), user, user, PopupType.LargeCaution);
            args.Handled = true;
            return;
        }

        var markerPrototypeId = component.MarkerPrototype.ToString();
        var marker = FindTeleportMarker(markerPrototypeId);
        if (marker == null)
        {
            _popup.PopupEntity(Loc.GetString("emergency-teleport-no-marker"), user, user, PopupType.LargeCaution);
            args.Handled = true;
            return;
        }

        var markerXform = Transform(marker.Value);
        _transform.SetCoordinates(user, markerXform.Coordinates);

        var dummy = Spawn("LifelineMarker", markerXform.Coordinates);
        _metaData.SetEntityName(dummy, "CENTRAL COMMAND EMERGENCY BROADCAST");

        var message = Loc.GetString("emergency-teleport-radio-msg", ("user", user));

        _radio.SendRadioMessage(dummy, message, "CentCom", dummy);

        QueueDel(dummy);

        _popup.PopupEntity(Loc.GetString("emergency-teleport-success"), user, user, PopupType.Medium);
        _chatManager.SendAdminAlert(Loc.GetString("admin-lifeline-tp", ("playerName", Name(user))));

        args.Handled = true;
    }

    private bool HasNukeDisk(EntityUid user)
    {
        var diskQuery = EntityQueryEnumerator<NukeDiskComponent>();
        while (diskQuery.MoveNext(out var diskUid, out _))
        {
            var diskTransform = Transform(diskUid);
            var parent = diskTransform.ParentUid;
            while (parent.IsValid())
            {
                if (parent == user) return true;
                parent = Transform(parent).ParentUid;
            }
        }
        return false;
    }

    private EntityUid? FindTeleportMarker(string markerPrototypeId)
    {
        var query = EntityQueryEnumerator<MetaDataComponent>();
        while (query.MoveNext(out var uid, out var metadata))
        {
            if (metadata.EntityPrototype?.ID == markerPrototypeId)
                return uid;
        }
        return null;
    }
}
