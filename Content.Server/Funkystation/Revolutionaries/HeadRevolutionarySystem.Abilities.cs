using System.Linq;
using Content.Shared.Revolutionary;
using Content.Shared.Store.Components;
using static Content.Shared.Revolutionary.HeadRevolutionaryPathComponent;
using Content.Server.Chat.Managers;
using Content.Shared.Chat;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Server.Revolutionary;

public sealed partial class HeadRevolutionarySystem
{
    [Dependency]
    private readonly IChatManager _chat = default!;
    [Dependency]
    private readonly SharedHandsSystem _hands = default!;
    [Dependency]
    private readonly EntityLookupSystem _lookup = default!;
    [Dependency]
    private readonly MetaDataSystem _metadata = default!;

    public EntProtoId[] Uniforms = [];

    public static string GetFriendlyRevPathName(RevolutionaryPaths path)
    {
        return path switch
        {
            RevolutionaryPaths.NONE => "None",
            RevolutionaryPaths.VANGUARD => "Vanguard",
            RevolutionaryPaths.WOTP => "WOTP",
            RevolutionaryPaths.WARLORD => "Warlord",
            _ => "None",
        };
    }

    public void SubscribeEvents()
    {
        SubscribeLocalEvent<HeadRevolutionaryPathComponent, EventHeadRevolutionaryOpenUplink>(OnOpenStore);
        SubscribeLocalEvent<HeadRevolutionaryPathComponent, HeadRevolutionarySelectedVanguardEvent>(OnSelectVanguardPath);
        SubscribeLocalEvent<HeadRevolutionaryPathComponent, HeadRevolutionarySelectedWotpEvent>(OnSelectWarlordPath);
        SubscribeLocalEvent<HeadRevolutionaryPathComponent, HeadRevolutionarySelectedWarlordEvent>(OnSelectWOTPPath);

        SubscribeLocalEvent<HeadRevolutionaryPathComponent, HeadRevolutionaryCreateUniformEvent>(OnTryCreateRevolutionaryUniform);
    }

    private void OnOpenStore(EntityUid uid, HeadRevolutionaryPathComponent comp, ref EventHeadRevolutionaryOpenUplink args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        _store.ToggleUi(uid, uid, store);
    }

    private bool CheckIfUniformIsAdded(EntityUid uid)
    {
        return TryComp<MetaDataComponent>(uid, out var metaData)
               && Uniforms.Any(uniform => metaData.EntityPrototype!.ID.Contains(uniform));
    }

    private bool CheckIfItemIsClothing(EntityUid uid)
    {
        return TryComp<MetaDataComponent>(uid, out var metaData) && metaData.EntityPrototype!.ID.Contains("Clothing");
    }

    private void OnTryCreateRevolutionaryUniform(EntityUid uid, HeadRevolutionaryPathComponent comp, ref HeadRevolutionaryCreateUniformEvent args)
    {
        var activeItem = _hands.GetActiveItem(uid);

        if (CheckIfUniformIsAdded(uid))
            return;

        if (!CheckIfItemIsClothing(uid))
            return;


    }

    private void OnSelectVanguardPath(EntityUid uid, HeadRevolutionaryPathComponent comp, ref HeadRevolutionarySelectedVanguardEvent ev)
    {
        comp.CurrentPath = RevolutionaryPaths.VANGUARD;

        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        store.Categories.Add(RevCoinStore[RevolutionaryPaths.VANGUARD]);
        PlayerPathNotify(uid, comp.CurrentPath);
    }

    private void OnSelectWarlordPath(EntityUid uid, HeadRevolutionaryPathComponent comp, ref HeadRevolutionarySelectedWotpEvent ev)
    {
        comp.CurrentPath = RevolutionaryPaths.WARLORD;

        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        store.Categories.Add(RevCoinStore[RevolutionaryPaths.WARLORD]);
        PlayerPathNotify(uid, comp.CurrentPath);
    }

    private void OnSelectWOTPPath(EntityUid uid, HeadRevolutionaryPathComponent comp, ref HeadRevolutionarySelectedWarlordEvent ev)
    {
        comp.CurrentPath = RevolutionaryPaths.WOTP;

        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        store.Categories.Add(RevCoinStore[RevolutionaryPaths.WOTP]);
        PlayerPathNotify(uid, comp.CurrentPath);
    }

    private void PlayerPathNotify(EntityUid uid, RevolutionaryPaths path)
    {
        var selectedPathFriendly = GetFriendlyRevPathName(path);
        var message = Loc.GetString($"hrevmenu-{selectedPathFriendly.ToLower()}-select-chat");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));

        if (_mind.TryGetMind(uid, out _, out var mindComponent) && mindComponent.Session != null)
        {
            _chat.ChatMessageToOne(ChatChannel.Server,
                message,
                wrappedMessage,
                default,
                false,
                mindComponent.Session.Channel,
                Color.FromSrgb(new Color(224, 164, 164)));
        }
    }
}
