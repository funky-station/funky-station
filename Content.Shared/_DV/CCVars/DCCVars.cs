using Robust.Shared.Configuration;

namespace Content.Shared._DV.CCVars;

/// <summary>
/// DeltaV specific cvars.
/// </summary>
[CVarDefs]
// ReSharper disable once InconsistentNaming - Shush you
public sealed partial class DCCVars
{

    /*
     * No EORG
     */

    /// <summary>
    /// Whether the no EORG popup is enabled.
    /// </summary>
    public static readonly CVarDef<bool> RoundEndNoEorgPopup =
        CVarDef.Create("game.round_end_eorg_popup_enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Skip the no EORG popup.
    /// </summary>
    public static readonly CVarDef<bool> SkipRoundEndNoEorgPopup =
        CVarDef.Create("game.skip_round_end_eorg_popup", false, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// How long to display the EORG popup for.
    /// </summary>
    public static readonly CVarDef<float> RoundEndNoEorgPopupTime =
        CVarDef.Create("game.round_end_eorg_popup_time", 5f, CVar.SERVER | CVar.REPLICATED);

    /*
     * Auto ACO
     */

    /// <summary>
    /// How long after the announcement before the spare ID is unlocked
    /// </summary>
    public static readonly CVarDef<TimeSpan> SpareIdUnlockDelay =
        CVarDef.Create("game.spare_id.unlock_delay", TimeSpan.FromMinutes(5), CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// How long to wait before checking for a captain after roundstart
    /// </summary>
    public static readonly CVarDef<TimeSpan> SpareIdAlertDelay =
        CVarDef.Create("game.spare_id.alert_delay", TimeSpan.FromMinutes(15), CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// Determines if the automatic spare ID process should automatically unlock the cabinet
    /// </summary>
    public static readonly CVarDef<bool> SpareIdAutoUnlock =
        CVarDef.Create("game.spare_id.auto_unlock", true, CVar.SERVERONLY | CVar.ARCHIVE);

    /*
     * Misc.
     */

    /// <summary>
    /// Disables all vision filters for species like Vulpkanin or Harpies. There are good reasons someone might want to disable these.
    /// </summary>
    public static readonly CVarDef<bool> NoVisionFilters =
        CVarDef.Create("accessibility.no_vision_filters", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Disables the fullscreen shader at 700+ glimmer.
    /// </summary>
    public static readonly CVarDef<bool> DisableGlimmerShader =
        CVarDef.Create("accessibility.disable_glimmer_shader", false, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Whether the Shipyard is enabled.
    /// </summary>
    public static readonly CVarDef<bool> Shipyard =
        CVarDef.Create("shuttle.shipyard", true, CVar.SERVERONLY);

    /// <summary>
    /// What year it is in the game. Actual value shown in game is server date + this value.
    /// </summary>
    public static readonly CVarDef<int> YearOffset =
        CVarDef.Create("game.current_year_offset", 550, CVar.SERVERONLY);

    /* Laying down combat */

    /// <summary>
    /// Modifier to apply to all melee attacks when laying down.
    /// Don't increase this above 1...
    /// </summary>
    public static readonly CVarDef<float> LayingDownMeleeMod =
        CVarDef.Create("game.laying_down_melee_mod", 0.25f, CVar.REPLICATED);

    /// <summary>
    ///    Maximum number of characters in objective summaries.
    /// </summary>
    public static readonly CVarDef<int> MaxObjectiveSummaryLength =
        CVarDef.Create("game.max_objective_summary_length", 256, CVar.SERVER | CVar.REPLICATED);

    /* OOC shuttle vote */

    /// <summary>
    /// How long players should have to vote on the round end shuttle being sent
    /// </summary>
    public static readonly CVarDef<TimeSpan> EmergencyShuttleVoteTime =
        CVarDef.Create("shuttle.vote_time", TimeSpan.FromMinutes(1), CVar.SERVER);

    /*
     * Cosmic Cult
     */
    /// <summary>
    /// How much entropy a convert is worth towards the next monument tier.
    /// </summary>
    public static readonly CVarDef<int> CosmicCultistEntropyValue =
        CVarDef.Create("cosmiccult.cultist_entropy_value", 8, CVar.SERVER);

    /// <summary>
    /// How much of the crew the cult is aiming to convert for a tier 3 monument.
    /// </summary>
    public static readonly CVarDef<int> CosmicCultTargetConversionPercent =
        CVarDef.Create("cosmiccult.target_conversion_percent", 40, CVar.SERVER);

    /// <summary>
    /// How long the timer for the cult's stewardship vote lasts.
    /// </summary>
    public static readonly CVarDef<int> CosmicCultStewardVoteTimer =
        CVarDef.Create("cosmiccult.steward_vote_timer", 80, CVar.SERVER);

    /// <summary>
    /// How long we wait before starting the stewardship vote.
    /// </summary>
    public static readonly CVarDef<int> CosmicCultStewardVoteDelayTimer =
        CVarDef.Create("cosmiccult.steward_vote_delay", 25, CVar.SERVER);

    /// <summary>
    /// The delay between the monument getting upgraded to tier 2 and rifts starting to appear. the monument cannot be upgraded again in this time.
    /// </summary>
    public static readonly CVarDef<int> CosmicCultT2RevealDelaySeconds =
        CVarDef.Create("cosmiccult.t2_reveal_delay_seconds", 30, CVar.SERVER);

    /// <summary>
    /// The delay between the monument getting upgraded to tier 3 and the crew learning of that fact. the monument cannot be upgraded again in this time.
    /// </summary>
    public static readonly CVarDef<int> CosmicCultT3RevealDelaySeconds =
        CVarDef.Create("cosmiccult.t3_reveal_delay_seconds", 180, CVar.SERVER);

    /// <summary>
    /// The delay between the monument getting upgraded to tier 3 and the finale starting.
    /// </summary>
    public static readonly CVarDef<int> CosmicCultFinaleDelaySeconds =
        CVarDef.Create("cosmiccult.extra_entropy_for_finale", 1, CVar.SERVER);
}
