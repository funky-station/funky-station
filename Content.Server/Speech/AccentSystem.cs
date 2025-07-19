// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 TakoDragon <69509841+BackeTako@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Text.RegularExpressions;
using Content.Server.Chat;
using Content.Server.Chat.Systems;

namespace Content.Server.Speech
{
    public sealed class AccentSystem : EntitySystem
    {
        public static readonly Regex SentenceRegex = new(@"(?<=[\.!\?‽])(?![\.!\?‽])", RegexOptions.Compiled);

        public override void Initialize()
        {
            SubscribeLocalEvent<TransformSpeechEvent>(AccentHandler);
        }

        private void AccentHandler(TransformSpeechEvent args)
        {
            var accentEvent = new AccentGetEvent(args.Sender, args.Message);

            RaiseLocalEvent(args.Sender, accentEvent, true);
            args.Message = accentEvent.Message;
        }
    }

    public sealed class AccentGetEvent : EntityEventArgs
    {
        /// <summary>
        ///     The entity to apply the accent to.
        /// </summary>
        public EntityUid Entity { get; }

        /// <summary>
        ///     The message to apply the accent transformation to.
        ///     Modify this to apply the accent.
        /// </summary>
        public string Message { get; set; }

        public AccentGetEvent(EntityUid entity, string message)
        {
            Entity = entity;
            Message = message;
        }
    }
}
