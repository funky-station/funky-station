// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using System.Linq;
using Content.Shared.Body.Organ;
using Content.Shared.Chemistry.Components.SolutionManager;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Serialization;
using Content.Shared.BloodCult.Components;
using Content.Shared.Antag;

namespace Content.Shared.BloodCult;

public abstract class SharedBloodCultistSystem : EntitySystem
{
	[Dependency] protected readonly SharedContainerSystem Container = default!;

	public override void Initialize()
    {
        base.Initialize();

		SubscribeLocalEvent<BloodCultistComponent, ComponentGetStateAttemptEvent>(OnCultistCompGetStateAttempt);
		SubscribeLocalEvent<BloodCultistComponent, ComponentStartup>(DirtyRevComps);
		
		// Subscribe to BeforeEntityFlush to ensure cleanup before shutdown flush (runs on both client and server)
		EntityManager.BeforeEntityFlush += OnBeforeEntityFlush;
	}
	
	public override void Shutdown()
	{
		EntityManager.BeforeEntityFlush -= OnBeforeEntityFlush;
		base.Shutdown();
	}
	
	/// <summary>
	/// Handles cleanup before entity flush during shutdown.
	/// This ensures all blood gland organs and their solution entities are deleted before the flush.
	/// Runs on both client and server.
	/// </summary>
	private void OnBeforeEntityFlush()
	{
		// Find all blood gland organs (including those already terminating)
		var query = EntityQueryEnumerator<OrganComponent>();
		while (query.MoveNext(out var organUid, out var organ))
		{
			if (organ.SlotId != "blood_gland")
				continue;
				
			// Try to get solution entities directly from containers
			// Use TryGetContainer which should work even if the organ is terminating
			if (Container.TryGetContainer(organUid, "solution@organ", out var organContainer) 
				&& organContainer is ContainerSlot organSlot 
				&& organSlot.ContainedEntity is { } organSolution)
			{
				// Force delete even if already terminating
				if (Exists(organSolution))
				{
					try
					{
						EntityManager.DeleteEntity(organSolution);
					}
					catch
					{
						// Ignore errors during shutdown
					}
				}
			}
			
			if (Container.TryGetContainer(organUid, "solution@food", out var foodContainer) 
				&& foodContainer is ContainerSlot foodSlot 
				&& foodSlot.ContainedEntity is { } foodSolution)
			{
				// Force delete even if already terminating
				if (Exists(foodSolution))
				{
					try
					{
						EntityManager.DeleteEntity(foodSolution);
					}
					catch
					{
						// Ignore errors during shutdown
					}
				}
			}
			
			// Also try to get all containers as a fallback
			var allContainers = Container.GetAllContainers(organUid);
			foreach (var container in allContainers)
			{
				foreach (var contained in container.ContainedEntities.ToArray())
				{
					if (Exists(contained))
					{
						try
						{
							EntityManager.DeleteEntity(contained);
						}
						catch
						{
							// Ignore errors during shutdown
						}
					}
				}
			}
			
			// Delete the organ itself (force delete even if already terminating)
			if (Exists(organUid))
			{
				try
				{
					EntityManager.DeleteEntity(organUid);
				}
				catch
				{
					// Ignore errors during shutdown
				}
			}
		}
	}

	/// <summary>
    /// Determines if a BloodCultist component should be sent to the client.
    /// </summary>
    private void OnCultistCompGetStateAttempt(EntityUid uid, BloodCultistComponent comp, ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }
	/// <summary>
    /// The criteria that determine whether a BloodCultist component should be sent to a client.
    /// </summary>
    /// <param name="player"> The Player the component will be sent to.</param>
    /// <returns></returns>
    private bool CanGetState(ICommonSession? player)
    {
        if (player?.AttachedEntity is not {} uid)
            return true;

        if (HasComp<BloodCultistComponent>(uid))
            return true;

        return HasComp<ShowAntagIconsComponent>(uid);
    }

    private void DirtyRevComps<T>(EntityUid someUid, T someComp, ComponentStartup ev)
    {
        var cultComps = AllEntityQuery<BloodCultistComponent>();
        while (cultComps.MoveNext(out var uid, out var comp))
        {
            Dirty(uid, comp);
        }
    }
}

[Serializable, NetSerializable]
public enum BloodCultistCommuneUIKey : byte
{
	Key
}

[Serializable, NetSerializable]
public sealed class BloodCultCommuneBuiState : BoundUserInterfaceState
{
	public readonly string Message;

	public BloodCultCommuneBuiState(string message)
	{
		Message = message;
	}
}

[Serializable, NetSerializable]
public sealed class BloodCultCommuneSendMessage : BoundUserInterfaceMessage
{
    public readonly string Message;

    public BloodCultCommuneSendMessage(string message)
    {
        Message = message;
    }
}

/// <summary>
///    Called when a revive rune is used on the target. Revives the target if
///	   and only if enough revive charges remain.
/// </summary>
public sealed class ReviveRuneAttemptEvent : CancellableEntityEventArgs
{
	public readonly EntityUid Target;
	public readonly EntityUid? User;
	public readonly EntityUid? Used;

	public ReviveRuneAttemptEvent(EntityUid target, EntityUid? user, EntityUid? used)
	{
		Target = target;
		User = user;
		Used = used;
	}
}

/// <summary>
///    Called when a target has been potentially revived by a rune.
///	   Turns a catatonic target into a ghost role.
/// </summary>
public sealed class GhostifyRuneEvent : CancellableEntityEventArgs
{
	public readonly EntityUid Target;
	public readonly EntityUid? User;
	public readonly EntityUid? Used;

	public GhostifyRuneEvent(EntityUid target, EntityUid? user, EntityUid? used)
	{
		Target = target;
		User = user;
		Used = used;
	}
}

/// <summary>
///    Called when a victim has been potentially sacrificed by a rune.
/// </summary>
public sealed class SacrificeRuneEvent : CancellableEntityEventArgs
{
	public readonly EntityUid Victim;
	public readonly EntityUid? User;
	public readonly EntityUid? Used;
	public readonly EntityUid[] Invokers;

	public SacrificeRuneEvent(EntityUid victim, EntityUid user, EntityUid? used, EntityUid[] invokers)
	{
		Victim = victim;
		User = user;
		Used = used;
		Invokers = invokers;
	}
}

/// <summary>
///    Called when a subject has been potentially converted by a rune.
/// </summary>
public sealed class ConvertRuneEvent : CancellableEntityEventArgs
{
	public readonly EntityUid Subject;
	public readonly EntityUid? User;
	public readonly EntityUid? Used;
	public readonly EntityUid[] Invokers;

	public ConvertRuneEvent(EntityUid subject, EntityUid user, EntityUid? used, EntityUid[] invokers)
	{
		Subject = subject;
		User = user;
		Used = used;
		Invokers = invokers;
	}
}
