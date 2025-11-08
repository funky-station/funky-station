// SPDX-FileCopyrightText: 2025 mkanke-real <mikekanke@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.OOCAuthorship.Components;

[RegisterComponent]
public sealed partial class OocAuthorshipComponent : Component
{
    // <summary>
    //  Name of the author of the media
    // </summary>
    [DataField("oocAuthor")] public string OOCAuthor = "Put Author Here";
}
