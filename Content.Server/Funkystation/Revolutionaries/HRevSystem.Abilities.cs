
using Content.Shared.Actions;
using Content.Shared.Revolutionary;
using Content.Shared.Store.Components;
using static Content.Shared.Revolutionary.HRevComponent;
using Content.Server.Chat.Managers;
using Content.Shared.Chat;

namespace Content.Server.Revolutionary;

public sealed partial class HRevSystem : EntitySystem
{
    [Dependency]
    private readonly IChatManager _chat = default!;

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
        SubscribeLocalEvent<HRevComponent, EventHRevOpenStore>(OnOpenStore);
        SubscribeLocalEvent<HRevComponent, HRevSelectedVanguardEvent>(OnSelectVanguardPath);
        SubscribeLocalEvent<HRevComponent, HRevSelectedWarlordEvent>(OnSelectWarlordPath);
        SubscribeLocalEvent<HRevComponent, HRevSelectedWOTPEvent>(OnSelectWOTPPath);
    }

    private void OnOpenStore(EntityUid uid, HRevComponent comp, ref EventHRevOpenStore args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        _store.ToggleUi(uid, uid, store);
    }

    private void OnSelectVanguardPath(EntityUid uid, HRevComponent comp, ref HRevSelectedVanguardEvent ev)
    {
        comp.CurrentPath = HRevComponent.RevolutionaryPaths.VANGUARD;

        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        store.Categories.Add(RevCoinStore[RevolutionaryPaths.VANGUARD]);
        PlayerPathNotify(uid, comp.CurrentPath);
    }

    private void OnSelectWarlordPath(EntityUid uid, HRevComponent comp, ref HRevSelectedWarlordEvent ev)
    {
        comp.CurrentPath = RevolutionaryPaths.WARLORD;

        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        store.Categories.Add(RevCoinStore[HRevComponent.RevolutionaryPaths.WARLORD]);
        PlayerPathNotify(uid, comp.CurrentPath);
    }

    private void OnSelectWOTPPath(EntityUid uid, HRevComponent comp, ref HRevSelectedWOTPEvent ev)
    {
        comp.CurrentPath = HRevComponent.RevolutionaryPaths.WOTP;

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
