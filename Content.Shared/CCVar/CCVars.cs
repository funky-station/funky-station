using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

/// <summary>
/// Contains all the CVars used by content.
/// </summary>
/// <remarks>
/// NOTICE FOR FORKS: Put your own CVars in a separate file with a different [CVarDefs] attribute. RT will automatically pick up on it.
/// </remarks>
[CVarDefs]
public sealed partial class CCVars : CVars
{
    // Only debug stuff lives here.

    /// <summary>
    /// A simple toggle to test <c>OptionsVisualizerComponent</c>.
    /// </summary>
    public static readonly CVarDef<bool> DebugOptionVisualizerTest =
        CVarDef.Create("debug.option_visualizer_test", false, CVar.CLIENTONLY);

        /// <summary>
        /// Set to true to disable parallel processing in the pow3r solver.
        /// </summary>
        public static readonly CVarDef<bool> DebugPow3rDisableParallel =
            CVarDef.Create("debug.pow3r_disable_parallel", true, CVar.SERVERONLY);

        #region Surgery

        public static readonly CVarDef<bool> CanOperateOnSelf =
            CVarDef.Create("surgery.can_operate_on_self", true, CVar.SERVERONLY);

        #endregion

        #region Discord AHelp Reply System

        /// <summary>
        ///     If an admin replies to users from discord, should it use their discord role color? (if applicable)
        ///     Overrides DiscordReplyColor and AdminBwoinkColor.
        /// </summary>
        public static readonly CVarDef<bool> UseDiscordRoleColor =
            CVarDef.Create("admin.use_discord_role_color", true, CVar.SERVERONLY);

        /// <summary>
        ///     If an admin replies to users from discord, should it use their discord role name? (if applicable)
        /// </summary>
        public static readonly CVarDef<bool> UseDiscordRoleName =
            CVarDef.Create("admin.use_discord_role_name", true, CVar.SERVERONLY);

        /// <summary>
        ///     The text before an admin's name when replying from discord to indicate they're speaking from discord.
        /// </summary>
        public static readonly CVarDef<string> DiscordReplyPrefix =
            CVarDef.Create("admin.discord_reply_prefix", "(DISCORD) ", CVar.SERVERONLY);

        /// <summary>
        ///     The color of the names of admins. This is the fallback color for admins.
        /// </summary>
        public static readonly CVarDef<string> AdminBwoinkColor =
            CVarDef.Create("admin.admin_bwoink_color", "red", CVar.SERVERONLY);

        /// <summary>
        ///     The color of the names of admins who reply from discord. Leave empty to disable.
        ///     Overrides AdminBwoinkColor.
        /// </summary>
        public static readonly CVarDef<string> DiscordReplyColor =
            CVarDef.Create("admin.discord_reply_color", string.Empty, CVar.SERVERONLY);

        /// <summary>
        ///     Use the admin's Admin OOC color in bwoinks.
        ///     If either the ooc color or this is not set, uses the admin.admin_bwoink_color value.
        /// </summary>
        public static readonly CVarDef<bool> UseAdminOOCColorInBwoinks =
            CVarDef.Create("admin.bwoink_use_admin_ooc_color", true, CVar.SERVERONLY);

        #endregion

        /// <summary>
        ///     Goobstation: The amount of time between NPC Silicons draining their battery in seconds.
        /// </summary>
        public static readonly CVarDef<float> SiliconNpcUpdateTime =
            CVarDef.Create("silicon.npcupdatetime", 1.5f, CVar.SERVERONLY);
        /*
        * Blob
        */

        public static readonly CVarDef<int> BlobMax =
            CVarDef.Create("blob.max", 3, CVar.SERVERONLY);

        public static readonly CVarDef<int> BlobPlayersPer =
            CVarDef.Create("blob.players_per", 20, CVar.SERVERONLY);

        public static readonly CVarDef<bool> BlobCanGrowInSpace =
            CVarDef.Create("blob.grow_space", true, CVar.SERVER);
    }
}
