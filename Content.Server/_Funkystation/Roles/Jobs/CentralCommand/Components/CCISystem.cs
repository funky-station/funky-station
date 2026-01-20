using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Preferences.Managers;
using Content.Shared.Humanoid;

namespace Content.Server._Funkystation.Roles.Jobs.CentralCommand.Components;

/// <summary>
/// This handles...
/// </summary>
public sealed class CCISystem : EntitySystem
{
    [Dependency] private readonly IServerPreferencesManager _preferences = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _appearance = default!;
    [Dependency] private readonly GhostRoleSystem _ghostRole = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CentralCommandInternComponent, TakeGhostRoleEvent>(OnTakeGhostRole);
    }

    private void OnTakeGhostRole(Entity<CentralCommandInternComponent> ent, ref TakeGhostRoleEvent args)
    {
        if (args.TookRole)
            return;

        var session = args.Player;

        var prefs = _preferences.GetPreferences(session.UserId);

        var profile = prefs.SelectProfileForJob("Passenger");

        if (profile == null)
        {
            Log.Warning($"No valid profile found for Central Command Intern ghost role for player {session.Name}.");
            return;
        }

        Log.Info($"Applying Central Command Intern ghost role profile for player {session.Name}.");

        if (TryComp<HumanoidAppearanceComponent>(ent, out var appearance))
        {
            _appearance.LoadProfile(ent, profile);
        }
    }
}
