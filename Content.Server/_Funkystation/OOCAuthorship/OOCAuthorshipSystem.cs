// SPDX-FileCopyrightText: 2025 mkanke-real <mikekanke@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Examine;
using Content.Shared.Examine;
using Content.Server.OOCAuthorship.Components;
using Robust.Shared.Utility;


namespace Content.Server.OOCAuthorship
{
    // <summary>
    //This component exists to display an OOC Author in the examine window for things that require it such as with in game books
    // </summary>
    public sealed class OOCAuthorshipSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<OocAuthorshipComponent, ExaminedEvent>(OnExamined);
        }

        private void OnExamined(EntityUid uid,
            OocAuthorshipComponent comp,
            ExaminedEvent args)
        {
            var formattedmessage = new FormattedMessage();
            formattedmessage.AddText($"{Loc.GetString("ooc-author-name")} ");
            var OOCAuthor = comp.OOCAuthor;

            if (string.IsNullOrWhiteSpace(OOCAuthor))
            {
                formattedmessage.AddText($"{Loc.GetString("ooc-author-is-blank")}\n");
                return;
            }

            formattedmessage.AddText($"{OOCAuthor}\n");
            args.AddMessage(formattedmessage, -1);
        }
    }
}
