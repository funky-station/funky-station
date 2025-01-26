using System.Linq;
using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Server.Radio.Components;
using Content.Server.Roles;
using Content.Server.Station.Systems;
using Content.Shared.Administration;
using Content.Shared.Chat;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Stunnable;
using Content.Shared.Wires;
using Content.Shared.FixedPoint;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;
using Robust.Shared.Audio;
using Robust.Shared.Random;

namespace Content.Server.Silicons.Laws;

/// <inheritdoc/>
public sealed class SiliconLawSystem : SharedSiliconLawSystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
	[Dependency] private readonly IRobustRandom _robustRandom = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconLawBoundComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SiliconLawBoundComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<SiliconLawBoundComponent, ToggleLawsScreenEvent>(OnToggleLawsScreen);
        SubscribeLocalEvent<SiliconLawBoundComponent, BoundUIOpenedEvent>(OnBoundUIOpened);
        SubscribeLocalEvent<SiliconLawBoundComponent, PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);

        SubscribeLocalEvent<SiliconLawProviderComponent, GetSiliconLawsEvent>(OnDirectedGetLaws);
        SubscribeLocalEvent<SiliconLawProviderComponent, IonStormLawsEvent>(OnIonStormLaws);
        SubscribeLocalEvent<SiliconLawProviderComponent, MindAddedMessage>(OnLawProviderMindAdded);
        SubscribeLocalEvent<SiliconLawProviderComponent, MindRemovedMessage>(OnLawProviderMindRemoved);
        SubscribeLocalEvent<SiliconLawProviderComponent, GotEmaggedEvent>(OnEmagLawsAdded);
    }

    private void OnMapInit(EntityUid uid, SiliconLawBoundComponent component, MapInitEvent args)
    {
        GetLaws(uid, component);
    }

    private void OnMindAdded(EntityUid uid, SiliconLawBoundComponent component, MindAddedMessage args)
    {
        if (!TryComp<ActorComponent>(uid, out var actor))
            return;

        var msg = Loc.GetString("laws-notify");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
        _chatManager.ChatMessageToOne(ChatChannel.Server, msg, wrappedMessage, default, false, actor.PlayerSession.Channel, colorOverride: Color.FromHex("#2ed2fd"));

        if (!TryComp<SiliconLawProviderComponent>(uid, out var lawcomp))
            return;

        if (!lawcomp.Subverted)
            return;

        var modifedLawMsg = Loc.GetString("laws-notify-subverted");
        var modifiedLawWrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", modifedLawMsg));
        _chatManager.ChatMessageToOne(ChatChannel.Server, modifedLawMsg, modifiedLawWrappedMessage, default, false, actor.PlayerSession.Channel, colorOverride: Color.Red);
    }

    private void OnLawProviderMindAdded(Entity<SiliconLawProviderComponent> ent, ref MindAddedMessage args)
    {
        if (!ent.Comp.Subverted)
            return;
        EnsureSubvertedSiliconRole(args.Mind);
    }

    private void OnLawProviderMindRemoved(Entity<SiliconLawProviderComponent> ent, ref MindRemovedMessage args)
    {
        if (!ent.Comp.Subverted)
            return;
        RemoveSubvertedSiliconRole(args.Mind);

    }

    private void OnToggleLawsScreen(EntityUid uid, SiliconLawBoundComponent component, ToggleLawsScreenEvent args)
    {
        if (args.Handled || !TryComp<ActorComponent>(uid, out var actor))
            return;
        args.Handled = true;

        _userInterface.TryToggleUi(uid, SiliconLawsUiKey.Key, actor.PlayerSession);
    }

    private void OnBoundUIOpened(EntityUid uid, SiliconLawBoundComponent component, BoundUIOpenedEvent args)
    {
        TryComp(uid, out IntrinsicRadioTransmitterComponent? intrinsicRadio);
        var radioChannels = intrinsicRadio?.Channels;

        var state = new SiliconLawBuiState(GetLaws(uid).Laws, radioChannels);
        _userInterface.SetUiState(args.Entity, SiliconLawsUiKey.Key, state);
    }

    private void OnPlayerSpawnComplete(EntityUid uid, SiliconLawBoundComponent component, PlayerSpawnCompleteEvent args)
    {
        component.LastLawProvider = args.Station;
    }

    private void OnDirectedGetLaws(EntityUid uid, SiliconLawProviderComponent component, ref GetSiliconLawsEvent args)
    {
        if (args.Handled)
            return;

		args.Laws = VerifyLawsetInitialized(component);
        args.Handled = true;
    }

	private SiliconLawset VerifyLawsetInitialized(SiliconLawProviderComponent component)
	{
		if (component.Lawset == null)
            component.Lawset = GetLawset(component.Laws);
		return component.Lawset;
	}

	#region LawSetters
	/// <summary>
	/// Add a law to the current lawset.
	/// </summary>
	public void AddLaw(SiliconLawProviderComponent component, string lawText, LawGroups lawGroup)
	{
		switch (lawGroup) {
			case LawGroups.Law0:
				SetLaw0(component, lawText);
				break;
			case LawGroups.HackedLaw:
				AddHackedLaw(component, lawText);
				break;
			case LawGroups.IonLaw:
				AddIonLaw(component, lawText);
				break;
			case LawGroups.LawsetLaw:
				AddLawsetLaw(component, lawText);
				break;
			default:
				AddCustomLaw(component, lawText);
				break;
		}
	}

	/// <summary>
	/// Add a Law 0 to the current lawset.
	/// </summary>
	public void SetLaw0(SiliconLawProviderComponent component, string lawText)
	{
		SetLaw0(VerifyLawsetInitialized(component), lawText);
	}

	public void SetLaw0(SiliconLawset lawset, string lawText)
	{
		var law = new SiliconLaw
        {
            LawString = lawText,
			LawGroup = LawGroups.Law0,
            Order = FixedPoint2.New(0)
        };
		lawset.Law0 = law;
	}

	/// <summary>
	/// Add a hacked (emag) law to the current lawset.
	/// </summary>
	public void AddHackedLaw(SiliconLawProviderComponent component, string lawText)
	{
		AddHackedLaw(VerifyLawsetInitialized(component), lawText);
	}

	public void AddHackedLaw(SiliconLawset lawset, string lawText)
	{
		var law = new SiliconLaw
		{
			LawString = lawText,
			LawGroup = LawGroups.HackedLaw,
            Order = FixedPoint2.New(0.1 + (0.01 * lawset.HackedLaws.Count)),
			LawIdentifierOverride = GenerateCorruptLawString()
		};
		lawset.HackedLaws.Add(law);
	}

	/// <summary>
	/// Add an ion law to the current lawset.
	/// </summary>
	public void AddIonLaw(SiliconLawProviderComponent component, string lawText)
	{
		AddIonLaw(VerifyLawsetInitialized(component), lawText);
	}

	public void AddIonLaw(SiliconLawset lawset, string lawText)
	{
		var law = new SiliconLaw
		{
			LawString = lawText,
			LawGroup = LawGroups.IonLaw,
            Order = FixedPoint2.New(0.2 + (0.01 * lawset.IonLaws.Count)),
			LawIdentifierOverride = GenerateCorruptLawString()
		};
		lawset.IonLaws.Add(law);
	}

	/// <summary>
	/// Add a lawset law to the current lawset.
	/// </summary>
	public void AddLawsetLaw(SiliconLawProviderComponent component, string lawText)
	{
		AddLawsetLaw(VerifyLawsetInitialized(component), lawText);
	}

	public void AddLawsetLaw(SiliconLawProviderComponent component, SiliconLaw law)
	{
		AddLawsetLaw(VerifyLawsetInitialized(component), new string(law.LawString));
	}

	public void AddLawsetLaw(SiliconLawset lawset, SiliconLaw law)
	{
		AddLawsetLaw(lawset, new string(law.LawString));
	}

	public void AddLawsetLaw(SiliconLawset lawset, string lawText)
	{
		var law = new SiliconLaw
		{
			LawString = lawText,
			LawGroup = LawGroups.IonLaw,
            Order = FixedPoint2.New(1 + (1 * lawset.LawsetLaws.Count))
		};
		lawset.LawsetLaws.Add(law);
		ReorderCustomLaws(lawset);
	}

	/// <summary>
	/// Add a custom law to the current lawset.
	/// </summary>
	public void AddCustomLaw(SiliconLawProviderComponent component, string lawText)
	{
		AddCustomLaw(VerifyLawsetInitialized(component), lawText);
	}

	public void AddCustomLaw(SiliconLawset lawset, string lawText)
	{
		var law = new SiliconLaw
		{
			LawString = lawText,
			LawGroup = LawGroups.IonLaw,
            Order = FixedPoint2.New(
				1 + (1 * lawset.LawsetLaws.Count) + (1 * lawset.CustomLaws.Count)
			)
		};
		lawset.CustomLaws.Add(law);
	}
	#endregion
	#region LawModifiers
	/// <summary>
	/// Wipe all laws in the Lawset group.
	/// </summary>
	public void ClearLawsetLaws(SiliconLawProviderComponent component)
	{
		SiliconLawset lawset = VerifyLawsetInitialized(component);
		lawset.LawsetLaws = new List<SiliconLaw>();
		ReorderCustomLaws(component);
	}

	/// <summary>
	/// Reorder the Custom Laws.
	/// Called after Lawset Laws are changed.
	/// </summary>
	public void ReorderCustomLaws(SiliconLawProviderComponent component)
	{
		ReorderCustomLaws(VerifyLawsetInitialized(component));
	}

	public void ReorderCustomLaws(SiliconLawset lawset)
	{
		var baseOrder = FixedPoint2.New(lawset.LawsetLaws.Count);
		for (int i = 0; i < lawset.CustomLaws.Count; i++)
		{
			lawset.CustomLaws[i].Order = baseOrder + i;
		}
	}

	/// <summary>
	/// Shuffle the Lawset Laws.
	/// Sometimes used by Ion Storms.
	/// </summary>
	public void ShuffleLawset(SiliconLawset lawset)
	{
		var baseOrder = FixedPoint2.New(1);
		_robustRandom.Shuffle(lawset.LawsetLaws);

		// change order based on shuffled position
		for (int i = 0; i < lawset.LawsetLaws.Count; i++)
		{
			lawset.LawsetLaws[i].Order = baseOrder + i;
		}
	}

	/// <summary>
	/// Remove a random law.
	/// Law 0s are not eligible.
	/// Sometimes used by Ion Storms.
	/// </summary>
	public void RemoveRandomLaw(SiliconLawset lawset)
	{
		var i = (lawset.Law0 == null) ? _robustRandom.Next(lawset.Laws.Count) : _robustRandom.Next(lawset.Laws.Count-1);

		if (i < lawset.HackedLaws.Count)
			lawset.HackedLaws.RemoveAt(i);
		else if ((i - lawset.HackedLaws.Count) < lawset.IonLaws.Count)
			lawset.IonLaws.RemoveAt(i - lawset.HackedLaws.Count);
		else if ((i - (lawset.HackedLaws.Count + lawset.IonLaws.Count)) < lawset.LawsetLaws.Count)
			lawset.LawsetLaws.RemoveAt(i - (lawset.HackedLaws.Count + lawset.IonLaws.Count));
		else
			lawset.CustomLaws.RemoveAt(i - (lawset.HackedLaws.Count + lawset.IonLaws.Count + lawset.LawsetLaws.Count));
	}

	/// <summary>
	/// Replace a random law.
	/// Law 0s are not eligible.
	/// Sometimes used by Ion Storms.
	/// </summary>
	public void ReplaceRandomLaw(SiliconLawset lawset, string lawText)
	{
		var i = (lawset.Law0 == null) ? _robustRandom.Next(lawset.Laws.Count) : _robustRandom.Next(lawset.Laws.Count-1);

		if (i < lawset.HackedLaws.Count)
		{
			lawset.HackedLaws[i] = new SiliconLaw()
            {
                LawString = lawText,
                Order = lawset.HackedLaws[i].Order,
				LawIdentifierOverride = GenerateCorruptLawString()
            };
		}
		else if ((i - lawset.HackedLaws.Count) < lawset.IonLaws.Count)
		{
			int idx = (i - lawset.HackedLaws.Count);
			lawset.IonLaws[idx] = new SiliconLaw()
            {
                LawString = lawText,
                Order = lawset.IonLaws[idx].Order,
				LawIdentifierOverride = GenerateCorruptLawString()
            };
		}
		else if ((i - (lawset.HackedLaws.Count + lawset.IonLaws.Count)) < lawset.LawsetLaws.Count)
		{
			int idx = (i - (lawset.HackedLaws.Count + lawset.IonLaws.Count));
			lawset.LawsetLaws[idx] = new SiliconLaw()
            {
                LawString = lawText,
                Order = lawset.LawsetLaws[idx].Order,
				LawIdentifierOverride = GenerateCorruptLawString()
            };
		}
		else
		{
			int idx = (i - (lawset.HackedLaws.Count + lawset.IonLaws.Count + lawset.LawsetLaws.Count));
			lawset.CustomLaws[idx] = new SiliconLaw()
            {
                LawString = lawText,
                Order = lawset.CustomLaws[idx].Order,
				LawIdentifierOverride = GenerateCorruptLawString()
            };
		}		
	}
	#endregion

	/// <summary>
	/// Generate a random string of characters for
	/// corrupted laws' LawIdentifierOverride properties.
	/// </summary>
	private string GenerateCorruptLawString(int characters = 3)
	{
		char[] chars = ['?', '$', '-', '/', '£', '&', '@'];
		string result = "##";
		for (int i = 0; i < characters; i++)
			result = result + chars[_robustRandom.Next(0, chars.Length)];
		return result + "##";
	}

    private void OnIonStormLaws(EntityUid uid, SiliconLawProviderComponent component, ref IonStormLawsEvent args)
    {
        // Emagged borgs are immune to ion storm
        if (!HasComp<EmaggedComponent>(uid))
        {
            component.Lawset = args.Lawset;

            // gotta tell player to check their laws
            NotifyLawsChanged(uid, component.LawUploadSound);

            // Show the silicon has been subverted.
            component.Subverted = true;

            // new laws may allow antagonist behaviour so make it clear for admins
            if(_mind.TryGetMind(uid, out var mindId, out _))
                EnsureSubvertedSiliconRole(mindId);

        }
    }

    private void OnEmagLawsAdded(EntityUid uid, SiliconLawProviderComponent component, ref GotEmaggedEvent args)
    {
        SiliconLawset lawset = VerifyLawsetInitialized(component);

        // Show the silicon has been subverted.
        component.Subverted = true;

        // Add the first emag law before the others
        SetLaw0(lawset,
			Loc.GetString("law-emag-custom", ("name", Name(args.UserUid)), ("title", Loc.GetString(lawset.ObeysTo)))
		);

        //Add the secrecy law after the others
		AddHackedLaw(lawset,
			Loc.GetString("law-emag-secrecy", ("faction", Loc.GetString(lawset.ObeysTo)))
		);
    }

    protected override void OnGotEmagged(EntityUid uid, EmagSiliconLawComponent component, ref GotEmaggedEvent args)
    {
        if (component.RequireOpenPanel && TryComp<WiresPanelComponent>(uid, out var panel) && !panel.Open)
            return;

        base.OnGotEmagged(uid, component, ref args);
        NotifyLawsChanged(uid, component.EmaggedSound);
        if(_mind.TryGetMind(uid, out var mindId, out _))
            EnsureSubvertedSiliconRole(mindId);

        _stunSystem.TryParalyze(uid, component.StunTime, true);

    }

    private void EnsureSubvertedSiliconRole(EntityUid mindId)
    {
        if (!_roles.MindHasRole<SubvertedSiliconRoleComponent>(mindId))
            _roles.MindAddRole(mindId, "MindRoleSubvertedSilicon");
    }

    private void RemoveSubvertedSiliconRole(EntityUid mindId)
    {
        if (_roles.MindHasRole<SubvertedSiliconRoleComponent>(mindId))
            _roles.MindTryRemoveRole<SubvertedSiliconRoleComponent>(mindId);
    }

    public SiliconLawset GetLaws(EntityUid uid, SiliconLawBoundComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return new SiliconLawset();

        var ev = new GetSiliconLawsEvent(uid);

        RaiseLocalEvent(uid, ref ev);
        if (ev.Handled)
        {
            component.LastLawProvider = uid;
            return ev.Laws;
        }

        var xform = Transform(uid);

        if (_station.GetOwningStation(uid, xform) is { } station)
        {
            RaiseLocalEvent(station, ref ev);
            if (ev.Handled)
            {
                component.LastLawProvider = station;
                return ev.Laws;
            }
        }

        if (xform.GridUid is { } grid)
        {
            RaiseLocalEvent(grid, ref ev);
            if (ev.Handled)
            {
                component.LastLawProvider = grid;
                return ev.Laws;
            }
        }

        if (component.LastLawProvider == null ||
            Deleted(component.LastLawProvider) ||
            Terminating(component.LastLawProvider.Value))
        {
            component.LastLawProvider = null;
        }
        else
        {
            RaiseLocalEvent(component.LastLawProvider.Value, ref ev);
            if (ev.Handled)
            {
                return ev.Laws;
            }
        }

        RaiseLocalEvent(ref ev);
        return ev.Laws;
    }

    public void NotifyLawsChanged(EntityUid uid, SoundSpecifier? cue = null)
    {
        if (!TryComp<ActorComponent>(uid, out var actor))
            return;

        var msg = Loc.GetString("laws-update-notify");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
        _chatManager.ChatMessageToOne(ChatChannel.Server, msg, wrappedMessage, default, false, actor.PlayerSession.Channel, colorOverride: Color.Red);

        if (cue != null && _mind.TryGetMind(uid, out var mindId, out _))
            _roles.MindPlaySound(mindId, cue);
    }

    /// <summary>
    /// Extract all the laws from a lawset's prototype ids.
    /// </summary>
    public SiliconLawset GetLawset(ProtoId<SiliconLawsetPrototype> lawset)
    {
        var proto = _prototype.Index(lawset);

		var laws = new SiliconLawset();
		foreach (var law in proto.Laws)
		{
			AddLawsetLaw(laws, _prototype.Index<SiliconLawPrototype>(law));
		};
		laws.ObeysTo = String.Copy(proto.ObeysTo);

        return laws;
    }

    /// <summary>
    /// Set the laws of a silicon entity while notifying the player.
    /// </summary>
    public void SetLaws(List<SiliconLaw> newLaws, EntityUid target, SoundSpecifier? cue = null)
    {
        if (!TryComp<SiliconLawProviderComponent>(target, out var component))
            return;

		ClearLawsetLaws(component);
		foreach (SiliconLaw law in newLaws)
			AddLawsetLaw(component, law);
        NotifyLawsChanged(target, cue);
    }

    protected override void OnUpdaterInsert(Entity<SiliconLawUpdaterComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        // TODO: Prediction dump this
        if (!TryComp(args.Entity, out SiliconLawProviderComponent? provider))
            return;

        var lawset = GetLawset(provider.Laws).Laws;
        var query = EntityManager.CompRegistryQueryEnumerator(ent.Comp.Components);

        while (query.MoveNext(out var update))
        {
            SetLaws(lawset, update, provider.LawUploadSound);
        }
    }
}

[ToolshedCommand, AdminCommand(AdminFlags.Admin)]
public sealed class LawsCommand : ToolshedCommand
{
    private SiliconLawSystem? _law;

    [CommandImplementation("list")]
    public IEnumerable<EntityUid> List()
    {
        var query = EntityManager.EntityQueryEnumerator<SiliconLawBoundComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            yield return uid;
        }
    }

    [CommandImplementation("get")]
    public IEnumerable<string> Get([PipedArgument] EntityUid lawbound)
    {
        _law ??= GetSys<SiliconLawSystem>();

        foreach (var law in _law.GetLaws(lawbound).Laws)
        {
            yield return $"law {law.LawIdentifierOverride ?? law.Order.ToString()}: {Loc.GetString(law.LawString)}";
        }
    }
}
