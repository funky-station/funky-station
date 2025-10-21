// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 mr-bo-jangles <mr-bo-jangles@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ArchRBX <5040911+ArchRBX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Carrot <carpecarrot@gmail.com>
// SPDX-FileCopyrightText: 2025 Currot <carpecarrot@gmail.com>
// SPDX-FileCopyrightText: 2025 Sophia Rustfield <gitlab@catwolf.xyz>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 archrbx <punk.gear5260@fastmail.com>
// SPDX-FileCopyrightText: 2025 jackel234 <52829582+jackel234@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;
using Content.Shared.Radio.Components;

namespace Content.Shared.PAI;

/// <summary>
/// pAIs, or Personal AIs, are essentially portable ghost role generators.
/// In their current implementation, they create a ghost role anyone can access,
/// and that a player can also "wipe" (reset/kick out player).
/// Theoretically speaking pAIs are supposed to use a dedicated "offer and select" system,
///  with the player holding the pAI being able to choose one of the ghosts in the round.
/// This seems too complicated for an initial implementation, though,
///  and there's not always enough players and ghost roles to justify it.
/// </summary>
public abstract class SharedPAISystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PAIComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PAIComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<PAIComponent, PAIEnableEncryptionEvent>(EnableEncryption);
    }

    private void OnMapInit(Entity<PAIComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ent.Comp.ShopActionId);
        _actions.AddAction(ent, ent.Comp.OpenPdaActionId);
    }

    private void OnShutdown(Entity<PAIComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent, ent.Comp.ShopAction);
        _actions.RemoveAction(ent, ent.Comp.OpenPdaAction);
    }

    private void EnableEncryption(Entity<PAIComponent> ent, ref PAIEnableEncryptionEvent args)
    {
        EnsureComp<EncryptionKeyHolderComponent>(ent, out var holder);
        holder.KeySlots = 4;
    }
}
public sealed partial class PAIShopActionEvent : InstantActionEvent
{
}
public sealed partial class PAIOpenPdaActionEvent : InstantActionEvent
{
}

[DataDefinition]
public sealed partial class PAIEnableEncryptionEvent : EntityEventArgs;
