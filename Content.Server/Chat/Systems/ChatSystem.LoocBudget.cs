// SPDX-FileCopyrightText: 2025 duston <66768086+dch-GH@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Funkystation.CCVars;
using Content.Shared.Chat;
using Robust.Shared.Console;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server.Chat.Systems;

// Funkystation
public partial class ChatSystem
{
    private sealed class LoocBudget
    {
        public int Budget;
        public int SentMessages;
        public bool CanSendLooc => SentMessages <= Budget;
    }

    private Dictionary<NetUserId, LoocBudget> _loocBudgets = new();

    private CompletionResult SetLoocBudgetHelper(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(),
                Loc.GetString("cmd-looc-budget-1")),
            2 => CompletionResult.FromHint(Loc.GetString("cmd-looc-budget-2")),
            3 => CompletionResult.FromHint(Loc.GetString("cmd-looc-budget-3")),
            4 => CompletionResult.FromHint(Loc.GetString("cmd-looc-budget-4")),
            _ => CompletionResult.Empty,
        };
    }

    /// <summary>
    /// args: session: user, int: budget, bool: refill budget, bool: inform user
    /// </summary>
    /// <param name="shell"></param>
    /// <param name="argstr"></param>
    /// <param name="args"></param>
    private void SetLoocBudget(IConsoleShell shell, string argstr, string[] args)
    {
        if (!_configurationManager.GetCVar(CCVars_Funky.LoocBudgetEnabled))
            return;

        void SetBudget(ICommonSession player)
        {
            // Get the existing budget or make one or bail out if something goes really wrong.
            if (!_loocBudgets.TryGetValue(player.UserId, out var playerBudget))
            {
                if (_loocBudgets.TryAdd(player.UserId, new LoocBudget() { }))
                    playerBudget = _loocBudgets[player.UserId];
                else
                    return;
            }

            // Set their new budget. (Max LOOC messages they can send per round)
            playerBudget.Budget = int.Parse(args[1]);

            // Reset/refill their budget?
            if (args.Length >= 3 && bool.Parse(args[2]))
                playerBudget.SentMessages = 0;

            // Should we let them know their budget has been changed?
            if (args.Length < 4 || !bool.Parse(args[3]))
                return;

            var messageContent = Loc.GetString("hud-chatbox-looc-budget-refilled", ("count", playerBudget.Budget));
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", messageContent));
            _chatManager.ChatMessageToOne(ChatChannel.Server,
                messageContent,
                wrappedMessage,
                default,
                false,
                player.Channel,
                Color.Red);
        }

        if (args.Length < 2)
        {
            shell.WriteLine(Loc.GetString("cmd-looc-budget-help"));
            return;
        }

        if (args[0] == "all")
        {
            foreach(var session in _playerManager.Sessions)
                SetBudget(session);
        }
        else if (!_playerManager.TryGetSessionByUsername(args[0], out var player))
            shell.WriteLine(Loc.GetString("cmd-looc-budget-error-no-user"));
        else
            SetBudget(player);
    }

    private void TrySendLoocWithBudget(EntityUid source, ICommonSession player, string message, bool hideChat)
    {
        var maxLoocAllowed = _configurationManager.GetCVar(CCVars_Funky.DefaultLoocBudget);
        if (!_loocBudgets.ContainsKey(player.UserId))
        {
            _loocBudgets.TryAdd(player.UserId,
                new LoocBudget()
                    { Budget= maxLoocAllowed, SentMessages = 0});
        }

        if (!_loocBudgets.TryGetValue(player.UserId, out var playerBudget))
            return;

        playerBudget.SentMessages++;
        if (playerBudget.CanSendLooc)
            SendLOOC(source, player, message, hideChat);
        else
        {
            var messageContent = Loc.GetString("hud-chatbox-looc-budget-spent", ("count", playerBudget.Budget));
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", messageContent));
            _chatManager.ChatMessageToOne(ChatChannel.Server,
                message,
                wrappedMessage,
                default,
                false,
                player.Channel,
                Color.Red);
        }

    }
}
